using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Config;

public abstract record PosSelectorBase
{
    [SerializeReference, LabelText("且坐标满足"), PropertyOrder(9996)] public PosFilterBase? PosFilter;
    [LabelText("随机选择"), HideIf(nameof(IsConst)), PropertyOrder(9997f)] public bool Random;
    [SerializeReference, LabelText("选择数量上限"), HideIf(nameof(IsConst)), PropertyOrder(9998)] public IntSelectorBase TakeMax = new IntSelectorInfinite();
    [SerializeReference, LabelText("排序"), HideIf(nameof(IsConst)), PropertyOrder(9999)] public PosSortBase? PosSort;
    bool IsConst => this is PosSelectorConst;
}
[TypeRegistryItem("固定数值{0}")]
public record PosSelectorConst : PosSelectorBase
{
    [LabelText("{0}: 数值")] public Vector2Int Value;
}

[TypeRegistryItem(ItemDesConfig.FromLast + ": 位置")]
public record PosSelectorFromResult : PosSelectorBase;
[TypeRegistryItem("3X3范围")]
public record PosSelector3X3 : PosSelectorBase;

[TypeRegistryItem(ItemDesConfig.FromLast + ": 棋子的位置")]
public record PosSelectorFromResultItem : PosSelectorBase;