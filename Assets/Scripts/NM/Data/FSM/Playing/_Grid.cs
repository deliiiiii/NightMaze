using GeneralPreview;
using NM.Config;

namespace NM.Data;
public partial class GamePlaying
{
    public MyOption<Grid> this[EttGrid ettId] => GetEttComOptional<EttGrid, Grid>(ettId);
    public partial class Grid(GamePlaying thisNode, EttGrid ettGrid, int id, Vector2Int pos) : ComBase<EttGrid, Grid>(thisNode, ettGrid)
    {
        public Vector2Int Pos = pos;
        public int ID = id;
        public GridConfig Config => field ??= RefPoolMulti<GridConfig>.AcquireOne(c => c.ID == ID);
    }
}