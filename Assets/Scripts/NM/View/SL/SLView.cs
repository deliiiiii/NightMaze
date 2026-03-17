using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

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
            new EvtClickLoad(curSelected.Data).Forget();
        });
        yield return btnReturn.onClick.EvtBindTo(() =>
        {
            gameObject.SetActiveFalse();
            new EvtClickReturn().Forget();
        });
    }

    UniEvt<MainView.EvtClickBtnOpenSL> OnClickBtnOpenSL => new()
    {
        Invoke = async (evt, ct) =>
        {
            if (!GameRoot.Root.HasCom<GameTitle>())
                return;
            gameObject.SetActiveTrue();
            curSelected = null;
            tranContent.ClearChildren();
            var dataList = await Saver.LoadAllWithVerAsync<GamePlaying>(NameC.SlotFolder, ct);
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
        },
        Des = "(在标题点击了选择存档按钮) 尝试进入存档选择界面"
    };
    
    UniEvt<EvtClickLoad> OnEvtClickLoad => new()
    {
        Invoke = async (evt, ct) =>
        {
            gameObject.SetActiveFalse();
            await GameRoot.Root.AddComAsync(evt.Data, true);
        },
        Des = "(点击了加载按钮) 进入游戏状态"
    };
    public record EvtClickLoad(GamePlaying Data) : EvtForgetBase;
    UniEvt<EvtCLickStartNew> OnEvtCLickStartNew => new()
    {
        Invoke = async (evt, ct) =>
        {
            gameObject.SetActiveFalse();
            var data = new GamePlaying(playerName: evt.PlayerName);
            await GameRoot.Root.AddComAsync(data, false);
        },
        Des = "(点击了新游戏按钮) 创建游戏数据并进入游戏状态"
    };
    public record EvtCLickStartNew(string PlayerName) : EvtForgetBase;
    public record EvtClickReturn : EvtForgetBase;
}