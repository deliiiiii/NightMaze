using System.Collections.Generic;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Symbol> this[EttSymbol ettId] => GetEttComOptional<EttSymbol, Symbol>(ettId);
    public partial class Symbol : MyItem<EttSymbol, Symbol, SymbolConfig>
    {
        public Symbol(GamePlaying thisNode, EttSymbol belongEtt, int id, Vector2Int pivotPos) : base(thisNode, belongEtt, id, pivotPos)
        { }
        public override SymbolConfig Config => field ??= RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ID);
    }
}