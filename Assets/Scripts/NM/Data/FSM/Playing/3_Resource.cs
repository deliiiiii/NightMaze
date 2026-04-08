using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public partial class Resource : MyItem<Resource, ResourceConfig>
    {
        public Resource(int id, Vector2Int pivotPos) : base(id, pivotPos) {}
        public override ResourceConfig Config => field ??= 
            RefPoolMulti<ResourceConfig>.AcquireOne(c => c.ID == ID)
            ?? RefPoolMulti<ResourceConfig>.AcquireFirst()
            ?? throw new System.Exception($"ResourceConfig 一个配置也没有.");
        public sealed override EItemType ItemType => EItemType.Resource;
        protected override PlaySpin.IItem CreateInSpin(PlaySpin spin) => new PlaySpin.Resource(spin, this);
    }
}