using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public class DataUnit : DataBase<DataUnit>
{
    public static readonly DataUnit One = new();
}
public abstract class DataBase<TThis> : DataBase<DataUnit>.ICom
    where TThis : DataBase<TThis>
{
    [JsonIgnore, HideInInspector] DataUnit DataUnit.ICom.BelongData { get; set; } = DataUnit.One;
    
    public virtual UniTask OnAddAsync(bool isThisFromLoad)
    {
        if (isThisFromLoad)
        {
            comDic.Values.ForEach(c =>
            {
                c.BelongData = (TThis)this;
                IUniEvt.BindAll(this, CurCt);
            });
        }
        Tick.ToBinder().Bind(CurCt);
        return UniTask.CompletedTask;
    }

    public virtual void OnRemove()
    {
        comDic.Values.ForEach(c => c.OnRemove());
        comDic.Clear();
        cts.Cancel();
    }

    public virtual void OnUpdate(float dt) { }
    void IDisposable.Dispose() => OnRemove();
    // 组件默认都只能存在一个.
    [ShowInInspector, PropertyOrder(-10), JsonProperty(Order = 9999)] Dictionary<Type, ICom> comDic = [];
    protected virtual List<HashSet<Type>> MutexListSet { get; [UsedImplicitly] private set; } = [];
    
    CancellationToken IHasCt.Ct { [DebuggerStepThrough] get => cts.Token; }
    public double savedVersion { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = Const.Version;

    [JsonIgnore] readonly CancellationTokenSource cts = new();
    [JsonIgnore] protected CancellationToken CurCt { [DebuggerStepThrough] get => cts.Token; }
    
    async UniTask _AddComAsync(ICom toAdd, bool isNewComFromLoad)
    {
        if(comDic.ContainsKey(toAdd.GetType()))
        {
            MyDebug.LogError($"{nameof(DataBase<>)} {GetType().Name} AddCom {toAdd.GetType().Name} But Already Exists");
            return;
        }
        comDic.Add(toAdd.GetType(), toAdd);
        toAdd.BelongData = (TThis)this;
        IUniEvt.BindAll(this, CurCt);
        await toAdd.OnAddAsync(isNewComFromLoad);
    }
    void _RemoveCom(ICom toRemove)
    {
        var key = toRemove.GetType();
        if (!comDic.ContainsKey(key))
        {
            MyDebug.LogError($"{nameof(DataBase<>)} {GetType().Name} RemoveCom {toRemove.GetType().Name} But NOT Exists");
            return;
        }
        toRemove.OnRemove();
        comDic.Remove(key);
    }
    Action<float> Tick => dt => { OnUpdate(dt); comDic.Values.ForEach(c => c.OnUpdate(dt)); };
    
    [DebuggerStepThrough]public async UniTask AddComAsync(ICom toAdd, bool isNewComFromLoad)
    {
        var gotMutex =
            from com in comDic.Values
            from mutexList in MutexListSet
            where mutexList.Contains(com.GetType()) && mutexList.Contains(toAdd.GetType())
            select com;
        gotMutex.ToList().ForEach(_RemoveCom);
        await _AddComAsync(toAdd, isNewComFromLoad);
    }
    [DebuggerStepThrough]public void RemoveCom(ICom toRemove) => _RemoveCom(toRemove);
    [DebuggerStepThrough]public MyOption<TCom> GetComOptional<TCom>() where TCom : Com<TCom>
        => comDic.TryGetValue(typeof(TCom), out var com) ? (TCom)com : None;
    [DebuggerStepThrough]public MyOption<ICom> GetFirstCom() => comDic.Values.FirstOptional(_ => true);
    [DebuggerStepThrough]public bool HasCom<TCom>() where TCom : Com<TCom>
        => comDic.ContainsKey(typeof(TCom));
    public interface ICom : IDisposable, IHasVersion, IHasCt
    {
        TThis BelongData { get; set; }
        UniTask OnAddAsync(bool isThisFromLoad);
        void OnRemove();
        void OnUpdate(float dt);
    }
    public abstract class Com<TSub> : DataBase<TSub>, ICom 
        where TSub : DataBase<TSub>
    {
        [JsonIgnore, HideInInspector] public TThis BelongData { get; set; } = null!;
        [DebuggerStepThrough]UniTask DataBase<TThis>.ICom.OnAddAsync(bool isThisFromLoad) => OnAddAsync(isThisFromLoad);
        [DebuggerStepThrough]void DataBase<TThis>.ICom.OnRemove() => OnRemove();
        [DebuggerStepThrough]void DataBase<TThis>.ICom.OnUpdate(float dt) => OnUpdate(dt);
    }
    
    public abstract record UniAction(TThis Self) : ICanAwait
    {
        [UnityEngine.HideInInspector] protected readonly TThis Self = Self;

        [DebuggerStepThrough] protected abstract UniTask InvokeAsync();
        public UniTask.Awaiter GetAwaiter() 
            => Self.cts.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();
        [DebuggerStepThrough] public void Forget() => InvokeAsync().Forget();
    }
}

public interface ICanAwait
{
    UniTask.Awaiter GetAwaiter();
}

// public record GamePlaying2 : FSM2<GamePlaying2>
// {
//     protected override List<HashSet<Type>> MutexListSet =>
//     [
//         [typeof(PlayInstantSpin1), typeof(PlayInstantSpin2)]
//     ];
// }
// public record PlayInstantSpin1 : GamePlaying2.ComBase<PlayInstantSpin1>;
// public record PlayInstantSpin2 : GamePlaying2.ComBase<PlayInstantSpin2>
// {
//     public static class TestClass
//     {
//         public static void TestFunc()
//         { 
//             var gp2 = new GamePlaying2();
//             gp2.AddComAsync(new PlayInstantSpin1(), false).Forget();
//             MyDebug.Log(gp2.GetComOptional<PlayInstantSpin1>().HasValue);
//             MyDebug.Log(gp2.GetComOptional<PlayInstantSpin2>().HasValue);
//             gp2.AddComAsync(new PlayInstantSpin2(), false).Forget();
//             MyDebug.Log(gp2.GetComOptional<PlayInstantSpin1>().HasValue);
//             MyDebug.Log(gp2.GetComOptional<PlayInstantSpin2>().HasValue);
//         }
//     }
// }