using Sirenix.OdinInspector;

namespace NM.Config;

public abstract record ItemDesTriggerBase;
[TypeRegistryItem("立即")]
public record ItemDesTriggerEnterSpin : ItemDesTriggerBase;
[TypeRegistryItem("本建筑花费了运营消耗")]
public record ItemDesTriggerBuildingRun : ItemDesTriggerBase;
[TypeRegistryItem("本事件未完成")]
public record ItemDesTriggerEventMiKanSei : ItemDesTriggerBase;
