using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Symbol> this[EttSymbol ettId] => GetEttComOptional<EttSymbol, Symbol>(ettId);
    public partial class Symbol : ComBase<EttSymbol, Symbol>
    {
        public Symbol(GamePlaying thisNode, EttSymbol ettSymbol, int id, Vector2Int pos) : base(thisNode, ettSymbol)
        {
            Pos = pos;
            ID = id;
        }
        public int ID;
        public Vector2Int Pos;
        public SymbolConfig Config => field ??= RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ID);
        // [JsonConstructor]
        // public Symbol(): base(null!, null!){}
    }
}