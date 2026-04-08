using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public partial record Building : MyItem<Building, BuildingConfig>
    {
        public Building(int id, Vector2Int pivotPos) : base(id, pivotPos) {}
        public override BuildingConfig Config => field ??= 
            RefPoolMulti<BuildingConfig>.AcquireOne(c => c.ID == ID)
            ?? RefPoolMulti<BuildingConfig>.AcquireFirst()
            ?? throw new System.Exception($"BuildingConfig 一个配置也没有.");
        public sealed override EItemType ItemType => EItemType.Building;
        protected override PlaySpin.IItem CreateInSpin(PlaySpin spin) => new PlaySpin.Building(spin, this);
    }
}
