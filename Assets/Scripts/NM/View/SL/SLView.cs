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
    [SerializeField] Btn btnLoad;
    [SerializeField] Btn btnReturn;
    [SerializeField] SlotView pfbSlotView;
    [SerializeField] SlotEmptyView pfbSlotEmptyView;
    [SerializeField] Trs tranContent;
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
        yield return Binder.FromEvt(btnReturn.onClick).To(() =>
        {
            gameObject.SetActive(false);
            Bus.FireAndForget(new EvtClickReturn());
        });
    }

    UniEvt<MainView.EvtClickBtnOpenSL> OnClickBtnOpenSL => new()
    {
        Invoke = async (evt, ct) =>
        {
            if (!GameRoot.Root.IsState<GameTitle>())
                return;
            gameObject.SetActive(true);
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
                paramView.gameObject.SetActive(true);
            };
        },
        Des = "(在标题点击了选择存档按钮) 尝试进入存档选择界面"
    };
    
    UniEvt<EvtClickLoad> OnEvtClickLoad => new()
    {
        Invoke = async (evt, ct) =>
        {
            gameObject.SetActive(false);
            await GameRoot.Root.EnterStateAsync(evt.Data, true);
        },
        Des = "(点击了加载按钮) 进入游戏状态"
    };
    public record EvtClickLoad(GamePlaying Data) : EvtBase;
    UniEvt<EvtCLickStartNew> OnEvtCLickStartNew => new()
    {
        Invoke = async (evt, ct) =>
        {
            gameObject.SetActive(false);
            var data = new GamePlaying(playerName: evt.PlayerName);
            await GameRoot.Root.EnterStateAsync(data, false);
        },
        Des = "(点击了新游戏按钮) 创建游戏数据并进入游戏状态"
    };
    public record EvtCLickStartNew(string PlayerName) : EvtBase;
    public record EvtClickReturn : EvtBase;
}