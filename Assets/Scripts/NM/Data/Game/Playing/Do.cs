using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public class DoSymbolAddSymbol : DoWithCtx<GamePlaying, SymbolEtt, SymbolEtt>
{
    public override async UniTask Do(SymbolEtt arg1, SymbolEtt arg2, CancellationToken ct)
    {
        Ctx.AddSymbol(arg2);
        await Bus.FireAsync(new EvtSymbolAddSymbol(){Arg1 = arg1, Arg2 = arg2}, ct);
    }
}