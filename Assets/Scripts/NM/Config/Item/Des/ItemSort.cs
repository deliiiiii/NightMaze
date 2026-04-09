using Sirenix.OdinInspector;

namespace NM.Config;

public abstract record ItemSortBase
{
    [LabelText("降序")]public bool Descending;
    public int DescendingValue => Descending ? -1 : 1;
}