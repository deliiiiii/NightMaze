using Sirenix.OdinInspector;

namespace NM.Config;

public abstract class ItemSortBase
{
    [LabelText("降序")]public bool Descending;
}

[TypeRegistryItem("位置从左到右, 从下到上")]
public class ItemSortPosLeftDown : ItemSortBase;
[TypeRegistryItem("位置从上到下, 从左到右")]
public class ItemSortPosUpLeft : ItemSortBase;