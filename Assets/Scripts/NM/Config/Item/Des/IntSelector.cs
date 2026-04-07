using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public abstract class IntSelectorBase;
[TypeRegistryItem("无限大")]
public class IntSelectorInfinite : IntSelectorBase;
[TypeRegistryItem("固定数值{0}")]
public class IntSelectorConst : IntSelectorBase
{
    [LabelText("{0}: 数值")] public int Value;
}

[TypeRegistryItem("每有一个物体{0}, 获得{1}数值")]
public class IntSelectorSumBy : IntSelectorBase
{
    [SerializeReference, LabelText("{0}: 来源物体")] public ItemSelectorBase ItemSelector = new ItemSelectorSelf();
    [LabelText("{1}: 数值倍数")] public int Value;
}