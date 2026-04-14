using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using General;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace NM.Config;

public abstract record ItemFilterBase
{
    [SerializeReference, LabelText("且满足"), PropertyOrder(9999)] public ItemFilterBase? ItemFilter;
}
[TypeRegistryItem("是自身")]
public record ItemFilterSelf : ItemFilterBase;
[TypeRegistryItem("不是自身")]
public record ItemFilterNotSelf : ItemFilterBase;
[TypeRegistryItem("属于物体类型: 棋子/建筑/资源/事件/地块")]
public record ItemFilterIsItemType : ItemFilterBase
{
    public EItemType ItemType;
}
[TypeRegistryItem("属于指定标签")]
public record ItemFilterTag : ItemFilterBase
{
    public static ValueDropdownList<EItemTag> GetItemTags() => ItemConfig.GetItemTags();
    public static ValueDropdownList<EGridTag> GetGridTags() => ItemConfig.GetGridTags();
    public static ValueDropdownList<ESymbolTag> GetSymbolTags() => ItemConfig.GetSymbolTags();
    public static ValueDropdownList<EResourceTag> GetResourceTags() => ItemConfig.GetResourceTags();
    public static ValueDropdownList<EBuildingTag> GetBuildingTags() => ItemConfig.GetBuildingTags();
    public static ValueDropdownList<EEventTag> GetEventTags() => ItemConfig.GetEventTags();
    
    [LabelText("通用标签"), ValueDropdown(nameof(GetItemTags), IsUniqueList = true)] public List<EItemTag> ItemTagList = [];
    [LabelText("地形标签"), ValueDropdown(nameof(GetGridTags), IsUniqueList = true)] public List<EGridTag> GridTagList = [];
    [LabelText("棋子标签"), ValueDropdown(nameof(GetSymbolTags), IsUniqueList = true)] public List<ESymbolTag> SymbolTagList = [];
    [LabelText("资源标签"), ValueDropdown(nameof(GetResourceTags), IsUniqueList = true)] public List<EResourceTag> ResourceTagList = [];
    [LabelText("建筑标签"), ValueDropdown(nameof(GetBuildingTags), IsUniqueList = true)] public List<EBuildingTag> BuildingTagList = [];
    [LabelText("事件标签"), ValueDropdown(nameof(GetEventTags), IsUniqueList = true)] public List<EEventTag> EventTagList = [];
    [DebuggerStepThrough]
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (ItemTagList.Any())
            sb.Append(string.Join(",", ItemTagList));
        if (GridTagList.Any())
            sb.Append(string.Join(",", GridTagList));
        if (SymbolTagList.Any())
            sb.Append(string.Join(",", SymbolTagList));
        if (ResourceTagList.Any())
            sb.Append(string.Join(",", ResourceTagList));
        if (BuildingTagList.Any())
            sb.Append(string.Join(",", BuildingTagList));
        if (EventTagList.Any())
            sb.Append(string.Join(",", EventTagList));
        return sb.ToString();
    }
}