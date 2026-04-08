namespace NM.Data;

public partial class PlaySpin
{
    public partial record Resource : MyItem<Resource, GamePlaying.Resource>
    {
        public Resource(PlaySpin spin, GamePlaying.Resource inPlay) : base(spin, inPlay) { }
    }
}