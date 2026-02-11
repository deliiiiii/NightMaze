using System;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;

public abstract class ActionBase;
public abstract class ActionWithEvt<TStateTk, TEvt> : ActionBase
    where TEvt : EvtBase
{
    public abstract UniTask DoAsync();
    public required TStateTk Token;
    protected TEvt Evt => field ??= (Activator.CreateInstance(typeof(TEvt)) as TEvt)!;
}

public abstract class ActionWithEvt1<TCtx, TEvt, TArg1> : ActionBase
    where TEvt : EvtBase1<TArg1>
{
    public abstract UniTask DoAsync();
    public required TCtx Token;
    public required TArg1 Arg1;

    protected TEvt Evt => field ??= (Activator.CreateInstance(typeof(TEvt), args: Arg1) as TEvt)!;
}

public abstract class ActionWithEvt2<TCtx, TEvt, TArg1, TArg2> : ActionBase
    where TEvt : EvtBase2<TArg1, TArg2>
{
    public abstract UniTask DoAsync();
    public required TCtx Ctx;
    public required TArg1 Arg1;
    public required TArg2 Arg2;

    protected TEvt Evt => field ??= (Activator.CreateInstance(typeof(TEvt), args: [Arg1, Arg2]) as TEvt)!;
}