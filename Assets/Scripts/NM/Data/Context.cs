global using static NM.Data.ContextEtt;
using GeneralPreview;

namespace NM.Data;

public class ContextEtt : EttBase<ContextEtt>
{
    public static readonly ContextEtt Context = new();
}