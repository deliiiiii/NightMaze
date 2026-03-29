using GeneralPreview;

namespace NM.Data;
public partial class GamePlaying
{
    public MyOption<Grid> this[EttGrid ettId] => GetEttComOptional<EttGrid, Grid>(ettId);
    public partial class Grid(Vector2Int pos) : ComBase<EttGrid, Grid>
    {
        public Vector2Int Pos = pos;
    }
}