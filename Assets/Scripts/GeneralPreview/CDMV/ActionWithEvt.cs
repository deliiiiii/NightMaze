using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;

public class DoDelay
{
    public required Func<CancellationToken, UniTask> DoAsync;
    public required int DelayTiming;
}
public abstract class DoWithCtx<TCtx>
{
    public abstract UniTask Do(CancellationToken ct);
}

public abstract class DoWithCtx<TCtx, TArg1>
{
    public required TCtx Ctx { get; init; }
    public abstract UniTask Do(TArg1 arg1, CancellationToken ct);
}

public abstract class DoWithCtx<TCtx, TArg1, TArg2>
{
    public required TCtx Ctx { get; init; }
    public abstract UniTask Do(TArg1 arg1, TArg2 arg2, CancellationToken ct);
}