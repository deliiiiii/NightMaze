using Sirenix.OdinInspector;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

namespace NM.Config;

public abstract class PosSelectorBase;
[TypeRegistryItem("固定数值{0}")]
public class PosSelectorConst : PosSelectorBase
{
    [LabelText("{0}: 数值")] public Vector2Int Value;
}
[TypeRegistryItem(ItemDesConfig.FromLast + ": 位置")]
public class PosSelectorFromResult : PosSelectorBase
{
    [SerializeReference, Required, HideLabel]
    public IItemDesOutPos IOutPos = null!;
}
[TypeRegistryItem("3X3范围")]
public class PosSelector3X3 : PosSelectorBase;

[TypeRegistryItem(ItemDesConfig.FromLast + ": 棋子的位置")]
public class PosSelectorFromResultItem : PosSelectorBase
{
    [SerializeReference, Required, HideLabel]
    public IItemDesOutItem IOutItem = null!;
}

public interface IItemDesOutPos
{
    public PosSelectorBase PosSelectorBase { get; }
}