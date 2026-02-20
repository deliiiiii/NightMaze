using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;

public delegate UniTask UniAction(CancellationToken token);
public delegate UniTask UniAction1<in TArg1>(TArg1 arg1, CancellationToken token);
public delegate UniTask UniAction2<in TArg1, in TArg2>(TArg1 arg1, TArg2 arg2, CancellationToken token);
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public class ActionDesAttribute(string des) : Attribute
{
    public readonly string Des = des;
}

public delegate UniTask UniEvt<in TArg1>(TArg1 arg1, CancellationToken token) where TArg1 : EvtBase;
[AttributeUsage(AttributeTargets.Property)]
public class UniEvtDesAttribute(string des) : Attribute
{
    public readonly string Des = des;
}