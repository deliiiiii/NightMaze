using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;

namespace GeneralPreview;

public static class EvtBus
{
    static readonly Dictionary<Type, List<Delegate>> evtDic = new();

    public static async UniTask FireAsync<T>(T evt, bool withDebug = false) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list)) 
            return;
        foreach (var dele in list)
        {
            if(withDebug)
                MyDebug.Log($"EvtBus Fire {dele}");
            await ((dele as Func<T, UniTask>)?.Invoke(evt) ?? UniTask.CompletedTask);
        }
    }

    public static void Register<T>(Func<T, UniTask> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    public static void UnRegister<T>(Func<T, UniTask> func) where T : EvtBase
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
    
    public static FuncWrap<T> Bind<T>(Func<T, UniTask> func) where T : EvtBase
    {
        return new FuncWrap<T>(func);
    }
}

public abstract record EvtBase
{
    public virtual object? NthValue(int n) => null;
}

public abstract record EvtBase1<T1>(T1 Arg1) : EvtBase
{
    public override object? NthValue(int n) 
        => n switch
        {
            1 => Arg1,
            _ => null
        };
}

public abstract record EvtBase2<T1, T2>(T1 Arg1, T2 Arg2) : EvtBase
{
    public override object? NthValue(int n) 
        => n switch
        {
            1 => Arg1, 
            2 => Arg2,
            _ => null
        };
}

public interface IFuncWrap
{
    void Register();
    void UnRegister();
}
public class FuncWrap<T>(Func<T, UniTask> action) : IFuncWrap
    where T : EvtBase
{
    public void Register() => EvtBus.Register(action);
    public void UnRegister() => EvtBus.UnRegister(action);
}