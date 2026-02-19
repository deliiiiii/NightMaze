using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeneralPreview;
[HideReferenceObjectPicker]
public abstract record UniDele
{
    // public readonly MyOption<int> Timing = None;
    [ReadOnly]public string Des = "None ...";
}

public interface IUniEvt
{
    public void Register();
    public void UnRegister();
}

public record UniAction : UniDele
{
    [HideInInspector]public required Func<CancellationToken, UniTask> DoAsync;
}

public record UniAction1<TArg1> : UniDele
{
    [HideInInspector]public required Func<TArg1, CancellationToken, UniTask> DoAsync;
}

public record UniAction2<TArg1, TArg2> : UniDele
{
    [HideInInspector]public required Func<TArg1, TArg2, CancellationToken, UniTask> DoAsync;
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