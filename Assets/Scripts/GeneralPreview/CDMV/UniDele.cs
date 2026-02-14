using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeneralPreview;

public abstract class UniDele
{
    public readonly MyOption<int> Timing = None;
    [ReadOnly]public string Des = "None ...";
}

public interface IUniEvt
{
    public void Register();
    public void UnRegister();
}

public class UniAction : UniDele
{
    [HideInInspector]public required Func<CancellationToken, UniTask> DoAsync;
}

public class UniAction1<TArg1> : UniDele
{
    [HideInInspector]public required Func<TArg1, CancellationToken, UniTask> DoAsync;
}

public class UniAction2<TArg1, TArg2> : UniDele
{
    [HideInInspector]public required Func<TArg1, TArg2, CancellationToken, UniTask> DoAsync;
}

public class UniEvt<TArg1> : UniAction1<TArg1>, IUniEvt where TArg1 : EvtBase
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