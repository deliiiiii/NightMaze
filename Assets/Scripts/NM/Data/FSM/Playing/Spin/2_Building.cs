using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    public MyOption<Building> this[EttBuilding ettId] => GetEttComOptional<EttBuilding, Building>(ettId);
    public partial class Building : MyItem<EttBuilding, Building>
    {
        public Building(EttBuilding belongEtt) : base(belongEtt)
        {
        }
    }
}