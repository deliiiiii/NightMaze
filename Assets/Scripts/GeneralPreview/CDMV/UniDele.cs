using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

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


public record UniAction<TArg1, TArg2, TArg3> : UniDele
{
    [HideInInspector]public required Func<TArg1, TArg2, TArg3, CancellationToken, UniTask> Invoke { get; init; }
    
    public UniAction Apply(TArg1 arg1, TArg2 arg2, TArg3 arg3) => new()
    {
        Invoke = ct => Invoke(arg1, arg2, arg3, ct),
        Des = $"{Des}. Arg = [{arg1}, {arg2}, {arg3}]",
    };
}

public record UniEvt<TEvt> : UniAction<TEvt>, IDisposable, IUniEvt
    where TEvt : EvtBase
{
    public UniEvt()
    {
        Bus.Register(this);
    }
    public void Dispose()
    {
        Bus.UnRegister(this);
    }

    public UniTask InvokeAsync(EvtBase evt, CancellationToken ct)
    {
        if (evt is TEvt e)
        {
            return Invoke(e, ct);
        }
        return UniTask.CompletedTask;
    }
}

public interface IUniEvt
{
    public static void BindAll(object obj, CancellationToken ct)
    {
        obj.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo =>
            {
                var pType = propertyInfo.PropertyType;
                return pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(UniEvt<>);
            })
            .ForEach(propertyInfo => ((IDisposable)propertyInfo.GetMemberValue(obj)).AddTo(ct));

    }
    public string Des { get; }
    public UniTask InvokeAsync(EvtBase evt, CancellationToken ct);
}