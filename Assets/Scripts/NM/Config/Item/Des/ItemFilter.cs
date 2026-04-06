using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public abstract class ItemFilterBase
{
    [SerializeReference, LabelText("且满足"), PropertyOrder(9999)] public ItemFilterBase? ItemFilter;
}
[TypeRegistryItem("是自身")]
public class ItemFilterSelf : ItemFilterBase;
[TypeRegistryItem("不是自身")]
public class ItemFilterNotSelf : ItemFilterBase;
[TypeRegistryItem("距离自身{0}格范围内(曼哈顿距离)")]
public class ItemFilterInManDis : ItemFilterBase
{
    [LabelText("{0}: 距离格数") ,MinValue(0)]public int Dis;
}