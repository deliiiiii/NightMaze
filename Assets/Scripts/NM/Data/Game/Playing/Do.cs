using GeneralPreview;

namespace NM.Data;

public class DoSymbolAddSymbol : DoWithCtx<GamePlaying, SymbolEtt, SymbolEtt>
{
    public override UniFunc<SymbolEtt, SymbolEtt> Do => async (arg1, arg2, ct) =>
    {
        Ctx.AddSymbol(arg2);
        await Bus.FireAsync(new EvtSymbolAddSymbol(){Arg1 = arg1, Arg2 = arg2}, ct);
    };
}