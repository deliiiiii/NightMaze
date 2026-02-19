using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeneralPreview;
[HideReferenceObjectPicker]
public abstract record UniDele
{
    // public readonly MyOption<int> Timing = None;
    [ReadOnly] public string Des { get; init; } = "None ...";
    // [ReadOnly]public List<string> FireList { get; init; } = [];
}

public interface IUniEvt
{
    public void Register();
    public void UnRegister();
}

public record UniAction : UniDele
{
    [HideInInspector]public required Func<CancellationToken, UniTask> DoAsync { get; init; }
    public UniTask this[CancellationToken ct] => DoAsync(ct);
}

public record UniAction1<TArg1> : UniDele
{
    [HideInInspector]public required Func<TArg1, CancellationToken, UniTask> DoAsync { get; init; }
    public UniTask this[TArg1 arg1, CancellationToken ct] => DoAsync(arg1, ct);
}

public record UniAction2<TArg1, TArg2> : UniDele
{
    [HideInInspector]public required Func<TArg1, TArg2, CancellationToken, UniTask> DoAsync { get; init; }
    public UniTask this[TArg1 arg1, TArg2 arg2, CancellationToken ct] => DoAsync(arg1, arg2, ct);
}

public record UniEvt<TArg1> : UniAction1<TArg1>, IUniEvt where TArg1 : EvtBase
{
    public void Register()
    {
        Bus.Register(this);
    }

    public void UnRegister()
    {
        Bus.UnRegister(this);
    }
}