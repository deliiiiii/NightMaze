using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    public MyOption<Resource> this[EttResource ettId] => GetEttComOptional<EttResource, Resource>(ettId);
    public partial class Resource : MyItem<EttResource, Resource>
    {
        public Resource(EttResource belongEtt) : base(belongEtt)
        {
        }
    }
}