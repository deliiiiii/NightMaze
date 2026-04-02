using System.Collections.Generic;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Symbol> this[EttSymbol ettId] => GetEttComOptional<EttSymbol, Symbol>(ettId);
    public partial class Symbol : ComBase<EttSymbol, Symbol>
    {
        public Symbol(GamePlaying thisNode, EttSymbol ettSymbol, int id, Vector2Int pivotPos) : base(thisNode, ettSymbol)
        {
            PivotPos = pivotPos;
            ID = id;
        }
        public int ID;
        public Vector2Int PivotPos;
        public List<Vector2Int> DeltaPosList = [Vector2Int.Zero];

        public bool CoverPos(Vector2Int pos)
            => DeltaPosList.Any(d => d + PivotPos == pos);
        public IEnumerable<Vector2Int> CoveredPosList
            => DeltaPosList.Select(d => d + PivotPos);

        public SymbolConfig Config => field ??= RefPoolMulti<SymbolConfig>.AcquireOne(c => c.ID == ID);
        // [JsonConstructor]
        // public Symbol(): base(null!, null!){}
    }
}