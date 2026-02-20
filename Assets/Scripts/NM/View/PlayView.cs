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

    protected override void Awake()
    {
        base.Awake();
        Bus.Register(OnEvtOnEnterPlayingAsync);
        Bus.Register(OnEvtClickSpinAsync);
        Bus.Register(OnEvtSpinSymbolAtAsync);
    }

    protected override void OnDestroy()
    {
        Bus.UnRegister(OnEvtOnEnterPlayingAsync);
        Bus.UnRegister(OnEvtClickSpinAsync);
        Bus.UnRegister(OnEvtSpinSymbolAtAsync);
        base.OnDestroy();
    }

    [UniEvtDes("(进入Root - Playing状态时) 清空所有格子")]
    UniEvt<EvtOnEnterPlaying> OnEvtOnEnterPlayingAsync => (evt, ct) =>
    {
        SetAllEmpty(evt.Ctx);
        return UniTask.CompletedTask;
    };
    
    [UniEvtDes("(进入Playing - Spin时) 清空所有格子")]
    UniEvt<EvtOnEnterSpin> OnEvtClickSpinAsync => (evt, ct) =>
    {
        SetAllEmpty(evt.Ctx.BelongFSM);
        return UniTask.CompletedTask;
    };
    
    [UniEvtDes("(某符号旋转到某位置时) 在格子上显示符号")]
    UniEvt<EvtSpinSymbolAt> OnEvtSpinSymbolAtAsync => (evt, ct) =>
    {
        SetSymbolAt(evt.Symbol, evt.Pos);
        return UniTask.CompletedTask;
    };
    
    
    void SetAllEmpty(GamePlaying ctx)
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