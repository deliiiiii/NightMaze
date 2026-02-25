using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

public partial class GamePlaying
{
    public UniAction InitAct => new()
    {
        Invoke = async ct =>
        {
            await Bus.FireAsync(new EvtOnEnter(this), ct);
            await ClearDeckAct.Invoke(ct);
            await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(0), ct);
            await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(1), ct);
            await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), ct);
            await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), ct);
            await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), ct);
            await AddSymbolAct.Invoke(SymbolEtt.CreateSymbol(2), ct);
            while (SymbolDeckList.Count < DeckMax)
            {
                await AddSymbolAct.Invoke(SymbolEtt.CreateEmptySymbol(), ct);
            }

            await LaunchAsync<PlayingIdle>();
        },
        Des = "初始化",
    };
    public record EvtOnEnter(GamePlaying Ctx) : EvtBase;

    public UniAction<SymbolEtt, SymbolEtt> SymbolAddSymbolAct => new()
    {
        Invoke = async (arg1, arg2, ct) =>
        {
            await AddSymbolAct.Invoke(arg2, ct);
            await Bus.FireAsync(new EvtSpinSymbolAddSymbol(arg1, arg2), ct);
        },
        Des = "符号添加符号",
    };
    [TypeRegistryItem("某符号添加某符号时\t(SymbolEtt, SymbolEtt)")]
    public record EvtSpinSymbolAddSymbol(SymbolEtt Symbol, SymbolEtt AddedSymbol) : EvtBase;

    public UniAction ClearDeckAct => new()
    {
        Invoke = async (ct) =>
        {
            foreach (var symbol in SymbolDeckList.ToList())
            {
                await RemoveSymbolAct.Invoke(symbol, ct);
            }
        },
        Des = "清空符号列表"
    };

    public UniAction<SymbolEtt> AddSymbolAct => new()
    {
        Invoke = async (toAdd, ct) =>
        {
            SymbolDeckList.Add(toAdd);
            if (!toAdd.IsEmpty)
            {
                await SymbolDeckList.MyFirst(s => s.IsEmpty)
                    .MatchAsync(async some => await RemoveSymbolAct.Invoke(some, ct), RTask);
            }

            await ShowSymbolRandomlyAct.Invoke(toAdd, ct);
        },
        Des = "添加符号"
    };

    public UniAction<SymbolEtt> RemoveSymbolAct => new()
    {
        Invoke = async (toRemove, ct) =>
        {
            SymbolDeckList.Remove(toRemove);
            if (SymbolDeckList.Count < DeckMax)
            {
                await AddSymbolAct.Invoke(SymbolEtt.CreateEmptySymbol(), ct);
            }
        },
        Des = "移除符号"
    };

    public UniAction<SymbolEtt> ShowSymbolRandomlyAct => new()
    {
        Invoke = async (symbol, ct) =>
        {
            await SymbolShownList
                .Where(s => s.IsEmpty)
                .SelectMany(s => s.Pos.Match(some => [some], Enumerable.Empty<Vector2Int>))
                .ToList()
                .RandomItem()
                .MatchAsync(async some => await ShowSymbolAtAsync.Invoke(symbol, some, ct), RTask);
        },
        Des = "将符号显示在随机一个空位上"
    };

    public UniAction<SymbolEtt, Vector2Int> ShowSymbolAtAsync => new()
    {
        Invoke = async (symbol, pos, ct) =>
        {
            SymbolShownList.Add(symbol);
            symbol.Pos = pos;
            await Bus.FireAsync(new EvtShowSymbolAt(this, symbol, pos), ct);
        },
        Des = "符号显示在某位置"
    };
    [TypeRegistryItem("符号显示在某位置时\t(SymbolEtt, Vector2Int)")]
    public record EvtShowSymbolAt(GamePlaying Ctx, SymbolEtt Symbol, Vector2Int Pos) : EvtBase;
    
    [TypeRegistryItem("某符号每旋转N次\t(SymbolEtt, int)")]
    public record EvtSpinSymbolEverySpinN(SymbolEtt Symbol, int SpinCountN) : EvtBase;

    [TypeRegistryItem("某符号消除某符号时\t(SymbolEtt, SymbolEtt)")]
    public record EvtSpinSymbolDestroySymbol(SymbolEtt Symbol, SymbolEtt DestroyedSymbol) : EvtBase;
    [TypeRegistryItem("某符号移除某符号时\t(SymbolEtt, SymbolEtt)")]
    public record EvtSpinSymbolRemoveSymbol(SymbolEtt Symbol, SymbolEtt RemovedSymbol) : EvtBase;
    [TypeRegistryItem("某符号临时加算时\t(SymbolEtt, int)")]
    public record EvtSpinSymbolPayoutAddTemp(SymbolEtt Symbol, int Add) : EvtBase;
    [TypeRegistryItem("某符号临时乘算时\t(SymbolEtt, int)")]
    public record EvtSpinSymbolPayoutMulTemp(SymbolEtt Symbol, int Mul) : EvtBase;
    [TypeRegistryItem("某符号永久加算时\t(SymbolEtt, int)")]
    public record EvtSpinSymbolPayoutAddPermanent(SymbolEtt Symbol, int Add) : EvtBase;
    [TypeRegistryItem("某符号积攒X时\t(SymbolEtt, int)")]
    public record EvtSpinSymbolStock(SymbolEtt Symbol, int Stock) : EvtBase;
    [TypeRegistryItem("玩家移除某符号时\t(SymbolEtt)")]
    public record EvtSpinPlayerRemoveSymbol(SymbolEtt RemovedSymbol) : EvtBase;
}