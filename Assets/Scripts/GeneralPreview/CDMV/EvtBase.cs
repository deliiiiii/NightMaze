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
    static readonly Dictionary<Type, List<IUniEvt>> evtDic = [];

    [ShowInInspector]
    static Dictionary<string, List<string>> NonViewDic
        => evtDic
            .Where(pair => !pair.Key.Namespace?.Contains("View") ?? false)
            .ToDictionary(
                pair => pair.Key.GetNiceName(),
                pair => pair.Value.Select(dele => dele.Des).ToList()
            );

    internal static void FireAndForget<T>(T evt, bool debug = true) where T : IEvtBase
        => FireAsync(evt, CancellationToken.None, debug).Forget();
    internal static async UniTask FireAsync<T>(T evt, CancellationToken ct, bool debug = true) where T : IEvtBase
    {
        var evtType = evt.GetType();
        if (BusDisposable.IsMute(evtType.FullName))
            return;
        if (debug)
        {
            var attr = evtType.GetCustomAttribute<EvtNameAttribute>();
            var typeName = attr != null ? $"{attr.Name}" : evtType.GetNiceName();
            var details = evt.ToString();
            var leftBracketIndex = details.IndexOf('{');
            var rightBracketIndex = details.IndexOf('}');
            MyDebug.Log($"Fired - {typeName} {details.Substring(leftBracketIndex, rightBracketIndex - leftBracketIndex + 1)}");
        }
        if (!evtDic.TryGetValue(evtType, out var list)) 
            return;
        foreach (var dele in list.Where(_ => !ct.IsCancellationRequested).ToList())
        {
            await dele.InvokeAsync(evt, ct);
        }
    }
    internal static void Register<T>(UniEvt<T> act) where T : IEvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    internal static void UnRegister<T>(UniEvt<T> func) where T : IEvtBase
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

public abstract record EvtBase<THasCt>(THasCt WhoHasCt)
    : IEvtBase, ICanAwait
    where THasCt : IHasCt
{
    bool getDebug = true;
    public EvtBase<THasCt> Debug(bool debug) { getDebug = debug; return this; }
    [HideInInspector] public THasCt WhoHasCt = WhoHasCt;
    [ShowInInspector] string EvtDes => ToString();
    public UniTask.Awaiter GetAwaiter() 
        => WhoHasCt.CurCt.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : Bus.FireAsync(this, WhoHasCt.CurCt, getDebug).GetAwaiter();
}

public abstract record EvtForgetBase : IEvtBase
{
    public void Forget() => Bus.FireAndForget(this);
}

public interface IHasCt
{
    CancellationToken CurCt { get; }
}
public interface IEvtBase;