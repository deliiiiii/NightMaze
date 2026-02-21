using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

namespace GeneralPreview;

[HideReferenceObjectPicker]
public abstract record UniDele
{
    // public readonly MyOption<int> Timing = None;
    [ReadOnly, ShowInInspector] public required string Des { get; init; } = "None ...";
    // [ReadOnly]public List<string> FireList { get; init; } = [];
}
public record UniAction : UniDele
{
    [HideInInspector]public required Func<CancellationToken, UniTask> Invoke { get; init; }
}

public record UniAction<TArg1> : UniDele
{
    [HideInInspector]public required Func<TArg1, CancellationToken, UniTask> Invoke { get; init; }
    
    public UniAction Apply(TArg1 arg1) => new()
    {
        Invoke = ct => Invoke(arg1, ct),
        Des = $"{Des}. Arg = [{arg1}]",
    };
}

public record UniAction<TArg1, TArg2> : UniDele
{
    [HideInInspector]public required Func<TArg1, TArg2, CancellationToken, UniTask> Invoke { get; init; }
    
    public UniAction Apply(TArg1 arg1, TArg2 arg2) => new()
    {
        Invoke = ct => Invoke(arg1, arg2, ct),
        Des = $"{Des}. Arg = [{arg1}, {arg2}]",
    };
}

public delegate UniTask UniEvt<in TArg1>(TArg1 arg1, CancellationToken token) where TArg1 : EvtBase;
[AttributeUsage(AttributeTargets.Property)]
public class UniEvtDesAttribute(string des) : Attribute
{
    public readonly string Des = des;
}