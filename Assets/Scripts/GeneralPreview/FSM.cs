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
public abstract record FSM<TThis> : IDisposable
    where TThis : FSM<TThis>
{
    [JsonIgnore] static bool StateHasSubClass => typeof(IState).SubTypeList().Any();
    [JsonIgnore] readonly CancellationTokenSource cts = new();
    [JsonIgnore] protected CancellationToken CurCt => cts.Token;
    [JsonIgnore] string CurStateName => CurState?.GetType().Name ?? "Null";
    [ShowInInspector, PropertyOrder(-10), LabelText(nameof(CurStateName))] protected IState? CurState;
    protected async UniTask LaunchAsync(IState initState, bool isCurStateFromLoad)
    {
        if (CurState != null)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        await EnterStateAsync(initState, isCurStateFromLoad);
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
    public async UniTask EnterStateAsync<TState>(TState newState, bool isCurStateFromLoad) where TState : IState
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
        MyDebug.Log($"{GetType().GetNiceName()} Enter{newState.GetType().GetNiceName()} IsLaunch :{CurState == null}");
        
        CurState = newState;
        CurState.BelongFSM = (TThis)this;
        CurState.RegisterAll();
        await CurState.OnEnterAsync(isCurStateFromLoad);
    }
    [DebuggerStepThrough] public MyOption<TState> InState<TState>() => CurState is TState state ? state : None;
    [DebuggerStepThrough] public bool IsState<TState>() => CurState is TState;
    [DebuggerStepThrough] void Tick(float dt) => CurState?.OnUpdate(dt);

    public interface IState
    {
        TThis BelongFSM { get; set; }
        UniTask OnEnterAsync(bool isThisFromLoad) => UniTask.CompletedTask;
        [DebuggerStepThrough] void OnExit(){}
        [DebuggerStepThrough] void OnUpdate(float dt){}
        bool EnableReEnter => true;
        [DebuggerStepThrough] void RegisterAll(){}
    }
    [Serializable]
    public abstract record StateFSM<TSub> : FSM<TSub>, IState
        where TSub : StateFSM<TSub>
    {
        [DebuggerStepThrough] public override int GetHashCode() => base.GetHashCode();
        [field: JsonIgnore, NonSerialized] public TThis BelongFSM { get; set; } = null!;
        [DebuggerStepThrough] UniTask FSM<TThis>.IState.OnEnterAsync(bool isThisFromLoad) => OnEnterAsync(isThisFromLoad);
        [DebuggerStepThrough] protected virtual UniTask OnEnterAsync(bool isThisFromLoad) => UniTask.CompletedTask;
        [DebuggerStepThrough] void FSM<TThis>.IState.OnExit() => OnExit();
        [DebuggerStepThrough] protected virtual void OnExit(){}
        [DebuggerStepThrough] void FSM<TThis>.IState.OnUpdate(float dt) => OnUpdate(dt);
        [DebuggerStepThrough] protected virtual void OnUpdate(float dt){}
        bool FSM<TThis>.IState.EnableReEnter => EnableReEnter;
        protected virtual bool EnableReEnter => true;
        [DebuggerStepThrough] void FSM<TThis>.IState.RegisterAll() => IUniEvt.BindAll(this, CurCt);
    }
    
    public abstract record UniAction : IUniAction
    {
        // ReSharper disable once InconsistentNaming
        [HideInInspector, JsonIgnore] public required TThis @this;

        [DebuggerStepThrough] UniTask IUniAction.InvokeAsync(CancellationToken ct) => InvokeAsync(ct);
        [DebuggerStepThrough] protected abstract UniTask InvokeAsync(CancellationToken ct);
        [DebuggerStepThrough] public UniTask.Awaiter GetAwaiter() => InvokeAsync(@this.CurCt).GetAwaiter();
        [DebuggerStepThrough] public void Forget() => InvokeAsync(CancellationToken.None).Forget();
    }

    public void Dispose() => Release();
}

public interface IUniAction
{
    string ToString();
    UniTask InvokeAsync(CancellationToken ct);
    UniTask.Awaiter GetAwaiter();
}