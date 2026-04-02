using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public partial class Resource : ComBase<EttResource, Resource>
    {
        public Resource(GamePlaying thisNode, EttResource belongEtt, int id, Vector2Int pos) : base(thisNode, belongEtt)
        {
            ID = id;
            Pos = pos;
        }
        public int ID;
        public Vector2Int Pos;
        public ResourceConfig Config => field ??= RefPoolMulti<ResourceConfig>.AcquireOne(c => c.ID == ID);
    }
}