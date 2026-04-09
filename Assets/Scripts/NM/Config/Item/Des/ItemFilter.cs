using System;
using System.Diagnostics;
using System.Text;
using General;
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
[TypeRegistryItem("属于物体类型: 地块/棋子/建筑/资源")]
public record ItemFilterIsItemType : ItemFilterBase
{
    public EItemType ItemType;
}
[TypeRegistryItem("属于指定标签")]
public record ItemFilterTag : ItemFilterBase
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
        if (ItemTag != 0)
            sb.Append(ItemTag);
        if (GridTag != 0)
            sb.Append(GridTag);
        if (SymbolTag != 0)
            sb.Append(SymbolTag);
        if (ResourceTag != 0)
            sb.Append(ResourceTag);
        if (BuildingTag != 0)
            sb.Append(BuildingTag);
        return sb.ToString();
    }
}

[Flags]
public enum EItemType
{
    [LabelText("0_地块")]Grid = 1 << 1,
    [LabelText("1_棋子")]Symbol = 1 << 2,
    [LabelText("2_建筑")]Building = 1 << 3,
    [LabelText("3_资源")]Resource = 1 << 4,
}