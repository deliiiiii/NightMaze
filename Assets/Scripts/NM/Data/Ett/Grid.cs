using GeneralPreview;

namespace NM.Data;

public record EttGrid : EttBase<EttGrid>;

public partial class GamePlaying
{
    public MyOption<Grid> this[EttGrid ettId] => GetEttCom<EttGrid, Grid>(ettId);
    public partial class Grid(Vector2Int pos) : INodeCom<EttGrid, Grid>
    {
        public Vector2Int Pos = pos;
    }
}