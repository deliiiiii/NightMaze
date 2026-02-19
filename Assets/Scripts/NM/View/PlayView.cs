using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using General.BindData;
using GeneralPreview;
using NM.Data;
using NM.ViewEvt;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class PlayView : ViewBase
{
    [SerializeField] List<SymbolColumnView> symbolColumnList = [];
    public TextMeshProUGUI TxtCoin;
    public Button BtnSpin;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return Binder.FromEvt(BtnSpin.onClick).To(() => Bus.FireAndForget(new EvtClickSpin()));
    }
    protected override IEnumerable<IUniEvt> OnEvt()
    {
        yield return new UniEvt<EvtOnEnterPlaying>
        {
            DoAsync = (evt, ct) =>
            {
                SetAllEmpty();
                return UniTask.CompletedTask;
            },
            Des = "（进入游戏状态时）清空所有格子"
        };
        yield return new UniEvt<EvtOnEnterSpin>
        {
            DoAsync = (evt, ct) =>
            {
                SetAllEmpty();
                return UniTask.CompletedTask;
            },
            Des = "（点击旋转时）清空所有格子"
        };
        yield return new UniEvt<EvtSpinSymbolAt>
        {
            DoAsync = async (evt, ct) =>
            {
                SetSymbolAt(evt.Arg1, evt.Arg2);
                await UniTask.Yield();
            },
            Des = "（某符号旋转到某位置时）在格子上显示符号"
        };
    }
    
    
    void SetAllEmpty()
    {
        _ = from x in Enumerable.Range(Const.SpinFirstID, Const.SpinW)
            from y in Enumerable.Range(Const.SpinFirstID, Const.SpinH)
            select SetSymbolAt(SymbolEtt.CreateEmptySymbol(), new Vector2Int(x, y));
    }

    ValueTuple? SetSymbolAt(SymbolEtt symbolEtt, Vector2Int pos)
    {
        var symbolView = symbolColumnList[pos.X - 1].SymbolList[pos.Y - 1];
        symbolView.SymbolEtt = symbolEtt;
        return null;
    }
}