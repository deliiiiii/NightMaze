using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

[Serializable]
public partial record PlayingSpin : GamePlaying.StateFSM<PlayingSpin>
{
    public override string ToString() => nameof(PlayingSpin);

    public List<IUniAction> DelayAddList = [];
    public List<IUniAction> DelayDestroyList = [];

    protected override async UniTask OnEnterAsync(bool isThisFromLoad)
    {
        BelongFSM.SymbolDeck.ForEach(s =>
        {
            s.AlreadyChecked = false;
            s.TempAdd.Clear();
            s.TempMulti.Clear();
            s.Pos.MatchA(some => s.Pos = None);
        });
        await BelongFSM.SymbolDeck
            .ShuffleTo()
            .Take(Const.SpinW * Const.SpinH)
            .ForEachAsync(async toShow =>
            {
                var shownCount = BelongFSM.SymbolShownSorted.Count;
                var addX = shownCount / Const.SpinH + 1;
                var addY = shownCount % Const.SpinH + 1;
                await new GamePlaying.ActShowSymbolAt
                {
                    Symbol = toShow,
                    Pos = new Vector2Int(addX, addY),
                    @this = BelongFSM
                };
            });
        do
        {
            DelayAddList.Clear();
            await BelongFSM.SymbolShownSorted
                .Where(s => !s.AlreadyChecked)
                .ForEachAsync(async symbol =>
                {
                    symbol.AlreadyChecked = true;
                    await new EvtImmediateDoSymbol(symbol);
                    await BelongFSM.GetAdjacent(symbol).ForEachAsync(async adjacentSymbol =>
                    {
                        var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                        await new EvtSpinSymbolAdjacentSymbol(this, adjacentSymbol, symbol)
                        {
                            GetDebug = () => debug
                        };
                    });
                });
            await DelayAddList.ForEachAsync(async x => await x);
        } while (DelayAddList.Count != 0);

        await BelongFSM.SymbolShownSorted
            .ForEachAsync(async symbol =>
            {
                var pay = symbol.GetUltimateGive();
                if(pay == 0)
                    return;
                await new EvtPay(symbol, pay);
                await new GamePlaying.ActSetCoin
                {
                    Value = BelongFSM.Coin + pay,
                    @this = BelongFSM,
                };
            });
        await BelongFSM.EnterStateAsync(new PlayingIdle(), false);
    }
    
    

    [EvtName("立即执行符号")]
    public record EvtImmediateDoSymbol(SymbolData SymbolData) : EvtBase<SymbolData>(SymbolData);
    [EvtName("结算符号")]
    public record EvtPay(SymbolData SymbolData, long Pay) : EvtBase<SymbolData>(SymbolData);
    [EvtName("发现某符号与当前某符号相邻")]
    public record EvtSpinSymbolAdjacentSymbol(PlayingSpin Ctx, SymbolData AdjacentSymbol, SymbolData Symbol) : EvtBase<PlayingSpin>(Ctx);
}