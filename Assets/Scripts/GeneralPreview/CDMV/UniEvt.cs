using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
public record UniEvt<TEvt> : IDisposable, IUniEvt
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
            .ForEach(propertyInfo =>
            {
                var iDisposable = (IDisposable)propertyInfo.GetMemberValue(obj);
                iDisposable.AddTo(ct);
            });

    }
    public string Des { get; }
    public UniTask InvokeAsync(IEvtBase evt, CancellationToken ct);
}