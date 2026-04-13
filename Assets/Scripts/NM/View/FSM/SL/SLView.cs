using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SLView : ViewBase
{
    [SerializeField] Btn btnLoad;
    [SerializeField] Btn btnReturn;
    [SerializeField] SlotView pfbSlotView;
    [SerializeField] SlotEmptyView pfbSlotEmptyView;
    [SerializeField] Trs tranContent;
    [SerializeField] NewSlotParamView paramView;

    [SerializeField, ReadOnly] SlotView? curSelected;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnLoad.onClick.EvtBindTo(() =>
        {
            if (curSelected == null)
                return;
            gameObject.SetActiveFalse();
            GameRoot.ChangeStateAsync(curSelected.Data, true).Forget();
        });
        yield return btnReturn.onClick.EvtBindTo(() =>
        {
            gameObject.SetActiveFalse();
            TitleViewIns.gameObject.SetActiveTrue();
        });
    }

    public async UniTask OnOpenAsync(CancellationToken ct)
    {
        if (!GameRoot.IsState<GameTitle>())
            return;
        gameObject.SetActiveTrue();
        curSelected = null;
        tranContent.ClearChildren();
        var dataList = await Saver.LoadAllWithVerAsync<GamePlaying>(Const.SaveName.SlotFolder, ct);
        foreach (var data in dataList)
        {
            var ins = Instantiate(pfbSlotView, tranContent);
            ins.Init(data);
            ins.OnClick += () =>
            {
                curSelected?.OnUnSelect();
                curSelected = ins;
                curSelected?.OnSelect();
            };
            await UniTask.WaitForSeconds(0.1f, cancellationToken: ct);
        }
        var insEmpty = Instantiate(pfbSlotEmptyView, tranContent);
        insEmpty.OnClick += () =>
        {
            curSelected?.OnUnSelect();
            curSelected = null;
            paramView.gameObject.SetActiveTrue();
        };
    }
}