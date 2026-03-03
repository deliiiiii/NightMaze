using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class SLView : ViewBase
{
    [SerializeField] Button btnLoad;
    [SerializeField] Button btnReturn;
    [SerializeField] SlotView pfbSlotView;
    [SerializeField] SlotEmptyView pfbSlotEmptyView;
    [SerializeField] Transform tranContent;

    [SerializeField] NewSlotParamView paramView;

    [SerializeField, ReadOnly] SlotView? curSelected;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return Binder.FromEvt(btnLoad.onClick).To(() =>
        {
            if (curSelected == null)
                return;
            Bus.FireAndForget(new EvtClickLoad(curSelected.Data));
        });
    }

    UniEvt<MainView.EvtClickBtnOpenSL> OnClickBtnOpenSL => new()
    {
        Invoke = async (evt, ct) =>
        {
            gameObject.SetActive(true);
            tranContent.ClearChildren();
            var dataList = await Saver.LoadAllAsync<GamePlaying>(NameC.SlotFolder, ct);
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
            insEmpty.OnClick += () => paramView.gameObject.SetActive(true);
        },
        Des = "(在主界面点击了加载按钮) 尝试进入存档选择界面"
    };
    
    UniEvt<EvtClickLoad> OnEvtClickLoad => new()
    {
        Invoke = async (evt, ct) =>
        {
            gameObject.SetActive(false);
            await GameRoot.Root.EnterStateAsync(evt.Data);
        },
        Des = "(点击了加载按钮) 尝试进入游戏状态"
    };
    public record EvtClickLoad(GamePlaying Data) : EvtBase;
}