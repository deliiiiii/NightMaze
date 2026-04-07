using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    public MyOption<Grid> this[EttGrid ettId] => GetEttComOptional<EttGrid, Grid>(ettId);
    public partial class Grid : MyItem<EttGrid, Grid>
    {
        public Grid(EttGrid belongEtt) : base(belongEtt)
        {
        }
    }
}