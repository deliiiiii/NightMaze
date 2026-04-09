using Sirenix.OdinInspector;

namespace NM.Config;

public abstract record PosSortBase
{
    [LabelText("降序")]public bool Descending;
    public int DescendingValue => Descending ? -1 : 1;
}
[TypeRegistryItem("从左到右, 从下到上")]
public record PosSortPosLeftDown : PosSortBase;
[TypeRegistryItem("从上到下, 从左到右")]
public record PosSortPosUpLeft : PosSortBase;