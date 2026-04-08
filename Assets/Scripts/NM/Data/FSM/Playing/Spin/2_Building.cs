namespace NM.Data;

public partial class PlaySpin
{
    public partial record Building : MyItem<Building, GamePlaying.Building>
    {
        public Building(PlaySpin spin, GamePlaying.Building inPlay) : base(spin, inPlay) { }
    }
}