using System;
using System.Collections.Generic;

namespace GeneralPreview;

public static class EvtBus
{
    static readonly Dictionary<Type, List<Delegate>> evtDic = new();

    public static void Fire<T>(T evt) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list)) 
            return;
        foreach (var dele in list)
        {
            (dele as Action<T>)?.Invoke(evt);
        }
    }

    public static void Register<T>(Action<T> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    public static void UnRegister<T>(Action<T> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
            return;
        var index = list.FindIndex(h => h == (Delegate)act);
        if (index == -1) 
            return;
        list.RemoveAt(index);
        if (list.Count == 0)
        {
            evtDic.Remove(typeof(T));
        }
    }
    
    public static ActionWrap<T> Bind<T>(Action<T> action) where T : EvtBase
    {
        return new ActionWrap<T>(action);
    }
}

public abstract record EvtBase;

public abstract record EvtBase<TCtx>(TCtx Ctx) : EvtBase
    where TCtx : IEvtCtx;
public interface IEvtCtx;

public interface IActionWrap
{
    void Register();
    void UnRegister();
}
public class ActionWrap<T>(Action<T> action) : IActionWrap
    where T : EvtBase
{
    public void Register() => EvtBus.Register(action);
    public void UnRegister() => EvtBus.UnRegister(action);
}