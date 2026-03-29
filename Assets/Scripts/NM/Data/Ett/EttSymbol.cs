using GeneralPreview;
using Newtonsoft.Json;

namespace NM.Data;

public record EttSymbol : EttBase<EttSymbol>;

public partial class GamePlaying
{
    public MyOption<Symbol> this[EttSymbol ettId] => GetEttCom<EttSymbol, Symbol>(ettId);
    public partial class Symbol(Vector2Int pos) : INodeCom<EttSymbol, Symbol>
    {
        [JsonProperty(ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public Vector2Int Pos = pos;
    }
}