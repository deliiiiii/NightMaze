using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
public abstract class FSM<TThis> : IDisposable
    where TThis : FSM<TThis>
{
    [JsonIgnore] static bool StateHasSubClass => typeof(IState).SubTypeList().Any();
    [JsonIgnore] readonly CancellationTokenSource cts = new();
    [JsonIgnore] protected CancellationToken CurCt => cts.Token;
    [JsonIgnore, ShowInInspector, PropertyOrder(-10), LabelText(nameof(CurStateName))] protected IState? CurState;
    [JsonIgnore] string CurStateName => CurState?.GetType().Name ?? "Null";

    protected async UniTask LaunchAsync(IState initState)
    {
        if (CurState != null)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        await EnterStateAsync(initState);
        Binder.FromTick(Tick).Bind(CurCt);
    }
    protected void Release()
    {
        if (CurState == null)
        {
            MyDebug.LogError($"FSM {GetType().Name} Release But NOT Launched");
            return;
        }
        CurState.OnExit();
        CurState = null;
        cts.Cancel();
    }
    public async UniTask EnterStateAsync<TState>(TState newState) where TState : IState
    {
        if (CurState != null)
        {
            if (CurState.GetType() == typeof(TState) && !CurState.EnableReEnter)
            {
                MyDebug.Log($"FSM {GetType().Name} ReEnter State {typeof(TState).Name} But ReEnter is Not Enabled");
                return;
            }
            CurState.OnExit();
        }
        MyDebug.Log($"{GetType().GetNiceName()} Enter{newState.GetType().GetNiceName()}");
        
        CurState = newState;
        CurState.BelongFSM = (TThis)this;
        CurState.RegisterAll();
        await CurState.OnEnterAsync();
    }
    [DebuggerStepThrough]
    public MyOption<TState> InState<TState>() => CurState is TState state ? state : None;
    void Tick(float dt) => CurState?.OnUpdate(dt);

    public interface IState
    {
        TThis BelongFSM { get; set; }
        UniTask OnEnterAsync() => UniTask.CompletedTask;
        void OnExit(){}
        void OnUpdate(float dt){}
        bool EnableReEnter => true;
        void RegisterAll(){}
    }
    [Serializable]
    public abstract class StateFSM<TSub> : FSM<TSub>, IState
        where TSub : StateFSM<TSub>
    {
        [field: JsonIgnore, NonSerialized] public TThis BelongFSM { get; set; } = null!;
        UniTask FSM<TThis>.IState.OnEnterAsync() => OnEnterAsync();
        protected virtual UniTask OnEnterAsync() => UniTask.CompletedTask;
        void FSM<TThis>.IState.OnExit() => OnExit();
        protected virtual void OnExit(){}
        void FSM<TThis>.IState.OnUpdate(float dt) => OnUpdate(dt);
        protected virtual void OnUpdate(float dt){}
        bool FSM<TThis>.IState.EnableReEnter => EnableReEnter;
        protected virtual bool EnableReEnter => true;
        void FSM<TThis>.IState.RegisterAll() => IUniEvt.BindAll(this, CurCt);
    }
    
    public abstract record UniAction
    {
        [HideInInspector] public required TThis Ctx;
        [ShowInInspector] public abstract string Des { get; }
        [DebuggerStepThrough] protected abstract UniTask InvokeAsync(CancellationToken ct);
        [DebuggerStepThrough] public UniTask.Awaiter GetAwaiter() => InvokeAsync(Ctx.CurCt).GetAwaiter();
        [DebuggerStepThrough] public void Forget() => InvokeAsync(CancellationToken.None).Forget();
    }

    public void Dispose() => Release();
}