using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;
[ActContainer]
public partial record GamePlaying
{
    [Obsolete("符号添加符号")]
    async UniTask SymbolAddSymbolAsync(SymbolData subjectSymbol, SymbolData addedSymbol, CancellationToken ct)
    {
        await new ActAddSymbol{@this = this, ToAdd = addedSymbol};
    }
    [Obsolete("金币变化")]
    UniTask SetCoinAsync(long value, CancellationToken ct)
    {
        Coin = value;
        return UniTask.CompletedTask;
    }
    [Obsolete("清空符号列表")]
    async UniTask ClearDeckAsync(CancellationToken ct)
    {
        foreach (var symbol in symbolDeckList.ToList())
        {
            await new ActRemoveSymbol{ @this = this, ToRemove = symbol, ShouldAddEmpty = false};
        }
    }
    [Obsolete("添加符号")]
    async UniTask AddSymbolAsync(SymbolData toAdd, CancellationToken ct)
    {
        symbolDeckList.Add(toAdd);
        await new ActShowSymbolRandomly { Symbol = toAdd, @this = this };
        if(symbolDeckList.Count > DeckMax)
        {
            await symbolDeckList.MyFirst(s => s.IsEmpty && s.Pos == toAdd.Pos).MatchAsync(async some =>
            {
                await new ActRemoveSymbol { ToRemove = some, @this = this, ShouldAddEmpty = false};
            }, RTask);
        }
    }
    [Obsolete("移除符号")]
    async UniTask RemoveSymbolAsync(SymbolData toRemove, bool shouldAddEmpty, CancellationToken ct)
    {
        symbolDeckList.Remove(toRemove);
        toRemove.Dispose();
        if (symbolDeckList.Count < DeckMax && shouldAddEmpty)
        {
            await new ActAddSymbol { ToAdd = SymbolData.CreateEmpty(), @this = this };
        }
    }
    [Obsolete("将符号显示在随机一个空位上")]
    async UniTask ShowSymbolRandomlyAsync(SymbolData symbol, CancellationToken ct)
    {
        await SymbolShownSorted
            .Where(s => s.IsEmpty)
            .SelectMany(s => s.Pos.Match(some => [some], Enumerable.Empty<Vector2Int>))
            .ToList()
            .RandomItem()
            .MatchAsync(async some => await new ActShowSymbolAt { Symbol = symbol, Pos = some, @this = this }, RTask);
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