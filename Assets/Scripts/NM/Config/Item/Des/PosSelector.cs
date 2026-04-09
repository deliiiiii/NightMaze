using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Config;

public abstract record PosSelectorBase : ICanSelectPos
{
    [field: SerializeReference, LabelText("且坐标满足"), PropertyOrder(9996)] public PosFilterBase? PosFilter { get; init; }
    [field: SerializeReference, LabelText("选择数量上限"), HideIf(nameof(IsConst)), PropertyOrder(9997)] public IntSelectorBase TakeMax { get; init; } = new IntSelectorInfinite();
    [LabelText("随机选择"), HideIf(nameof(IsConst)), PropertyOrder(9998)] public bool Random { get; init; }
    [field: SerializeReference, LabelText("排序"), HideIf(nameof(IsConst)), PropertyOrder(9999)] public PosSortBase? PosSort { get; init; }
    bool IsConst => this is PosSelectorConst;
}
[TypeRegistryItem("固定数值{0}")]
public record PosSelectorConst : PosSelectorBase
{
    [LabelText("{0}: 数值")] public Vector2Int Value;
}

[TypeRegistryItem(ItemDesConfig.FromLast + "位置")]
public record PosSelectorFromResult : PosSelectorBase
{
    [LabelText("{0}: 之前第n步(限1~3步)"),Range(1,3)]public int LastStepCount = 1;
    [SerializeReference, LabelText("从结果中筛选")]public PosSelectorFromResultFilterBase? FromResultFilter;
}
public abstract record PosSelectorFromResultFilterBase;
[TypeRegistryItem("测试.")]
public record PosSelectorFromResultFilterFalse : PosSelectorFromResultFilterBase;


[TypeRegistryItem("自身周围3X3")]
public record PosSelector3X3 : PosSelectorBase;

[TypeRegistryItem(ItemDesConfig.FromLast + "棋子的位置")]
public record PosSelectorFromResultItem : PosSelectorBase
{
    [LabelText("{0}: 之前第n步(限1~3步)"),Range(1,3)]public int LastStepCount = 1;
    [SerializeReference, LabelText("从结果中筛选")]public ItemSelectorFromResultFilterBase? FromResultFilter;
}