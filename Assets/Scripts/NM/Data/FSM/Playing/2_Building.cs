using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Building> this[EttBuilding ettId] => GetEttComOptional<EttBuilding, Building>(ettId);
    public partial class Building : MyItem<EttBuilding, Building, BuildingConfig>
    {
        public Building(EttBuilding belongEtt, int id, Vector2Int pivotPos) : base(belongEtt, id, pivotPos) {}
        public override BuildingConfig Config => field ??= 
            RefPoolMulti<BuildingConfig>.AcquireOne(c => c.ID == ID)
            ?? RefPoolMulti<BuildingConfig>.AcquireFirst()
            ?? throw new System.Exception($"BuildingConfig 一个配置也没有.");
        public sealed override EItemType ItemType => EItemType.Building;
    }
}
