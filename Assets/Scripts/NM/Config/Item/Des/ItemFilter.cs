using System;
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
[TypeRegistryItem("距离自身{0}格范围内(曼哈顿距离)")]
public record ItemFilterInManDis : ItemFilterBase
{
    [LabelText("{0}: 距离格数") ,MinValue(0)]public int Dis;
}
[TypeRegistryItem("在自身周围3x3格范围内")]
public record ItemFilterIn3X3 : ItemFilterBase;

[TypeRegistryItem("属于地块/棋子/建筑/资源")]
public record ItemFilterIsItemType : ItemFilterBase
{
    public EItemType ItemType;
}
[Flags]
public enum EItemType
{
    [LabelText("0_地块")]Grid = 1 << 1,
    [LabelText("1_棋子")]Symbol = 1 << 2,
    [LabelText("2_建筑")]Building = 1 << 3,
    [LabelText("3_资源")]Resource = 1 << 4,
}