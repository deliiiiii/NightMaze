using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Data;
using TMPro;
using UnityEngine;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class PlayView : ViewBase
{
    [SerializeField] List<SymbolColumnView> symbolColumnList = [];
    public TextMeshProUGUI TxtCoin;
    protected override IEnumerable<IFuncWrap> OnEvt()
    {
        yield return EvtBus.Bind(OnEnterPlayingAsync);
        yield return EvtBus.Bind(OnSpinSymbolAtAsync);
    }

    Func<EvtOnEnterPlaying, UniTask> OnEnterPlayingAsync => evt =>
    {
        symbolColumnList.ForEach(column =>
        {
            column.SymbolList.ForEach(symbolView =>
            {
                symbolView.SymbolEtt = SymbolEtt.CreateEmptySymbol();
            });
        });
        return UniTask.CompletedTask;
    };
    
    Func<EvtSpinSymbolAt, UniTask> OnSpinSymbolAtAsync => evt =>
    {
        var symbolView = symbolColumnList[evt.Pos.X - 1].SymbolList[evt.Pos.Y - 1];
        symbolView.SymbolEtt = evt.Symbol;
        return UniTask.CompletedTask;
    };
}