using Sirenix.OdinInspector;

namespace NM.Config;

public interface IItemDesInPlay;
public interface IItemDesInSpin;
public abstract record ItemDesTriggerBase;
[TypeRegistryItem("结算第1轮时，立即")]
public record ItemDesTriggerEnterSpin : ItemDesTriggerBase, IItemDesInSpin;
[TypeRegistryItem("结算第1轮时, 本建筑满足运营消耗")]
public record ItemDesTriggerBuildingRun : ItemDesTriggerBase, IItemDesInSpin;
[TypeRegistryItem("结算第1轮时, 本事件未完成")]
public record ItemDesTriggerEventMiKanSei : ItemDesTriggerBase, IItemDesInSpin;
[TypeRegistryItem("回合间隙. 本事件完成")]
public record ItemDesTriggerEventKanSei : ItemDesTriggerBase , IItemDesInPlay;
