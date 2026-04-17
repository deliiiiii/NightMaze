using System;
using System.Collections.Generic;
using General;
using GeneralPreview;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
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
        EItemType.None => "None",
        _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(EItemType)}类型: {ItemType}.")
    };
    static ItemTypeResourceMgr Mgr => field ??= ConfigLoader.Acquire<ItemTypeResourceMgr>();
    [Header("—— 通用配置 ——")]
    [LabelText("风味文本")] public string FlavorDes = string.Empty;
    [LabelText("稀有度")] public ERarity Rarity;
    [Required, LabelText("占据位置")] public ItemPos Pos = new ItemPosRectangle();
#if UNITY_EDITOR
    [Required, LabelText("通用Tag"), ValueDropdown(nameof(GetItemTags), IsUniqueList = true)]
#endif
    public List<int> TagList = [];
    [LabelText("可拖动")] public bool CanDrag;
    [LabelText("类型"), OnValueChanged(nameof(OnItemTypeChanged))] public EItemType ItemType;
    public void OnItemTypeChanged()
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
#if UNITY_EDITOR
    [ShowIf(nameof(IsGrid)), Required, LabelText("地形Tag"), ValueDropdown(nameof(GetGridTags), IsUniqueList = true)]
#endif
    public List<int> GridTagList = [];
#if UNITY_EDITOR
    [ShowIf(nameof(IsSymbol)), Required, LabelText("棋子Tag"), ValueDropdown(nameof(GetSymbolTags), IsUniqueList = true)]
#endif
    public List<int> SymbolTagList = [];
    [ShowIf(nameof(IsSymbol)), Required, LabelText("棋子: 属性白值")] public Dictionary<EPropType, long> SymbolPropValueList = [];
#if UNITY_EDITOR
    [ShowIf(nameof(IsBuilding)), Required, LabelText("建筑Tag"), ValueDropdown(nameof(GetBuildingTags), IsUniqueList = true)] 
#endif
    public List<int> BuildingTagList = [];
    [ShowIf(nameof(IsBuilding)), Required, LabelText("建筑: 运营消耗")]public Dictionary<EPropType, long> RunPropValueList = [];
#if UNITY_EDITOR
    [ShowIf(nameof(IsResource)), Required, LabelText("资源Tag"), ValueDropdown(nameof(GetResourceTags), IsUniqueList = true)] 
#endif
    public List<int> ResourceTagList = [];
#if UNITY_EDITOR
    [ShowIf(nameof(IsEvent)), Required, LabelText("事件Tag"), ValueDropdown(nameof(GetEventTags), IsUniqueList = true)]
#endif
    public List<int> EventTagList = [];
    [ShowIf(nameof(IsEvent)), Required, LabelText("事件: 奖励列表")] public List<EvtDesResultBase> EvtDesResultList = [];
    
    [ShowIf(nameof(IsBuildingOrEvent)), Required, LabelText("建筑/事件: 交互消耗")]public Dictionary<EPropType, long> BuildPropValueList = [];
    [Required, LabelText("结算时: 词条列表")] public List<ItemDesConfig> DesList = [];

    
    public List<DetailTagInfo> DetailTagInfos =>
    [
        ..TagList.Select(t => Mgr.ItemDic.First(p => p.Key.ID == t).Value),
        ..GridTagList.Select(t => Mgr.GridDic.First(p => p.Key.ID == t).Value),
        ..SymbolTagList.Select(t =>Mgr.SymbolDic.First(p => p.Key.ID == t).Value),
        ..BuildingTagList.Select(t => Mgr.BuildingDic.First(p => p.Key.ID == t).Value),
        ..ResourceTagList.Select(t => Mgr.ResourceDic.First(p => p.Key.ID == t).Value),
        ..EventTagList.Select(t => Mgr.EventDic.First(p => p.Key.ID == t).Value),
    ];
    public int Order => (int)ItemType;
#if UNITY_EDITOR
    static List<ValueDropdownItem<int>> GetItemTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.ItemTag);
    static List<ValueDropdownItem<int>> GetSymbolTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.SymbolTag);
    static List<ValueDropdownItem<int>> GetBuildingTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.BuildingTag);
    static List<ValueDropdownItem<int>> GetResourceTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.ResourceTag);
    static List<ValueDropdownItem<int>> GetEventTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.EventTag);
    static List<ValueDropdownItem<int>> GetGridTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.GridTag);
#endif
    public static List<ValueDropdownItem<EItemType>> GetItemTypes() => GetEnumDropdownList<EItemType>();
    static List<ValueDropdownItem<T>> GetEnumDropdownList<T>() where T : Enum 
        => [..GeneralPreview.EnumExt.GetValues<T>().Select(t => new ValueDropdownItem<T>(t.GetLabelText(), t))];
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
public enum EGridTag
{
    [LabelText("肥沃")]Rich = 0,
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
    public override IEnumerable<Vector2Int> GetDeltaPos() =>
        from i in Enumerable.Range(0, Length)
        from j in Enumerable.Range(0, Height)
        select new Vector2Int(i, j);
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