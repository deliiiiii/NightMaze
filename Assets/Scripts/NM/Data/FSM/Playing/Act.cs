using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;
[ActContainer]
public partial class GamePlaying
{
    [Obsolete("符号添加符号")]
    async UniTask SymbolAddSymbolAsync(SymbolData subjectSymbol, SymbolData addedSymbol, CancellationToken ct)
    {
        await new ActAddSymbol(this) {ToAdd = addedSymbol};
    }
    [Obsolete("获得金币")]
    UniTask GainCoinAsync(long value, CancellationToken ct)
    {
        Coin += value;
        return UniTask.CompletedTask;
    }
    [Obsolete("清空符号列表")]
    UniTask ClearDeckAsync(CancellationToken ct) =>
        (
            from symbol in symbolDeckList 
            select new ActRemoveSymbol(this) {ToRemove = symbol, ShouldAddEmpty = false })
        .SeqAwait();

    [Obsolete("添加符号")]
    async UniTask AddSymbolAsync(SymbolData toAdd, CancellationToken ct)
    {
        symbolDeckList.Add(toAdd);
        await toAdd.OnAddAsync(false);
        await new ActShowSymbolRandomly(this) { Symbol = toAdd };
        if(symbolDeckList.Count > DeckMax)
        {
            await (from toRemove in symbolDeckList.FirstOptional(s => s.IsEmpty && s.Pos == toAdd.Pos)
                select new ActRemoveSymbol(this){ ToRemove = toRemove, ShouldAddEmpty = false }).ToUniTask();
        }
    }
    [Obsolete("移除符号")]
    async UniTask RemoveSymbolAsync(SymbolData toRemove, bool shouldAddEmpty, CancellationToken ct)
    {
        toRemove.OnRemove();
        symbolDeckList.Remove(toRemove);
        if (symbolDeckList.Count < DeckMax && shouldAddEmpty)
        {
            await new ActAddSymbol(this) { ToAdd = SymbolData.CreateEmpty()};
        }
    }

    [Obsolete("将符号显示在随机一个空位上")]
    async UniTask ShowSymbolRandomlyAsync(SymbolData symbol, CancellationToken ct)
    {
        var posList =
            (from _symbol in SymbolShownSorted
                where _symbol.IsEmpty
                from pos in _symbol.Pos.ToIEnumerable()
                select pos).ToList();
        await (from ranPos in posList.RandomItemOptional()
            select new ActShowSymbolAt(this) { Symbol = symbol, Pos = ranPos }).ToUniTask();
    }
        
    [Obsolete("将符号显示在某位置上")]
    UniTask ShowSymbolAtAsync(SymbolData symbol, Vector2Int pos, CancellationToken ct)
    {
        symbol.Pos = pos;
        return UniTask.CompletedTask;
    }
    // [TypeRegistryItem("某符号每旋转N次\t(SymbolData, int)")]
    // public record EvtSpinSymbolEverySpinN(SymbolData Symbol, int SpinCountN) : EvtBase;
    // [TypeRegistryItem("某符号消除某符号时\t(SymbolData, SymbolData)")]
    // public record EvtSpinSymbolDestroySymbol(SymbolData Symbol, SymbolData DestroyedSymbol) : EvtBase;
    // [TypeRegistryItem("某符号移除某符号时\t(SymbolData, SymbolData)")]
    // public record EvtSpinSymbolRemoveSymbol(SymbolData Symbol, SymbolData RemovedSymbol) : EvtBase;
    // [TypeRegistryItem("某符号临时加算时\t(SymbolData, int)")]
    // public record EvtSpinSymbolPayoutAddTemp(SymbolData Symbol, int Add) : EvtBase;
    // [TypeRegistryItem("某符号临时乘算时\t(SymbolData, int)")]
    // public record EvtSpinSymbolPayoutMulTemp(SymbolData Symbol, int Mul) : EvtBase;
    // [TypeRegistryItem("某符号永久加算时\t(SymbolData, int)")]
    // public record EvtSpinSymbolPayoutAddPermanent(SymbolData Symbol, int Add) : EvtBase;
    // [TypeRegistryItem("某符号积攒X时\t(SymbolData, int)")]
    // public record EvtSpinSymbolStock(SymbolData Symbol, int Stock) : EvtBase;
    // [TypeRegistryItem("玩家移除某符号时\t(SymbolData)")]
    // public record EvtSpinPlayerRemoveSymbol(SymbolData RemovedSymbol) : EvtBase;
}