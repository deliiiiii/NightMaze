using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;
[ActContainer]
public partial record PlayingSpin
{
    [Obsolete("will - 执行已出现未执行符号")]
    UniTask WillCheckUncheckedSymbolAsync(CancellationToken ct)
    {
        if (BelongFSM.SymbolShownSorted.All(s => s.AlreadyChecked))
            return UniTask.CompletedTask;
        InsertHead([..BelongFSM.SymbolShownSorted
            .Where(s => !s.AlreadyChecked)
            .Select(symbol => new ActImmediateCheckSymbol(this) { Symbol = symbol })
            , new ActWillCheckUncheckedSymbol(this)]);
        return UniTask.CompletedTask;
    }

    [Obsolete("will - 给所有出现符号结算")]
    UniTask WillPayShownSymbolAsync(CancellationToken ct)
    {
        var list = from symbol in BelongFSM.SymbolShownSorted
                let pay = symbol.GetUltimateGive() 
                where pay != 0
                select (ICanAwait[])[
                    new EvtSymbolPay(symbol, pay),
                    new GamePlaying.ActGainCoin(BelongFSM) { Value = pay }];
            InsertHead(list.SelectMany(x => x));
        return UniTask.CompletedTask;
    }
    [Obsolete("立即执行符号")]
    async UniTask ImmediateCheckSymbolAsync(SymbolData symbol, CancellationToken ct)
    {
        symbol.AlreadyChecked = true;
        await BelongFSM.GetAdjacent(symbol)
            .ForEachAsync(async adjacentSymbol =>
            {
                await new EvtSpinSymbolAdjacentSymbol(this, adjacentSymbol, symbol)
                    .Debug(!adjacentSymbol.IsEmpty && !symbol.IsEmpty);
            });
    }
    [Obsolete("进入Idle状态")]
    UniTask EnterIdleAsync(CancellationToken ct) => BelongFSM.EnterStateAsync(new PlayingIdle(), false);
    
    [EvtName("发现某符号与当前某符号相邻")]
    public record EvtSpinSymbolAdjacentSymbol(PlayingSpin WhoHasCt, SymbolData AdjacentSymbol, SymbolData Symbol) 
        : EvtBase<PlayingSpin>(WhoHasCt);
}