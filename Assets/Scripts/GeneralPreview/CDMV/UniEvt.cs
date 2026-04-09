using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
public record UniEvt<TEvt> : IUniEvt
    where TEvt : IEvtBase
{
    [ReadOnly, ShowInInspector] public required string Des { get; init; } = "None ...";
    [HideInInspector]public required Func<TEvt, CancellationToken, UniTask> Invoke { get; init; }
    public UniEvt()
    {
        Bus.Register(this);
    }
    public void Dispose()
    {
        Bus.UnRegister(this);
    }

    public UniTask InvokeAsync(IEvtBase evt, CancellationToken ct) => Invoke((TEvt)evt, ct);
}

public interface IUniEvt : IDisposable
{
    public static void BindAll(object obj, CancellationToken ct)
    {
        _ = (from propertyInfo in obj.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            let pType = propertyInfo.PropertyType
            where pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(UniEvt<>)
            select _ = ((IDisposable)propertyInfo.GetMemberValue(obj)).AddTo(ct)).ToList();
    }
    public string Des { get; }
    public UniTask InvokeAsync(IEvtBase evt, CancellationToken ct);
}