using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

public record EvtOnEnterSpin : EvtBase;

[TypeRegistryItem("立即")]
public record EvtSpinImmediateDoSymbol(SymbolEtt Symbol) : EvtBase;
[TypeRegistryItem("某符号每旋转N次\t(SymbolEtt, int)")]
public record EvtSymbolEverySpinN(SymbolEtt Symbol, int SpinCountN) : EvtBase;
[TypeRegistryItem("某符号添加某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolAddSymbol(SymbolEtt Symbol, SymbolEtt AddedSymbol) : EvtBase;
[TypeRegistryItem("某符号消除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolDestroySymbol(SymbolEtt Symbol, SymbolEtt DestroyedSymbol) : EvtBase;
[TypeRegistryItem("某符号移除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolRemoveSymbol(SymbolEtt Symbol, SymbolEtt RemovedSymbol) : EvtBase;
[TypeRegistryItem("某符号临时加算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutAddTemp(SymbolEtt Symbol, int Add) : EvtBase;
[TypeRegistryItem("某符号临时乘算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutMulTemp(SymbolEtt Symbol, int Mul) : EvtBase;
[TypeRegistryItem("某符号永久加算时\t(SymbolEtt, int)")]
public record EvtSymbolPayoutAddPermanent(SymbolEtt Symbol, int Add) : EvtBase;
[TypeRegistryItem("某符号积攒X时\t(SymbolEtt, int)")]
public record EvtSymbolStock(SymbolEtt Symbol, int Stock) : EvtBase;
[TypeRegistryItem("玩家移除某符号时\t(SymbolEtt)")]
public record EvtPlayerRemoveSymbol(SymbolEtt RemovedSymbol) : EvtBase;
[TypeRegistryItem("发现某符号与(当前)某符号相邻时\t(SymbolEtt, SymbolEtt)")]
public record EvtSymbolAdjacentSymbol(SymbolEtt AdjacentSymbol, SymbolEtt Symbol) : EvtBase;

[TypeRegistryItem("某符号旋转到某位置时\t(SymbolEtt, Vector2Int)")]
public record EvtSpinSymbolAt(SymbolEtt Symbol, Vector2Int Pos) : EvtBase;