using System;
using System.Collections.Generic;
using GeneralPreview;
using System.Linq;
using General;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using EnumExt = GeneralPreview.EnumExt;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Config;
public class ItemConfig : ConfigMulti<ItemConfig>
{
    public override string PrefixName => ItemType switch
    {
        EItemType.Symbol => "Symbol",
        EItemType.Building => "Building",
        EItemType.Resource => "Resource",
        EItemType.Event => "Event",
        EItemType.Grid => "Grid",
        _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(EItemType)}类型: {ItemType}.")
    };
    // ReSharper disable once StaticMemberInGenericType
    protected static ItemTypeResourceMgr Mgr => field ??= RefPoolSingle<ItemTypeResourceMgr>.Acquire();
    [Header("—— 通用配置 ——")]
    [LabelText("风味文本")] public string FlavorDes = string.Empty;
    [LabelText("稀有度")] public ERarity Rarity;
    [Required, LabelText("占据位置")] public ItemPos Pos = new ItemPosRectangle();
    [Required, LabelText("通用Tag"), ValueDropdown(nameof(GetItemTags), IsUniqueList = true)] public List<EItemTag> TagList = [];

    [LabelText("可拖动")] public bool CanDrag;
    [LabelText("类型"), OnValueChanged(nameof(OnItemTypeChanged))] public EItemType ItemType;
    void OnItemTypeChanged()
    {
        OnNameAndIdChanged();
        CanDrag = IsSymbol;
    }
    
    public bool IsGrid => ItemType == EItemType.Grid;
    public bool IsSymbol => ItemType == EItemType.Symbol;
    public bool IsBuilding => ItemType == EItemType.Building;
    public bool IsResource => ItemType == EItemType.Resource;
    public bool IsEvent => ItemType == EItemType.Event;
    public bool IsBuildingOrEvent => IsBuilding || IsEvent;
    [ShowIf(nameof(IsGrid)), Required, LabelText("地形Tag"), ValueDropdown(nameof(GetGridTags), IsUniqueList = true)] public List<EGridTag> GridTagList = [];
    
    [ShowIf(nameof(IsSymbol)), Required, LabelText("棋子Tag"), ValueDropdown(nameof(GetSymbolTags), IsUniqueList = true)] public List<ESymbolTag> SymbolTagList = [];
    [ShowIf(nameof(IsSymbol)), Required, LabelText("棋子: 属性白值")] public Dictionary<EPropType, long> SymbolPropValueList = [];
    
    [ShowIf(nameof(IsBuilding)), Required, LabelText("建筑Tag"), ValueDropdown(nameof(GetBuildingTags), IsUniqueList = true)] public List<EBuildingTag> BuildingTagList = [];
    [ShowIf(nameof(IsResource)), Required, LabelText("资源Tag"), ValueDropdown(nameof(GetResourceTags), IsUniqueList = true)] public List<EResourceTag> ResourceTagList = [];
    [ShowIf(nameof(IsEvent)), Required, LabelText("事件Tag"), ValueDropdown(nameof(GetEventTags), IsUniqueList = true)] public List<EEventTag> EventTagList = [];
    [ShowIf(nameof(IsBuildingOrEvent)), Required, LabelText("建筑/事件: 交互消耗")]public Dictionary<EPropType, long> BuildPropValueList = [];
    [ShowIf(nameof(IsBuilding)), Required, LabelText("建筑: 运营消耗")]public Dictionary<EPropType, long> RunPropValueList = [];
    
    [Required, LabelText("词条列表")] public List<ItemDesConfig> DesList = [];
    
    public List<DetailTagInfo> DetailTagInfos =>
    [
        ..TagList.Select(t => Mgr.ItemDic[t]),
        ..GridTagList.Select(t => Mgr.GridDic[t]),
        ..SymbolTagList.Select(t =>Mgr.SymbolDic[t]),
        ..BuildingTagList.Select(t => Mgr.BuildingDic[t]),
        ..ResourceTagList.Select(t => Mgr.ResourceDic[t]),
        ..EventTagList.Select(t => Mgr.EventDic[t]),
    ];
    public int Order => (int)ItemType;
        // switch
    // {
        // EItemType.Symbol => 1,
        // EItemType.Building => 2,
        // EItemType.Resource => 3,
        // EItemType.Event => 4,
        // EItemType.Grid => 5,
        // _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(EItemType)}类型: {ItemType}.")
    // };

    public static ValueDropdownList<EItemType> GetItemTypes() => GetEnumDropdownList<EItemType>();
    public static ValueDropdownList<EItemTag> GetItemTags() => GetEnumDropdownList<EItemTag>();
    public static ValueDropdownList<EGridTag> GetGridTags() => GetEnumDropdownList<EGridTag>();
    public static ValueDropdownList<ESymbolTag> GetSymbolTags() => GetEnumDropdownList<ESymbolTag>();
    public static ValueDropdownList<EBuildingTag> GetBuildingTags() => GetEnumDropdownList<EBuildingTag>();
    public static ValueDropdownList<EResourceTag> GetResourceTags() => GetEnumDropdownList<EResourceTag>();
    public static ValueDropdownList<EEventTag> GetEventTags() => GetEnumDropdownList<EEventTag>();

    static ValueDropdownList<T> GetEnumDropdownList<T>() where T : Enum 
        => [..EnumExt.GetValues<T>().Select(t => new ValueDropdownItem<T>(t.GetLabelText(), t))];
}

public enum ERarity
{
    [LabelText("普通")] Common,
    [LabelText("罕见")] UnCommon,
    [LabelText("稀有")] Rare,
    [LabelText("非常稀有")] VeryRare,
}

public enum EItemType
{
    [LabelText("无")]None = 0,
    [LabelText("1_棋子")]Symbol = 1,
    [LabelText("2_建筑")]Building = 2,
    [LabelText("3_资源")]Resource = 3,
    [LabelText("4_事件")]Event = 4,
    [LabelText("5_地块")]Grid = 5,
}
public enum EItemTag
{
    [LabelText("自然")]Natural = 0,
    [LabelText("异象")]Anomaly = 1,
}
public enum EGridTag
{
    [LabelText("肥沃")]Rich = 0,
}
public enum ESymbolTag
{
    [LabelText("人类")]People    = 0,
    [LabelText("机械")]Mechanics = 1,
}
public enum EBuildingTag
{
    [LabelText("庇护所")]Shelter = 0,
    [LabelText("科研")]Science = 1,
}
public enum EResourceTag
{
    [LabelText("作物")]Crops = 0,
    [LabelText("生物质")]Biomass = 1,
}
public enum EEventTag
{
    [LabelText("测试")] Test = 42,
    [LabelText("测试2")] Test2 = 43,
}
public abstract class ItemPos
{
    public abstract IEnumerable<Vector2Int> GetDeltaPos();
}
[PublicAPI, TypeRegistryItem("矩形")]
public class ItemPosRectangle : ItemPos
{
    [LabelText("宽")]public int Length = 1;
    [LabelText("高")]public int Height = 1;
    public override IEnumerable<Vector2Int> GetDeltaPos()
    {
        return from i in Enumerable.Range(0, Length)
                from j in Enumerable.Range(0, Height)
                select new Vector2Int(i, j);
    }
}
[PublicAPI, TypeRegistryItem("自定义")]
public class ItemPosCustom : ItemPos
{
    [LabelText("相对坐标列表"), OnValueChanged(nameof(OnChanged))]public List<Vector2Int> DeltaPosList = [new(0,0)];
    public override IEnumerable<Vector2Int> GetDeltaPos() => DeltaPosList;

    void OnChanged()
    {
        DeltaPosList ??= [];
    }
}