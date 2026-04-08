using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    public partial class Building : MyItem<Building, GamePlaying.Building>
    {
        public Building(PlaySpin spin, GamePlaying.Building inPlay) : base(spin, inPlay) { }
    }
}