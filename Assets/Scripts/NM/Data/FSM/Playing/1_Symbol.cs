using System.Collections.Generic;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Symbol> this[EttSymbol ettId] => GetEttComOptional<EttSymbol, Symbol>(ettId);
    public partial class Symbol : MyItem<EttSymbol, Symbol, SymbolConfig>
    {
        public Symbol(EttSymbol belongEtt, int id, Vector2Int pivotPos) : base(belongEtt, id, pivotPos) {}
        public override SymbolConfig Config => field ??= 
            RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ID)
            ?? RefPoolMulti<SymbolConfig>.AcquireFirst() 
            ?? throw new System.Exception($"SymbolConfig 一个配置也没有.");
    }
}