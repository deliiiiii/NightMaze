using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GeneralPreview;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
// public interface ITSelector<T>
// {
//     ITFilter<T> Filter { get; }
//     ITSorter<T> Sorter { get; }
// }
//
// public interface ITSorter<T>
// {
//     
// }
//
// public interface ITFilter<T>
// {
//     
// }
//
public abstract record ItemSelectorBase
{
    [SerializeReference, LabelText("且物体满足"), PropertyOrder(9996)] public ItemFilterBase? ItemFilter;
    [LabelText("随机选择"), PropertyOrder(9997f), HideIf(nameof(IsSelf))] public bool Random;
    [SerializeReference, LabelText("选择数量上限"), HideIf(nameof(IsSelf)), PropertyOrder(9998)] public IntSelectorBase TakeMax = new IntSelectorInfinite();
    [SerializeReference, LabelText("排序"), HideIf(nameof(IsSelf)), PropertyOrder(9999)] public ItemSortBase? ItemSort;

    bool IsSelf() => this is ItemSelectorSelf;

    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append(ItemFilter);
        sb.Append(Random);
        sb.Append(TakeMax);
        sb.Append(ItemSort);
        return true;
    }
}
[TypeRegistryItem("场上所有物体")]
public record ItemSelectorAllPresentItem : ItemSelectorBase;

[TypeRegistryItem(ItemDesConfig.FromLast + ": 棋子")]
public record ItemSelectorFromResult : ItemSelectorBase;
[TypeRegistryItem("指定物体(拖入: 物体Config)")]
public record ItemSelectorItem : ItemSelectorBase
{
    [LabelText("0_地块列表")][JsonIgnore] public List<GridConfig> GridList = [];
    [LabelText("1_棋子列表")][JsonIgnore] public List<SymbolConfig> SymbolList = [];
    [LabelText("2_建筑列表")][JsonIgnore] public List<BuildingConfig> BuildingList = [];
    [LabelText("3_资源列表")][JsonIgnore] public List<ResourceConfig> ResourceList = [];

    [JsonProperty] List<int> GridIds => GridList.Select(x => x.ID).ToList();
    [JsonProperty] List<int> SymbolIds => SymbolList.Select(x => x.ID).ToList();
    [JsonProperty] List<int> BuildingIds => BuildingList.Select(x => x.ID).ToList();
    [JsonProperty] List<int> ResourceIds => ResourceList.Select(x => x.ID).ToList();
    public ItemSelectorItem(){}
    [JsonConstructor]
    public ItemSelectorItem(int xx)
    {
        GridList = 
        [..
            from id in GridIds
            from config in RefPoolMulti<GridConfig>.AcquireOneOptional(x => x.ID == id).ToIEnumerable()
            select config
        ];
        SymbolList =
        [..
            from id in SymbolIds
            from config in RefPoolMulti<SymbolConfig>.AcquireOneOptional(x => x.ID == id).ToIEnumerable()
            select config
        ];
        BuildingList =
        [..
            from id in BuildingIds
            from config in RefPoolMulti<BuildingConfig>.AcquireOneOptional(x => x.ID == id).ToIEnumerable()
            select config
        ];
        ResourceList =
        [..
            from id in ResourceIds
            from config in RefPoolMulti<ResourceConfig>.AcquireOneOptional(x => x.ID == id).ToIEnumerable()
            select config
        ];
    }

    protected override bool PrintMembers(StringBuilder sb)
    {
        base.PrintMembers(sb);
        sb.Append($"GridList = [{string.Join(", ", GridList.Select(c => c.Name))}], ");
        sb.Append($"SymbolList = [{string.Join(", ", SymbolList.Select(c => c.Name))}], ");
        sb.Append($"BuildingList = [{string.Join(", ", BuildingList.Select(c => c.Name))}], ");
        sb.Append($"ResourceList = [{string.Join(", ", ResourceList.Select(c => c.Name))}], ");
        return true;
    }
}
[TypeRegistryItem("指定物体组(拖入: 物体组Config)")]
public record ItemSelectorItemSet : ItemSelectorBase
{
    [Required("物体组Config不能为空"), LabelText("物体组")]public ItemConfigSet Set = null!;
    
    [JsonProperty] int ConfigSetId => Set.ID;
    public ItemSelectorItemSet(){}
    [JsonConstructor]
    public ItemSelectorItemSet(int xx)
    {
        Set = RefPoolMulti<ItemConfigSet>.AcquireOne(x => x.ID == ConfigSetId)!;
    }

    protected override bool PrintMembers(StringBuilder sb)
    {
        base.PrintMembers(sb);
        sb.Append($"Set =[");
        sb.Append(string.Join(", ", Set.GridList.Select(c => c.Name)));
        sb.Append(string.Join(", ", Set.SymbolList.Select(c => c.Name)));
        sb.Append(string.Join(", ", Set.BuildingList.Select(c => c.Name)));
        sb.Append(string.Join(", ", Set.ResourceList.Select(c => c.Name)));
        sb.Append("]");
        return true;
    }
}

[TypeRegistryItem("自身")]
public record ItemSelectorSelf : ItemSelectorBase;
[TypeRegistryItem("指定标签")]
public record ItemSelectorTag : ItemSelectorBase
{
    [LabelText("通用标签")] public EItemTag ItemTag;
    [LabelText("地形标签")] public EGridTag GridTag;
    [LabelText("棋子标签")] public ESymbolTag SymbolTag;
    [LabelText("资源标签")] public EResourceTag ResourceTag;
    [LabelText("建筑标签")] public EBuildingTag BuildingTag;
    [DebuggerStepThrough]
    public override string ToString()
    {
        var sb = new StringBuilder();
        // sb.Append(ItemTag.ToValues());
        sb.Append(SymbolTag);
        sb.Append(ResourceTag);
        return sb.ToString();
    }
}