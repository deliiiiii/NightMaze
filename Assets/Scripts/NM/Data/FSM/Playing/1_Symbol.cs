using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public partial record Symbol : MyItem<Symbol, SymbolConfig>
    {
        public Symbol(int id, Vector2Int pivotPos) : base(id, pivotPos) {}
        public override SymbolConfig Config => field ??= 
            RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ID)
            ?? RefPoolMulti<SymbolConfig>.AcquireFirst() 
            ?? throw new System.Exception($"SymbolConfig 一个配置也没有.");
        public sealed override EItemType ItemType => EItemType.Symbol;
        protected override PlaySpin.IItem CreateInSpin(PlaySpin spin) => new PlaySpin.Symbol(spin, this);
    }
}