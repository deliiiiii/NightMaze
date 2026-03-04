using System;
using System.Collections.Generic;
using System.Linq;
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
    public static bool TryClear
    {
        get;
        set
        {
            field = value;
            if (evtDic.Any())
            {
                MyDebug.LogError("上次运行时注册的事件未清除，已自动清除。请检查是否报错，或有事件未正确注销...");
                evtDic.Clear();
            }
        }
    }
    [HideInInspector]
    static readonly Dictionary<Type, List<IUniEvt>> evtDic = new();

    [ShowInInspector]
    static Dictionary<string, List<string>> NonViewDic
        => evtDic
            // .Where(pair => !pair.Key.Namespace?.Contains("View") ?? false)
            .ToDictionary(
                pair => pair.Key.GetNiceName(),
                pair => pair.Value.Select(dele => dele.Des).ToList()
            );

    public static void FireAndForget<T>(T evt, Func<bool>? withDebug = null) where T : EvtBase
        => FireAsync(evt, CancellationToken.None, withDebug).Forget();
    public static async UniTask FireAsync<T>(T evt, CancellationToken ct, Func<bool>? withDebug = null) where T : EvtBase
    {
        withDebug ??= () => true;
        if(withDebug())
            MyDebug.Log($"Fired - {evt}");
        if (!evtDic.TryGetValue(typeof(T), out var list)) 
            return;
        foreach (var dele in list.Where(_ => !ct.IsCancellationRequested).ToList())
        {
            await dele.InvokeAsync(evt, ct);
        }
    }
    internal static void Register<T>(UniEvt<T> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    internal static void UnRegister<T>(UniEvt<T> func) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
            return;
        var index = list.FindIndex(h => (UniEvt<T>)h == func);
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