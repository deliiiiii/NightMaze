using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GeneralPreview;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;
public interface ICanSelect
{
    bool Random { get; }
    IntSelectorBase TakeMax { get; }
}

public interface ICanSelectPos : ICanSelect
{
    PosFilterBase? PosFilter { get; }
    PosSortBase? PosSort { get; }
}

public interface ICanSelectItem : ICanSelectPos
{
    ItemFilterBase? ItemFilter { get; }
    ItemSortBase? ItemSort { get; }
}

public abstract record ItemSelectorBase : ICanSelectItem
{
    [field: SerializeReference, LabelText("且物体满足"), PropertyOrder(9994)] public ItemFilterBase? ItemFilter { get; init; }
    [field: SerializeReference, LabelText("物体排序"), HideIf(nameof(IsSelf)), PropertyOrder(9995)] public ItemSortBase? ItemSort { get; init; }
    [field: SerializeReference, LabelText("且物体的位置满足"), PropertyOrder(9996)] public PosFilterBase? PosFilter { get; init; }
    [field: SerializeReference, LabelText("物体的位置排序"), HideIf(nameof(IsSelf)), PropertyOrder(9997)] public PosSortBase? PosSort { get; init; }
    
    [LabelText("随机选择"), HideIf(nameof(IsSelf)), PropertyOrder(9998)] public bool Random { get; set; }
    [field: SerializeReference, LabelText("选择数量上限"), HideIf(nameof(IsSelf)), PropertyOrder(9999)]
    public IntSelectorBase TakeMax {get; } = new IntSelectorInfinite();
    bool IsSelf() => this is ItemSelectorAtPresentSelf;
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
public record ItemSelectorAtPresentAll : ItemSelectorBase;
[TypeRegistryItem("自身")]
public record ItemSelectorAtPresentSelf : ItemSelectorBase;
public abstract record ItemSelectorFromConfigBase : ItemSelectorBase;

[TypeRegistryItem(ItemDesConfig.FromLast + ": 棋子")]
public record ItemSelectorFromResult : ItemSelectorBase
{
    [LabelText("{0}: 之前第n步(限1~3步)"),Range(1,3)]public int LastStepCount = 1;
    [SerializeReference, LabelText("从结果中筛选")]public ItemSelectorFromResultFilterBase? FromResultFilter;
}
public abstract record ItemSelectorFromResultFilterBase;
[TypeRegistryItem("被生成的")]
public record ItemSelectorFromResultFilterSpawned : ItemSelectorFromResultFilterBase;
[TypeRegistryItem("被移除的")]
public record ItemSelectorFromResultFilterRemoved : ItemSelectorFromResultFilterBase;
[TypeRegistryItem("尝试移动且成功了的")]
public record ItemSelectorFromResultFilterSuccessMoved : ItemSelectorFromResultFilterBase;
[TypeRegistryItem("尝试移动且失败了的")]
public record ItemSelectorFromResultFilterFailMoved : ItemSelectorFromResultFilterBase;
[TypeRegistryItem("被加算属性了的")]
public record ItemSelectorFromResultFilterAddPropX : ItemSelectorFromResultFilterBase;
[TypeRegistryItem("被乘算属性了的")]
public record ItemSelectorFromResultFilterMulPropX : ItemSelectorFromResultFilterBase;

[TypeRegistryItem("从配置中选择任意")]
public record ItemSelectorFromConfigCustom : ItemSelectorFromConfigBase
{
    [LabelText("0_地块列表")][JsonIgnore] public List<GridConfig> GridList = [];
    [LabelText("1_棋子列表")][JsonIgnore] public List<SymbolConfig> SymbolList = [];
    [LabelText("2_建筑列表")][JsonIgnore] public List<BuildingConfig> BuildingList = [];
    [LabelText("3_资源列表")][JsonIgnore] public List<ResourceConfig> ResourceList = [];

    [JsonProperty] List<int> GridIds => GridList.Select(x => x.ID).ToList();
    [JsonProperty] List<int> SymbolIds => SymbolList.Select(x => x.ID).ToList();
    [JsonProperty] List<int> BuildingIds => BuildingList.Select(x => x.ID).ToList();
    [JsonProperty] List<int> ResourceIds => ResourceList.Select(x => x.ID).ToList();
    public ItemSelectorFromConfigCustom(){}
    [JsonConstructor]
    public ItemSelectorFromConfigCustom(int xx)
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

[TypeRegistryItem("从配置中选择物体组")]
public record ItemSelectorItemFromConfigSet : ItemSelectorFromConfigBase
{
    [Required("物体组Config不能为空"), LabelText("物体组")]
    public ItemConfigSet Set = null!;

    [JsonProperty] int ConfigSetId => Set.ID;

    public ItemSelectorItemFromConfigSet()
    {
    }

    [JsonConstructor]
    public ItemSelectorItemFromConfigSet(int xx)
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