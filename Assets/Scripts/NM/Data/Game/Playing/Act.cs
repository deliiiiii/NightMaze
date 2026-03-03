using System.Linq;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

public partial class GamePlaying
{
    public UniAction InitAct => new()
    {
        InvokeAsync = async ct =>
        {
            await Bus.FireAsync(new EvtOnEnter(this), ct);
            await ClearDeckAct.InvokeAsync(ct);
            await AddSymbolAct.Invoke(SymbolData.Create(0), ct);
            await AddSymbolAct.Invoke(SymbolData.Create(1), ct);
            await AddSymbolAct.Invoke(SymbolData.Create(1), ct);
            await AddSymbolAct.Invoke(SymbolData.Create(1), ct);
            await AddSymbolAct.Invoke(SymbolData.Create(1), ct);
            await AddSymbolAct.Invoke(SymbolData.Create(2), ct);
            while (SymbolDeckList.Count < DeckMax)
            {
                await AddSymbolAct.Invoke(SymbolData.CreateEmpty(), ct);
            }

            await LaunchAsync<PlayingIdle>();
        },
        Des = "初始化",
    };
    public record EvtOnEnter(GamePlaying Ctx) : EvtBase;

    public UniAction<SymbolData, SymbolData> SymbolAddSymbolAct => new()
    {
        Invoke = async (arg1, arg2, ct) =>
        {
            await AddSymbolAct.Invoke(arg2, ct);
            await Bus.FireAsync(new EvtSpinSymbolAddSymbol(arg1, arg2), ct);
        },
        Des = "符号添加符号",
    };
    [TypeRegistryItem("某符号添加某符号时\t(SymbolData, SymbolData)")]
    public record EvtSpinSymbolAddSymbol(SymbolData Symbol, SymbolData AddedSymbol) : EvtBase;

    public UniAction ClearDeckAct => new()
    {
        InvokeAsync = async (ct) =>
        {
            foreach (var symbol in SymbolDeckList.ToList())
            {
                await RemoveSymbolAct.Invoke(symbol, ct);
            }
        },
        Des = "清空符号列表"
    };

    public UniAction<SymbolData> AddSymbolAct => new()
    {
        Invoke = async (toAdd, ct) =>
        {
            SymbolDeckList.Add(toAdd);
            if(SymbolDeckList.Count > DeckMax)
            {
                await GetEmpty().MatchAsync(async some =>
                {
                    await RemoveSymbolAct.Invoke(some, ct);
                }, RTask);
            }
            await ShowSymbolRandomlyAct.Invoke(toAdd, ct);
        },
        Des = "添加符号"
    };

    public UniAction<SymbolData> RemoveSymbolAct => new()
    {
        Invoke = async (toRemove, ct) =>
        {
            SymbolDeckList.Remove(toRemove);
            toRemove.Dispose?.Invoke();
            if (SymbolDeckList.Count < DeckMax)
            {
                await AddSymbolAct.Invoke(SymbolData.CreateEmpty(), ct);
            }
        },
        Des = "移除符号"
    };

    public UniAction<SymbolData> ShowSymbolRandomlyAct => new()
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

    public UniAction<SymbolData, Vector2Int> ShowSymbolAtAsync => new()
    {
        Invoke = async (symbol, pos, ct) =>
        {
            SymbolShownList.Add(symbol);
            symbol.Pos = pos;
            await Bus.FireAsync(new EvtShowSymbolAt(this, symbol, pos), ct);
        },
        Des = "符号显示在某位置"
    };
    [TypeRegistryItem("符号显示在某位置时\t(SymbolData, Vector2Int)")]
    public record EvtShowSymbolAt(GamePlaying Ctx, SymbolData Symbol, Vector2Int Pos) : EvtBase;
    
    [TypeRegistryItem("某符号每旋转N次\t(SymbolData, int)")]
    public record EvtSpinSymbolEverySpinN(SymbolData Symbol, int SpinCountN) : EvtBase;

    [TypeRegistryItem("某符号消除某符号时\t(SymbolData, SymbolData)")]
    public record EvtSpinSymbolDestroySymbol(SymbolData Symbol, SymbolData DestroyedSymbol) : EvtBase;
    [TypeRegistryItem("某符号移除某符号时\t(SymbolData, SymbolData)")]
    public record EvtSpinSymbolRemoveSymbol(SymbolData Symbol, SymbolData RemovedSymbol) : EvtBase;
    [TypeRegistryItem("某符号临时加算时\t(SymbolData, int)")]
    public record EvtSpinSymbolPayoutAddTemp(SymbolData Symbol, int Add) : EvtBase;
    [TypeRegistryItem("某符号临时乘算时\t(SymbolData, int)")]
    public record EvtSpinSymbolPayoutMulTemp(SymbolData Symbol, int Mul) : EvtBase;
    [TypeRegistryItem("某符号永久加算时\t(SymbolData, int)")]
    public record EvtSpinSymbolPayoutAddPermanent(SymbolData Symbol, int Add) : EvtBase;
    [TypeRegistryItem("某符号积攒X时\t(SymbolData, int)")]
    public record EvtSpinSymbolStock(SymbolData Symbol, int Stock) : EvtBase;
    [TypeRegistryItem("玩家移除某符号时\t(SymbolData)")]
    public record EvtSpinPlayerRemoveSymbol(SymbolData RemovedSymbol) : EvtBase;
}