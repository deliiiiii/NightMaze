using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;

namespace GeneralPreview;

public static class Bus
{
    static readonly Dictionary<Type, List<Delegate>> evtDic = new();

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
            await ((dele as Func<T, CancellationToken, UniTask>)?.Invoke(evt, ct) ?? UniTask.CompletedTask);
        }
    }
    public static void Register<T>(Func<T, CancellationToken, UniTask> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    public static void UnRegister<T>(Func<T, CancellationToken, UniTask> func) where T : EvtBase
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
    public static FuncWrap<T> Bind<T>(Func<T, CancellationToken, UniTask> func) where T : EvtBase
    {
        return new FuncWrap<T>(func);
    }
}

public abstract record EvtBase;
public abstract record EvtBase1<T1> : EvtBase
{
    public required T1 Arg1 { get; init; }
}

public abstract record EvtBase2<T1, T2> : EvtBase
{
    public required T1 Arg1 { get; init; }
    public required T2 Arg2 { get; init; }
}

public record EvtUnit : EvtBase;

public interface IFuncWrap
{
    void Register();
    void UnRegister();
}
public class FuncWrap<T>(Func<T, CancellationToken, UniTask> action) : IFuncWrap
    where T : EvtBase
{
    public void Register() => Bus.Register(action);
    public void UnRegister() => Bus.UnRegister(action);
}