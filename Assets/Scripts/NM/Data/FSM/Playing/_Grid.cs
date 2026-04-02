using GeneralPreview;

namespace NM.Data;
public partial class GamePlaying
{
    public MyOption<Grid> this[EttGrid ettId] => GetEttComOptional<EttGrid, Grid>(ettId);
    public partial class Grid(GamePlaying thisNode, EttGrid ettGrid, Vector2Int pos) : ComBase<EttGrid, Grid>(thisNode, ettGrid)
    {
        public Vector2Int Pos = pos;
    }
}