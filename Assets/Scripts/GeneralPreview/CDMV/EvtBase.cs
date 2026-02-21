using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public static class Bus
{ 
    [HideInInspector]
    static readonly Dictionary<Type, List<Delegate>> evtDic = new();
    [ShowInInspector]
    static Dictionary<string, List<EvtShower>> NonViewDic 
        => evtDic
            .Where(pair => !pair.Key.Namespace?.Contains("View") ?? false)
            .ToDictionary(
                pair => pair.Key.GetNiceName(),
                pair => pair.Value.Select(dele => new EvtShower
                {
                    Des = dele.GetMethodInfo().GetCustomAttribute<UniEvtDesAttribute>()?.Des ?? "None ...",
                    // NextList = dele.FireList.ToList(),
                }).ToList());

    public static void FireAndForget<T>(T evt, Func<bool>? withDebug = null) where T : EvtBase
        => FireAsync(evt, CancellationToken.None, withDebug).Forget();
    public static async UniTask FireAsync<T>(T evt, CancellationToken ct, Func<bool>? withDebug = null) where T : EvtBase
    {
        withDebug ??= () => true;
        if(withDebug())
            MyDebug.Log($"Fired - {evt}");
        if (!evtDic.TryGetValue(typeof(T), out var list)) 
            return;
        foreach (var dele in list)
        {
            await ((UniEvt<T>)dele)(evt, ct);
        }
    }
    public static void Register<T>(UniEvt<T> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    public static void UnRegister<T>(UniEvt<T> func) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
            return;
        var index = list.FindIndex(h => h == (Delegate)func);
        if (index == -1) 
            return;
        list.RemoveAt(index);
        if (list.Count == 0)
        {
            evtDic.Remove(typeof(T));
        }
    }
}


public abstract record EvtBase;
public class EvtShower
{
    public string Des = "None...";
    [HideIf(nameof(IsEmpty))]public List<string> NextList = [];
    [HideInInspector] bool IsEmpty => NextList.Count == 0;
}