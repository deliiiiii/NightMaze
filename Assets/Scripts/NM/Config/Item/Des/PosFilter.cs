using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public abstract record PosFilterBase;
[TypeRegistryItem("距离物体{0}{1}格范围内(曼哈顿距离)")]
public record PosFilterInManDis : PosFilterBase
{
    [SerializeReference, LabelText("{0}: 指定物体")] public ItemSelectorBase? ItemSelector = new ItemSelectorAtPresentSelf();
    [LabelText("{1}: 距离格数"), MinValue(0)] public int Dis;
}

[TypeRegistryItem("在物体{0}周围3x3格范围内")]
public record PosFilterIn3X3 : PosFilterBase
{
    [SerializeReference, LabelText("{0}: 指定物体")] public ItemSelectorBase? ItemSelector = new  ItemSelectorAtPresentSelf();
}