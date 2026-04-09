using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.Config;

public abstract record IntSelectorBase;
[TypeRegistryItem("固定数值{0}")]
public record IntSelectorConst : IntSelectorBase
{
    [LabelText("{0}: 数值")] public int Value;
}
[TypeRegistryItem("无限大")]
public record IntSelectorInfinite : IntSelectorBase;
[TypeRegistryItem("每有一个物体{0}, 获得{1}数值")]
public record IntSelectorSumBy : IntSelectorBase
{
    [SerializeReference, LabelText("{0}: 来源物体")] public ItemSelectorBase ItemSelector = new ItemSelectorAtPresentSelf();
    [LabelText("{1}: 数值倍数")] public int Value;
}