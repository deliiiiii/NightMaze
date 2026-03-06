using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

public partial class GamePlaying
{
    public record ActSymbolAddSymbol : UniAction
    {
        public override string Des => "符号添加符号";
        public required SymbolData Arg1, Arg2;

        protected override async UniTask InvokeAsync(CancellationToken ct)
        {
            await new ActAddSymbol{Ctx = Ctx, ToAdd = Arg2};
            await Bus.FireAsync(new EvtSymbolAddSymbol(Arg1, Arg2), ct);
        }
    }
    public record EvtSymbolAddSymbol(SymbolData Symbol, SymbolData AddedSymbol) : EvtBase;
    
    public record ActClearDeck : UniAction
    {
        public override string Des => "清空符号列表";

        protected override async UniTask InvokeAsync(CancellationToken ct)
        {
            foreach (var symbol in Ctx.SymbolDeckList.ToList())
            {
                await new ActRemoveSymbol{ Ctx = Ctx, ToRemove = symbol, ShouldAddEmpty = false};
            }
        }
    }
    public record ActAddSymbol : UniAction
    {
        public override string Des => "添加符号";
        public required SymbolData ToAdd;
        protected override async UniTask InvokeAsync(CancellationToken ct)
        {
            Ctx.SymbolDeckList.Add(ToAdd);
            await new ActShowSymbolRandomly { Symbol = ToAdd, Ctx = Ctx };
            if(Ctx.SymbolDeckList.Count > Ctx.DeckMax)
            {
                await Ctx.GetEmptyWhere(s => s.Pos == ToAdd.Pos).MatchAsync(async some =>
                {
                    await new ActRemoveSymbol { ToRemove = some, Ctx = Ctx, ShouldAddEmpty = false};
                }, RTask);
            }
            
        }
    }
    public record ActRemoveSymbol : UniAction
    {
        public override string Des => "移除符号";
        public required SymbolData ToRemove;
        public required bool ShouldAddEmpty;
        protected override async UniTask InvokeAsync(CancellationToken ct)
        {
            Ctx.SymbolDeckList.Remove(ToRemove);
            ToRemove.Dispose();
            if (Ctx.SymbolDeckList.Count < Ctx.DeckMax && ShouldAddEmpty)
            {
                await new ActAddSymbol() { ToAdd = SymbolData.CreateEmpty(), Ctx = Ctx };
            }
        }
    }
    public record ActShowSymbolRandomly : UniAction
    {
        public override string Des => "将符号显示在随机一个空位上";
        public required SymbolData Symbol;
        protected override async UniTask InvokeAsync(CancellationToken ct)
        {
            await Ctx.SymbolShownListSorted
                .Where(s => s.IsEmpty)
                .SelectMany(s => s.Pos.Match(some => [some], Enumerable.Empty<Vector2Int>))
                .ToList()
                .RandomItem()
                .MatchAsync(async some => await new ActShowSymbolAt { Symbol = Symbol, Pos = some, Ctx = Ctx }, RTask);
        }
    }
    public record ActShowSymbolAt : UniAction
    {
        public override string Des => "将符号显示在某位置上";
        public required SymbolData Symbol;
        public required Vector2Int Pos;
        protected override async UniTask InvokeAsync(CancellationToken ct)
        {
            Symbol.Pos = Pos;
            await Bus.FireAsync(new EvtShowSymbolAt(Ctx, Symbol, Pos), ct);
        }
    }
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