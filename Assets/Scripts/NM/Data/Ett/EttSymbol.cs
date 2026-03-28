using GeneralPreview;

namespace NM.Data;

public record EttSymbol : EttBase<EttSymbol>;

public partial class GamePlaying
{
    public MyOption<Symbol> this[EttSymbol ettGrid] => GetEttCom<EttSymbol, Symbol>(ettGrid);
    public partial class Symbol(Vector2Int pos) : EttSymbol.ComBase, INodeCom
    {
        public Vector2Int Pos = pos;
    }
}