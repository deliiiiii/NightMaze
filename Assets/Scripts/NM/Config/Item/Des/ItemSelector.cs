using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public interface ITSelector<T>
{
    ITFilter<T> Filter { get; }
    ITSorter<T> Sorter { get; }
}

public interface ITSorter<T>
{
    
}

public interface ITFilter<T>
{
    
}




public abstract class ItemSelectorBase
{
    [SerializeReference, LabelText("且物体满足"), PropertyOrder(9996)] public ItemFilterBase? ItemFilter;
    [LabelText("随机选择"), PropertyOrder(9997f)] public bool Random;
    [SerializeReference, LabelText("选择数量上限"), HideIf(nameof(IsSelf)), PropertyOrder(9998)] public IntSelectorBase TakeMax = new IntSelectorInfinite();
    [SerializeReference, LabelText("排序"), HideIf(nameof(IsSelf)), PropertyOrder(9999)] public ItemSortBase? ItemSort;

    bool IsSelf() => this is ItemSelectorSelf;
}

[TypeRegistryItem(ItemDesConfig.FromLast + ": 棋子")]
public class ItemSelectorFromResult : ItemSelectorBase
{
    [SerializeReference, Required, HideLabel]
    public IItemDesOutItem IOutItem = null!;
}
[TypeRegistryItem("指定物体(拖入: 物体Config)")]
public class ItemSelectorItem : ItemSelectorBase
{
    [LabelText("0_地块列表")] public List<GridConfig> GridList = [];
    [LabelText("1_棋子列表")] public List<SymbolConfig> SymbolList = [];
    [LabelText("2_建筑列表")] public List<BuildingConfig> BuildingList = [];
    [LabelText("3_资源列表")] public List<ResourceConfig> ResourceList = [];
}
[TypeRegistryItem("指定物体组(拖入: 物体组Config)")]
public class ItemSelectorItemSet : ItemSelectorBase
{
    [Required("物体组Config不能为空"), LabelText("物体组")]public ItemConfigSet Set = null!;
}

[TypeRegistryItem("自身")]
public class ItemSelectorSelf : ItemSelectorBase;
[TypeRegistryItem("指定标签")]
public class ItemSelectorTag : ItemSelectorBase
{
    [LabelText("通用标签")] public EItemTag ItemTag;
    [LabelText("地形标签")] public EGridTag GridTag;
    [LabelText("棋子标签")] public ESymbolTag SymbolTag;
    [LabelText("资源标签")] public EResourceTag ResourceTag;
    [LabelText("建筑标签")] public EBuildingTag BuildingTag;

    public override string ToString()
    {
        var sb = new StringBuilder();
        // sb.Append(ItemTag.ToValues());
        sb.Append(SymbolTag);
        sb.Append(ResourceTag);
        return sb.ToString();
    }
}

public interface IItemDesOutItem
{
    public ItemSelectorBase ItemSelector { get; }
}

