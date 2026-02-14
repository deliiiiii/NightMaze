using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Sirenix.Utilities;

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
            await ((dele as UniFunc<T>)?.Invoke(evt, ct) ?? UniTask.CompletedTask);
        }
    }
    public static void Register<T>(UniFunc<T> act) where T : EvtBase
    {
        if (!evtDic.TryGetValue(typeof(T), out var list))
        {
            list = [];
            evtDic[typeof(T)] = list;
        }
        list.Add(act);
    }
    public static void UnRegister<T>(UniFunc<T> func) where T : EvtBase
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
    public static FuncWrap<T> Bind<T>(UniFunc<T> func) where T : EvtBase
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
public class FuncWrap<T>(UniFunc<T> action) : IFuncWrap
    where T : EvtBase
{
    public void Register() => Bus.Register(action);
    public void UnRegister() => Bus.UnRegister(action);
}

public static class FuncWrapExt
{
    extension(IEnumerable<IFuncWrap> self)
    {
        public void BindAll() => self.ForEach(wrap => wrap.Register());
        public void UnBindAll() => self.ForEach(wrap => wrap.UnRegister());
    }
}