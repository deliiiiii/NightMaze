// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using General;
// using Newtonsoft.Json;
// using Sirenix.OdinInspector;
// using Sirenix.Utilities;
//
// namespace GeneralPreview;
//
// public class DataRoot : DataBase<DataRoot>
// {
//     public static readonly DataRoot One = new();
// }
//
//
//
// public abstract class DataBase<TThis> : DataBase<DataRoot>.ICom
//     where TThis : DataBase<TThis>
// {
//     // [JsonIgnore, HideInInspector] DataUnit DataUnit.ICom.BelongData { get; set; } = DataUnit.One;
//     async UniTask DataRoot.ICom.OnAddAsync(DataRoot _, bool isThisFromLoad)
//     {
//         IUniEvt.BindAll(this, CurCt);
//         Tick.ActBindTo().Bind(CurCt);
//         if (isThisFromLoad)
//         {
//             await comDic.Values.ForEachAsync(async c => await c.OnAddAsync((TThis)this, isThisFromLoad));
//         }
//     }
//     void DataRoot.ICom.OnRemove(DataRoot belongData)
//     {
//         comDic.Values.ForEach(c => c.OnRemove((TThis)this));
//         comDic.Clear();
//         cts.Cancel();
//     }
//     void DataRoot.ICom.OnUpdate(DataRoot belongData, float dt){}
//     void IDisposable.Dispose() => ((DataRoot.ICom)this).OnRemove(DataRoot.One);
//     // 组件默认都只能存在一个.
//     [ShowInInspector, PropertyOrder(-10), JsonProperty(Order = 9999)] Dictionary<Type, ICom> comDic = [];
//     protected virtual List<HashSet<Type>> MutexListSet { get; } = [];
//     
//     CancellationToken IHasCt.Ct { [DebuggerStepThrough] get => cts.Token; }
//     public double savedVersion { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = Const.Version;
//
//     [JsonIgnore] readonly CancellationTokenSource cts = new();
//     [JsonIgnore] protected CancellationToken CurCt { [DebuggerStepThrough] get => cts.Token; }
//     
//     async UniTask _AddComAsync(ICom toAdd, bool isNewComFromLoad)
//     {
//         if(comDic.ContainsKey(toAdd.GetType()))
//         {
//             MyDebug.LogError($"{nameof(DataBase<>)} {GetType().Name} AddCom {toAdd.GetType().Name} But Already Exists");
//             return;
//         }
//         comDic.Add(toAdd.GetType(), toAdd);
//         // toAdd.BelongData = (TThis)this;
//         IUniEvt.BindAll(toAdd, CurCt);
//         await toAdd.OnAddAsync((TThis)this, isNewComFromLoad);
//     }
//     void _RemoveCom(ICom toRemove)
//     {
//         var key = toRemove.GetType();
//         if (!comDic.ContainsKey(key))
//         {
//             MyDebug.LogError($"{nameof(DataBase<>)} {GetType().Name} RemoveCom {toRemove.GetType().Name} But NOT Exists");
//             return;
//         }
//         toRemove.OnRemove((TThis)this);
//         comDic.Remove(key);
//     }
//     Action<float> Tick => dt => 
//     {
//         OnRootUpdate(dt);  
//         foreach(var c in comDic.Values) 
//         {
//             c.OnUpdate((TThis)this, dt); 
//         }
//     };
//     
//     [DebuggerStepThrough]public async UniTask AddComAsync(ICom toAdd, bool isNewComFromLoad)
//     {
//         var gotMutex =
//             from com in comDic.Values
//             from mutexList in MutexListSet
//             where mutexList.Contains(com.GetType()) && mutexList.Contains(toAdd.GetType())
//             select com;
//         gotMutex.ToList().ForEach(_RemoveCom);
//         await _AddComAsync(toAdd, isNewComFromLoad);
//     }
//     [DebuggerStepThrough]public void RemoveCom(ICom toRemove) => _RemoveCom(toRemove);
//     [DebuggerStepThrough]public MyOption<TCom> GetComOptional<TCom>() where TCom : Com<TCom>
//         => comDic.TryGetValue(typeof(TCom), out var com) ? (TCom)com : None;
//     [DebuggerStepThrough]public MyOption<ICom> GetFirstCom() => comDic.Values.FirstOptional(_ => true);
//     [DebuggerStepThrough]public bool HasCom<TCom>() where TCom : Com<TCom>
//         => comDic.ContainsKey(typeof(TCom));
//     public interface ICom : IDisposable, IHasVersion, IHasCt
//     {
//         // TThis BelongData { get; set; }
//         UniTask OnAddAsync(TThis belongData, bool isThisFromLoad);
//         void OnRemove(TThis belongData);
//         void OnUpdate(TThis belongData, float dt);
//     }
//     public abstract class Com<TSub> : DataBase<TSub>, ICom 
//         where TSub : Com<TSub>
//     {
//         // [JsonIgnore, HideInInspector] public TThis BelongData { get; set; } = null!;
//         [DebuggerStepThrough] public virtual UniTask OnAddAsync(TThis belongData, bool isThisFromLoad) => UniTask.CompletedTask;
//         [DebuggerStepThrough] public virtual void OnRemove(TThis belongData) { }
//         [DebuggerStepThrough] public virtual void OnUpdate(TThis belongData, float dt) { }
//     }
//     
//     public abstract record UniAction(TThis Self) : ICanAwait
//     {
//         [UnityEngine.HideInInspector] protected readonly TThis Self = Self;
//
//         [DebuggerStepThrough] protected abstract UniTask InvokeAsync();
//         public UniTask.Awaiter GetAwaiter() 
//             => Self.cts.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();
//         [DebuggerStepThrough] public void Forget() => InvokeAsync().Forget();
//     }
// }