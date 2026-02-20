using GeneralPreview;
using Sirenix.OdinInspector;

namespace NM.Data;

public record EvtOnEnterSpin(PlayingSpin Ctx) : EvtBase;

[TypeRegistryItem("立即")]
public record EvtSpinImmediateDoSymbol(SymbolEtt Symbol) : EvtBase;
[TypeRegistryItem("某符号每旋转N次\t(SymbolEtt, int)")]
public record EvtSpinSymbolEverySpinN(SymbolEtt Symbol, int SpinCountN) : EvtBase;
[TypeRegistryItem("某符号添加某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSpinSymbolAddSymbol(SymbolEtt Symbol, SymbolEtt AddedSymbol) : EvtBase;
[TypeRegistryItem("某符号消除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSpinSymbolDestroySymbol(SymbolEtt Symbol, SymbolEtt DestroyedSymbol) : EvtBase;
[TypeRegistryItem("某符号移除某符号时\t(SymbolEtt, SymbolEtt)")]
public record EvtSpinSymbolRemoveSymbol(SymbolEtt Symbol, SymbolEtt RemovedSymbol) : EvtBase;
[TypeRegistryItem("某符号临时加算时\t(SymbolEtt, int)")]
public record EvtSpinSymbolPayoutAddTemp(SymbolEtt Symbol, int Add) : EvtBase;
[TypeRegistryItem("某符号临时乘算时\t(SymbolEtt, int)")]
public record EvtSpinSymbolPayoutMulTemp(SymbolEtt Symbol, int Mul) : EvtBase;
[TypeRegistryItem("某符号永久加算时\t(SymbolEtt, int)")]
public record EvtSpinSymbolPayoutAddPermanent(SymbolEtt Symbol, int Add) : EvtBase;
[TypeRegistryItem("某符号积攒X时\t(SymbolEtt, int)")]
public record EvtSpinSymbolStock(SymbolEtt Symbol, int Stock) : EvtBase;
[TypeRegistryItem("玩家移除某符号时\t(SymbolEtt)")]
public record EvtSpinPlayerRemoveSymbol(SymbolEtt RemovedSymbol) : EvtBase;
[TypeRegistryItem("发现某符号与(当前)某符号相邻时\t(SymbolEtt, SymbolEtt)")]
public record EvtSpinSymbolAdjacentSymbol(SymbolEtt AdjacentSymbol, SymbolEtt Symbol) : EvtBase;

[TypeRegistryItem("某符号放置到某位置时\t(SymbolEtt, Vector2Int)")]
public record EvtSpinSymbolAt(GamePlaying Ctx, SymbolEtt Symbol, Vector2Int Pos) : EvtBase;
[TypeRegistryItem("某符号的最终金钱改变时\t(SymbolEtt)")]
public record EvtSpinSymbolUltimateGiveChanged(SymbolEtt Symbol, long UltimateGive) : EvtBase;