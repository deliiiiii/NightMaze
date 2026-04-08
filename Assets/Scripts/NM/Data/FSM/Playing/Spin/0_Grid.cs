using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
    public partial class Grid : MyItem<Grid, GamePlaying.Grid>
    {
        public Grid(PlaySpin spin, GamePlaying.Grid inPlay) : base(spin, inPlay)
        {
        }
    }
}