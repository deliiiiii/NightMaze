using GeneralPreview;
using NM.Config;

namespace NM.Data;
public partial class GamePlaying
{
    public MyOption<Grid> this[EttGrid ettId] => GetEttComOptional<EttGrid, Grid>(ettId);
    public partial class Grid : MyItem<EttGrid, Grid, GridConfig>
    {
        public Grid(GamePlaying thisNode, EttGrid belongEtt, int id, Vector2Int pivotPos) : base(thisNode, belongEtt, id, pivotPos)
        {
        }
        public override GridConfig Config => field ??= RefPoolMulti<GridConfig>.AcquireOne(c => c.ID == ID);
    }
}