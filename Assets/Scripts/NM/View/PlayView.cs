using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using General.BindData;
using GeneralPreview;
using NM.Data;
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
    protected override IEnumerable<IFuncWrap> OnEvt()
    {
        yield return Bus.Bind(OnEnterPlayingAsync);
        yield return Bus.Bind(OnEnterPlayingSpinAsync);
        yield return Bus.Bind(OnSpinSymbolAtAsync);
    }

    UniFunc<EvtOnEnterPlaying> OnEnterPlayingAsync => (evt, ct) =>
    {
        SetAllEmpty();
        return UniTask.CompletedTask;
    };
    
    UniFunc<EvtOnEnterSpin> OnEnterPlayingSpinAsync => (evt, ct) =>
    {
        SetAllEmpty();
        return UniTask.CompletedTask;
    };
    
    UniFunc<EvtSpinSymbolAt> OnSpinSymbolAtAsync => async (evt, ct) =>
    {
        SetSymbolAt(evt.Arg1, evt.Arg2);
        await UniTask.Yield();
    };

    void SetAllEmpty()
    {
        _ = from x in Enumerable.Range(Const.SpinFirstID, Const.SpinW)
            from y in Enumerable.Range(Const.SpinFirstID, Const.SpinH)
            select SetSymbolAt(SymbolEtt.CreateEmptySymbol(), new Vector2Int(x, y));
    }

    ValueTuple SetSymbolAt(SymbolEtt symbolEtt, Vector2Int pos)
    {
        var symbolView = symbolColumnList[pos.X - 1].SymbolList[pos.Y - 1];
        symbolView.SymbolEtt = symbolEtt;
        return default;
    }
}