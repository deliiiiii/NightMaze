using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;
[ActContainer]
public partial class PlayingSpin
{
    [Obsolete("will - 执行已出现未执行符号")]
    UniTask WillCheckUncheckedSymbolAsync(CancellationToken ct)
    {
        if (BelongData.SymbolShownSorted.All(s => s.AlreadyChecked))
            return UniTask.CompletedTask;
        InsertHead([..
            from symbol in BelongData.SymbolShownSorted 
            where !symbol.AlreadyChecked
            select new ActImmediateCheckSymbol(this) { Symbol = symbol }
            , new ActWillCheckUncheckedSymbol(this)]);
        return UniTask.CompletedTask;
    }

    [Obsolete("will - 给所有出现符号结算")]
    UniTask WillPayShownSymbolAsync(CancellationToken ct)
    {
        var list = 
            from symbol in BelongData.SymbolShownSorted 
            let pay = symbol.GetUltimateGive() 
            where pay != 0 
            from evt in (List<ICanAwait>)[
                new EvtSymbolPay(symbol, pay), 
                new GamePlaying.ActGainCoin(BelongData) { Value = pay }]
            select evt;
        InsertHead(list);
        return UniTask.CompletedTask;
    }
    [Obsolete("立即执行符号")]
    async UniTask ImmediateCheckSymbolAsync(SymbolData symbol, CancellationToken ct)
    {
        symbol.AlreadyChecked = true;
        await (
            from adjacentSymbol in BelongData.GetAdjacent(symbol) 
            select new EvtSpinSymbolAdjacentSymbol(this, adjacentSymbol, symbol).Debug(!adjacentSymbol.IsEmpty && !symbol.IsEmpty))
            .SeqAwait();
    }
    [Obsolete("进入Idle状态")]
    UniTask EnterIdleAsync(CancellationToken ct) => BelongData.AddComAsync(new PlayingIdle(), false);
    
    [EvtName("发现某符号与当前某符号相邻")]
    public record EvtSpinSymbolAdjacentSymbol(PlayingSpin WhoHasCt, SymbolData AdjacentSymbol, SymbolData Symbol) 
        : EvtBase<PlayingSpin>(WhoHasCt);
}