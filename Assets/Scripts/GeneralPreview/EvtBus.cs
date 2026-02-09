using System;
using System.Collections.Generic;

namespace GeneralPreview;

public static class EvtBus
{
    class Handler
    {
        public required Delegate Act;
        public required string Des;
    }
    
    static readonly Dictionary<Type, List<Handler>> evtDic = new();

    public static void Fire<T>(T evt) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list)) 
            return;
        var snapshot = list.ToArray();
        foreach (var handler in snapshot)
        {
            (handler.Act as Action<T>)?.Invoke(evt);
        }
    }

    public static void Register<T>(Action<T> act, string des = "no des") where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(new Handler { Act = act, Des = des });
    }
    
    public static void Unregister<T>(Action<T> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
            return;
        var index = list.FindIndex(h => h.Act == (Delegate)act);
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


