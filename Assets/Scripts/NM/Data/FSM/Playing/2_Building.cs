using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    public MyOption<Building> this[EttBuilding ettId] => GetEttComOptional<EttBuilding, Building>(ettId);
    public partial class Building : MyItem<EttBuilding, Building, BuildingConfig>
    {
        public Building(GamePlaying thisNode, EttBuilding belongEtt, int id, Vector2Int pivotPos) : base(thisNode, belongEtt, id, pivotPos)
        {
        }
        public override BuildingConfig Config => field ??= RefPoolMulti<BuildingConfig>.AcquireOne(c => c.ID == ID);
    }
}
