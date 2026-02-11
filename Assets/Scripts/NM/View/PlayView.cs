using System;
using System.Collections.Generic;
using System.Threading;
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

    Func<EvtOnEnterPlaying, CancellationToken, UniTask> OnEnterPlayingAsync => (evt, ct) =>
    {
        ClearAll();
        return UniTask.CompletedTask;
    };
    
    Func<EvtOnEnterSpin, CancellationToken, UniTask> OnEnterPlayingSpinAsync => (evt, ct) =>
    {
        ClearAll();
        return UniTask.CompletedTask;
    };
    
    Func<EvtSpinSymbolAt, CancellationToken, UniTask> OnSpinSymbolAtAsync => (evt, ct) =>
    {
        var symbolView = symbolColumnList[evt.Arg2.X - 1].SymbolList[evt.Arg2.Y - 1];
        symbolView.SymbolEtt = evt.Arg1;
        return UniTask.CompletedTask;
    };

    void ClearAll()
    {
        symbolColumnList.ForEach(column =>
        {
            column.SymbolList.ForEach(symbolView =>
            {
                symbolView.SymbolEtt = SymbolEtt.CreateEmptySymbol();
            });
        });
    }
}