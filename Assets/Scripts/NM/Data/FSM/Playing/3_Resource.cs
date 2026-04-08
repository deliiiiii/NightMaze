using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Resource> this[EttResource ettId] => GetEttComOptional<EttResource, Resource>(ettId);
    public partial class Resource : MyItem<EttResource, Resource, ResourceConfig>
    {
        public Resource(EttResource belongEtt, int id, Vector2Int pivotPos) : base(belongEtt, id, pivotPos) {}
        public override ResourceConfig Config => field ??= 
            RefPoolMulti<ResourceConfig>.AcquireOne(c => c.ID == ID)
            ?? RefPoolMulti<ResourceConfig>.AcquireFirst()
            ?? throw new System.Exception($"ResourceConfig 一个配置也没有.");
    }
}