using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    public partial class Resource : MyItem<Resource, GamePlaying.Resource>
    {
        public Resource(PlaySpin spin, GamePlaying.Resource inPlay) : base(spin, inPlay) { }
    }
}