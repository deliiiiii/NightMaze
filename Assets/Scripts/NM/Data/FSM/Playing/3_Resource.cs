using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public partial class Resource : MyItem<EttResource, Resource, ResourceConfig>
    {
        public Resource(EttResource belongEtt, int id, Vector2Int pivotPos) : base(belongEtt, id, pivotPos) {}
        public override ResourceConfig Config => field ??= RefPoolMulti<ResourceConfig>.AcquireOne(c => c.ID == ID);
    }
}