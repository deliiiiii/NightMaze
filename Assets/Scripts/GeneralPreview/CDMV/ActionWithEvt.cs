using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;

public class DoDelay
{
    public required UniAction DoAsync;
    public required int DelayTiming;
}
public abstract class DoWithCtx<TCtx>
{
    public abstract UniAction Do { get; }
}

public abstract class DoWithCtx<TCtx, TArg1>
{
    public required TCtx Ctx { get; init; }
    public abstract UniFunc<TArg1> Do { get; }
}

public abstract class DoWithCtx<TCtx, TArg1, TArg2>
{
    public required TCtx Ctx { get; init; }
    public abstract UniFunc<TArg1, TArg2> Do {get; }
}