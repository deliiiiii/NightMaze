using System;
using System.Collections.Generic;
using System.Linq;
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
        });
        
        await Bus.FireAsync(new EvtOnEnter(this), CurCt);
        BelongFSM.SymbolDeck.Where(s => s.Pos.IsSome).ForEach(s => s.Pos = None);
        foreach (var toShow in BelongFSM.SymbolDeck.ShuffleTo())
        {
            var shownCount = BelongFSM.SymbolShownSorted.Count;
            if(shownCount == Const.SpinW * Const.SpinH)
                break;
            var addX = shownCount / Const.SpinH + 1;
            var addY = shownCount % Const.SpinH + 1;
            await new GamePlaying.ActShowSymbolAt
            {
                Symbol = toShow,
                Pos = new Vector2Int(addX, addY),
                @this = BelongFSM
            };
        }

        do
        {
            DelayAddList.Clear();
            foreach (var symbol in BelongFSM.SymbolShownSorted.Where(s => !s.AlreadyChecked))
            {
                symbol.AlreadyChecked = true;
                await Bus.FireAsync(new EvtImmediateDoSymbol(symbol), CurCt);
                foreach (var adjacentSymbol in BelongFSM.GetAdjacent(symbol))
                {
                    var debug = !adjacentSymbol.IsEmpty && !symbol.IsEmpty;
                    await Bus.FireAsync(new EvtSpinSymbolAdjacentSymbol(this, adjacentSymbol, symbol), CurCt, () => debug);
                }
            }
            foreach (var doDelay in DelayAddList)
            {
                await doDelay;
            }
        } while (DelayAddList.Count != 0);

        
        foreach (var symbol in BelongFSM.SymbolShownSorted)
        {
            var pay = symbol.GetUltimateGive();
            if(pay == 0)
                continue;
            await Bus.FireAsync(new EvtPay(symbol, pay), CurCt);
            await new GamePlaying.ActSetCoin
            {
                Value = BelongFSM.Coin + pay,
                @this = BelongFSM,
            };
        }
        MyDebug.Log("Spin End");
        await BelongFSM.EnterStateAsync(new PlayingIdle(), false);
    }
    
    
    public record EvtOnEnter(PlayingSpin Ctx) : EvtBase;

    /// <summary>
    /// 立即
    /// </summary>
    /// <param name="Symbol">被执行的符号</param>
    [EvtName("立即执行符号")]
    public record EvtImmediateDoSymbol(SymbolData Symbol) : EvtBase;
    /// <summary>
    /// 某符号结算时
    /// </summary>
    /// <param name="Symbol"></param>
    [EvtName("结算符号")]
    public record EvtPay(SymbolData Symbol, long Pay) : EvtBase;
    /// <summary>
    /// 发现某符号与当前符号相邻时
    /// </summary>
    /// <param name="Ctx">上下文</param>
    /// <param name="AdjacentSymbol">被发现的符号</param>
    /// <param name="Symbol">当前符号</param>
    [EvtName("发现某符号与当前某符号相邻")]
    public record EvtSpinSymbolAdjacentSymbol(PlayingSpin Ctx, SymbolData AdjacentSymbol, SymbolData Symbol) : EvtBase;
}