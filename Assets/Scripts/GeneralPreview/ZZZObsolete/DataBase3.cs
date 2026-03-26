// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using General;
// using Newtonsoft.Json;
// using Sirenix.Utilities;
//
// namespace GeneralPreview;
//
// public interface IComposite;
// public class DataRoot : IComposite;
//
// public interface ILeaf<TBelong> : IDisposable, IHasCt
//     where TBelong : class, IComposite
// {
//     TBelong BelongData { get; set; }
//     UniTask OnAddAsync(bool isThisFromLoad) => UniTask.CompletedTask;
//     void OnRemove(){}
//     void OnUpdate(float dt){}
//     void IDisposable.Dispose() => OnRemove();
// }
// public interface IComposite<TBelong, TThis> : ILeaf<TBelong>, IComposite, IHasVersion
//     where TBelong : class, IComposite
//     where TThis : class, IComposite, IComposite<TBelong, TThis>
// {
//     UniTask AddComAsync(ILeaf<TThis> toAdd, bool isNewComFromLoad);
//     public void RemoveCom(ILeaf<TThis> toRemove);
// }
//
// public abstract class CompositeBase<TBelong, TThis> : IComposite<TBelong, TThis>
//     where TBelong : class, IComposite
//     where TThis : CompositeBase<TBelong, TThis>
// {
//     public async UniTask OnAddAsync(bool isThisFromLoad)
//     {
//         IUniEvt.BindAll(this, CurCt);
//         var tick = OnUpdate;
//         tick.ToBinder().Bind(CurCt);
//         await OnInitData(isThisFromLoad);
//         await new EvtOnEnter((TThis)this);
//         await OnLaunchCom(isThisFromLoad);
//     }
//
//     protected virtual UniTask OnInitData(bool isThisFromLoad) => UniTask.CompletedTask;
//     /// 子类可覆盖逻辑, 或额外清理comDic和不是comDic里管理的东西, 如GamePlaying中SymbolDeckList
//     protected virtual async UniTask OnLaunchCom(bool isThisFromLoad)
//     {
//         if (isThisFromLoad)
//         {
//             await comDic.Values.ForEachAsync(async c => await c.OnAddAsync(isThisFromLoad));
//         }
//     }
//     /// 子类可覆盖逻辑, 或额外清理comDic和不是comDic里管理的东西, 如GamePlaying中SymbolDeckList
//     protected virtual void OnReleaseCom()
//     {
//         comDic.Values.ForEach(c => c.OnRemove());
//     }
//     
//     public void OnRemove()
//     {
//         new EvtOnExit().Forget();
//         OnReleaseCom();
//         comDic.Clear();
//         cts.Cancel();
//     }
//     public virtual void OnUpdate(float dt)
//     {
//         foreach (var c in comDic.Values)
//         {
//             c.OnUpdate(dt);
//         }
//     }
//     
//     public double savedVersion { get; set; } = Const.Version;
//     [JsonIgnore] readonly CancellationTokenSource cts = new();
//     public CancellationToken CurCt => cts.Token;
//     
//     protected virtual List<HashSet<Type>> MutexListSet { get; } = [];
//     [JsonProperty(Order = 9999)]readonly Dictionary<Type, ILeaf<TThis>> comDic = [];
//     [field: NonSerialized][JsonIgnore] public TBelong BelongData { get; set; } = null!;
//     public async UniTask AddComAsync(ILeaf<TThis> toAdd, bool isNewComFromLoad)
//     {
//         var gotMutex =
//             from com in comDic.Values
//             from mutexList in MutexListSet
//             where mutexList.Contains(com.GetType()) && mutexList.Contains(toAdd.GetType())
//             select com;
//         gotMutex.ToList().ForEach(RemoveCom);
//         if(comDic.ContainsKey(toAdd.GetType()))
//         {
//             MyDebug.LogError($"{GetType().GetNiceName()} AddCom {toAdd.GetType().Name} But Already Exists");
//             return;
//         }
//         comDic.Add(toAdd.GetType(), toAdd);
//         toAdd.BelongData = (TThis)this;
//         await toAdd.OnAddAsync(isNewComFromLoad);
//     }
//     public void RemoveCom(ILeaf<TThis> toRemove)
//     {
//         var key = toRemove.GetType();
//         if (!comDic.ContainsKey(key))
//         {
//             MyDebug.LogError(
//                 $"{GetType().GetNiceName()} RemoveCom {toRemove.GetType().Name} But NOT Exists");
//             return;
//         }
//
//         toRemove.OnRemove();
//         comDic.Remove(key);
//     }
//     [DebuggerStepThrough]public MyOption<TCom> GetComOptional<TCom>() where TCom : CompositeBase<TThis, TCom>
//         => comDic.TryGetValue(typeof(TCom), out var com) ? (TCom)com : None;
//     // [DebuggerStepThrough]public MyOption<ILeaf<TThis>> GetFirstCom() => comDic.Values.FirstOptional(_ => true);
//     [DebuggerStepThrough]public bool HasCom<TCom>() where TCom : CompositeBase<TThis, TCom>
//         => comDic.ContainsKey(typeof(TCom));
//     
//     public abstract record UniAction(TThis Self) : ICanAwait
//     {
//         [UnityEngine.HideInInspector] protected readonly TThis Self = Self;
//
//         [DebuggerStepThrough] protected abstract UniTask InvokeAsync();
//         public UniTask.Awaiter GetAwaiter() 
//             => Self.CurCt.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();
//         [DebuggerStepThrough] public void Forget() => InvokeAsync().Forget();
//     }
//     
//     /// 仅为了通知UI.
//     public record EvtOnEnter(TThis WhoHasCt) : EvtBase<TThis>(WhoHasCt);
//     /// 仅为了通知UI.
//     public record EvtOnExit : EvtForgetBase;
// }
