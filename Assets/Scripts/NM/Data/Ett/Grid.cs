using GeneralPreview;

namespace NM.Data;

public record EttGrid : EttBase<EttGrid>;

public partial class GamePlaying
{
    public MyOption<Grid> this[EttGrid ettGrid] => GetEttCom<EttGrid, Grid>(ettGrid);
    public partial class Grid(Vector2Int pos) : EttGrid.ComBase, INodeCom
    {
        public Vector2Int Pos = pos;
    }
}