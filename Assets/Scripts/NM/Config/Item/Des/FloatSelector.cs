using Sirenix.OdinInspector;

namespace NM.Config;

public abstract record DoubleSelectorBase;
[TypeRegistryItem("固定数值{0}")]
public record DoubleSelectorConst : DoubleSelectorBase
{
    [LabelText("{0}: 数值")] public double Value;
}