using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using Sirenix.Utilities;

namespace GeneralPreview;

public interface IComposite;
public class DataRoot : IComposite;

public interface ILeaf<TBelong> : IDisposable, IHasVersion, IHasCt
    where TBelong : class, IComposite
{
    TBelong BelongData { get; set; }
    UniTask OnAddAsync(bool isThisFromLoad);
    void OnRemove();
    void OnUpdate(float dt);
    void IDisposable.Dispose() => OnRemove();
}
public interface IComposite<TBelong, TThis> : ILeaf<TBelong>
    where TBelong : class, IComposite
    where TThis : class, IComposite, IComposite<TBelong, TThis>
{
    UniTask AddComAsync(ILeaf<TThis> toAdd, bool isNewComFromLoad);
    public void RemoveCom(ILeaf<TThis> toRemove);
}

public abstract class CompositeBase<TBelong, TThis> : IComposite, IComposite<TBelong, TThis>
    where TBelong : class, IComposite
    where TThis : CompositeBase<TBelong, TThis>
{
    public virtual UniTask OnAddAsync(bool isThisFromLoad)
    {
        IUniEvt.BindAll(this, CurCt);
        var tick = OnUpdate;
        tick.ToBinder().Bind(CurCt);
        return UniTask.CompletedTask;
        // 必须自己管理.
        // if (isThisFromLoad)
        // {
        //     await comDic.Values.ForEachAsync(async c => await c.OnAddAsync(isThisFromLoad));
        // }
    }

    protected async UniTask AllComOnAddAsync()
    {
        foreach (var c in comDic.Values)
        {
            await c.OnAddAsync(true);
        }
    }
    public virtual void OnRemove()
    {
        comDic.Values.ForEach(c => c.OnRemove());
        comDic.Clear();
        cts.Cancel();
    }
    public virtual void OnUpdate(float dt)
    {
        foreach (var c in comDic.Values)
        {
            c.OnUpdate(dt);
        }
    }
    
    public double savedVersion { get; set; } = Const.Version;
    [JsonIgnore] readonly CancellationTokenSource cts = new();
    public CancellationToken CurCt => cts.Token;
    
    protected virtual List<HashSet<Type>> MutexListSet { get; } = [];
    readonly Dictionary<Type, ILeaf<TThis>> comDic = [];
    [field: NonSerialized, JsonIgnore] public TBelong BelongData { get; set; } = null!;
    public async UniTask AddComAsync(ILeaf<TThis> toAdd, bool isNewComFromLoad)
    {
        var gotMutex =
            from com in comDic.Values
            from mutexList in MutexListSet
            where mutexList.Contains(com.GetType()) && mutexList.Contains(toAdd.GetType())
            select com;
        gotMutex.ToList().ForEach(RemoveCom);
        if(comDic.ContainsKey(toAdd.GetType()))
        {
            MyDebug.LogError($"{GetType().GetNiceName()} AddCom {toAdd.GetType().Name} But Already Exists");
            return;
        }
        comDic.Add(toAdd.GetType(), toAdd);
        toAdd.BelongData = (TThis)this;
        await toAdd.OnAddAsync(isNewComFromLoad);
    }
    public void RemoveCom(ILeaf<TThis> toRemove)
    {
        var key = toRemove.GetType();
        if (!comDic.ContainsKey(key))
        {
            MyDebug.LogError(
                $"{GetType().GetNiceName()} RemoveCom {toRemove.GetType().Name} But NOT Exists");
            return;
        }

        toRemove.OnRemove();
        comDic.Remove(key);
    }
    [DebuggerStepThrough]public MyOption<TCom> GetComOptional<TCom>() where TCom : CompositeBase<TThis, TCom>
        => comDic.TryGetValue(typeof(TCom), out var com) ? (TCom)com : None;
    [DebuggerStepThrough]public MyOption<ILeaf<TThis>> GetFirstCom() => comDic.Values.FirstOptional(_ => true);
    [DebuggerStepThrough]public bool HasCom<TCom>() where TCom : CompositeBase<TThis, TCom>
        => comDic.ContainsKey(typeof(TCom));
    
    public abstract record UniAction(TThis Self) : ICanAwait
    {
        [UnityEngine.HideInInspector] protected readonly TThis Self = Self;

        [DebuggerStepThrough] protected abstract UniTask InvokeAsync();
        public UniTask.Awaiter GetAwaiter() 
            => Self.CurCt.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();
        [DebuggerStepThrough] public void Forget() => InvokeAsync().Forget();
    }
}

public interface ICanAwait
{
    UniTask.Awaiter GetAwaiter();
}

// public static class DisposableTExt
// {
//     extension<T>(IDisposable<T> self)
//     {
//         public CancellationTokenRegistration AddTo(T ctx, CancellationToken ct)
//         {
//             return ct.RegisterWithoutCaptureExecutionContext((object state) =>
//             {
//                 var d = (IDisposable<T>)state;
//                 d.Dispose(ctx);
//             }, self);
//         }
//     }
// }