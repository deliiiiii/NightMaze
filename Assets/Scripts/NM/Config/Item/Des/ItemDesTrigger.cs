using Sirenix.OdinInspector;

namespace NM.Config;

public abstract record ItemDesTriggerBase;
[TypeRegistryItem("结算时，立即")]
public record ItemDesTriggerEnterSpin : ItemDesTriggerBase;
[TypeRegistryItem("结算时, 本建筑有人运营")]
public record ItemDesTriggerBuildingRun : ItemDesTriggerBase;
[TypeRegistryItem("结算时, 本事件未完成")]
public record ItemDesTriggerEventMiKanSei : ItemDesTriggerBase;
[TypeRegistryItem("结算时, 本事件完成")]
public record ItemDesTriggerEventKanSei : ItemDesTriggerBase;