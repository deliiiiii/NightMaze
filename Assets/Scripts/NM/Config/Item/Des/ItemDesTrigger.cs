using Sirenix.OdinInspector;

namespace NM.Config;

public abstract record ItemDesTriggerBase;
[TypeRegistryItem("结算时")]
public record ItemDesTriggerEnterSpin : ItemDesTriggerBase;