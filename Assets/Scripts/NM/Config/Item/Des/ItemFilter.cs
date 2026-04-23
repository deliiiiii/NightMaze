using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GeneralPreview;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
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

[TypeRegistryItem("属于物体之一(配置中选择任意)")]
public record ItemFilterIsItemCustom : ItemFilterBase
{
    [LabelText("物体列表"), Required][JsonIgnore] public List<ItemConfig?> ItemList = [];

    [JsonProperty] List<int> ItemIds => ItemList.Where(i => i != null).Select(x => x!.ID).ToList();
    public ItemFilterIsItemCustom(){}
    [JsonConstructor]
    public ItemFilterIsItemCustom(int xx)
    {
        ItemList = 
        [..
            from id in ItemIds
            from config in ConfigLoader.AcquireOptional<ItemConfig>(id).ToIEnumerable()
            select config
        ];
    }
    protected override bool PrintMembers(StringBuilder sb)
    {
        base.PrintMembers(sb);
        sb.Append($"ItemList = [{string.Join(", ", ItemList.Where(i => i != null).Select(c => c!.Name))}], ");
        return true;
    }
}

[TypeRegistryItem("属于物体组之一(配置中选择物体组)")]
public record ItemFilterIsItemSet : ItemFilterBase
{
    [Required("物体组Config不能为空"), LabelText("物体组")]
    public ItemConfigSet? Set;
    [JsonProperty] int ConfigSetId => Set?.ID ?? 0;

    public ItemFilterIsItemSet() { }
    [JsonConstructor]
    public ItemFilterIsItemSet(int xx)
    {
        Set = ConfigLoader.Acquire<ItemConfigSet>(ConfigSetId);
    }

    protected override bool PrintMembers(StringBuilder sb)
    {
        if (Set == null)
            return true;
        base.PrintMembers(sb);
        sb.Append($"Set =[");
        sb.Append(string.Join(", ", Set.ItemList.Where(i => i != null).Select(i => i!.Name)));
        sb.Append("]");
        return true;
    }
}
[TypeRegistryItem("属于物体类型: 棋子/建筑/资源/事件/地块")]
public record ItemFilterIsItemType : ItemFilterBase
{
    public EItemType ItemType;
}
[TypeRegistryItem("属于指定标签")]
public record ItemFilterTag : ItemFilterBase
{
#if UNITY_EDITOR
    static List<ValueDropdownItem<int>> GetItemTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.ItemTag);
    static List<ValueDropdownItem<int>> GetSymbolTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.SymbolTag);
    static List<ValueDropdownItem<int>> GetBuildingTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.BuildingTag);
    static List<ValueDropdownItem<int>> GetResourceTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.ResourceTag);
    static List<ValueDropdownItem<int>> GetEventTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.EventTag);
    static List<ValueDropdownItem<int>> GetGridTags() => Editor.MgrEditor.GetConfigEnumDropDownList(Const.Res.Config.GridTag);
#endif
#if UNITY_EDITOR
    [LabelText("通用标签"), ValueDropdown(nameof(GetItemTags), IsUniqueList = true)] 
#endif
    public List<int> ItemTagList = [];
#if UNITY_EDITOR
    [LabelText("地形标签"), ValueDropdown(nameof(GetGridTags), IsUniqueList = true)]
#endif
    public List<int> GridTagList = [];
#if UNITY_EDITOR
    [LabelText("棋子标签"), ValueDropdown(nameof(GetSymbolTags), IsUniqueList = true)]
#endif
    public List<int> SymbolTagList = [];
#if UNITY_EDITOR
    [LabelText("资源标签"), ValueDropdown(nameof(GetResourceTags), IsUniqueList = true)]
#endif
    public List<int> ResourceTagList = [];
#if UNITY_EDITOR
    [LabelText("建筑标签"), ValueDropdown(nameof(GetBuildingTags), IsUniqueList = true)]
#endif
    public List<int> BuildingTagList = [];
#if UNITY_EDITOR
    [LabelText("事件标签"), ValueDropdown(nameof(GetEventTags), IsUniqueList = true)]
#endif
    public List<int> EventTagList = [];
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