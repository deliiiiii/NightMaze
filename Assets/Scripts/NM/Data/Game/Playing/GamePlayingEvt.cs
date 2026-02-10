using GeneralPreview;

namespace NM.Data;

public record EvtAdjacent(SymbolEtt Symbol, SymbolEtt AdjacentSymbol)
    : EvtBase;

// public record EvtOnEnterSpin : EvtBase;
public record EvtSpinSymbolAt(SymbolEtt Symbol, Vector2Int Pos) : EvtBase;

// public record EvtAdjacent(PlayingSpin Ctx, SymbolEtt Symbol, SymbolEtt AdjacentSymbol)
// : EvtBase<PlayingSpin>(Ctx);