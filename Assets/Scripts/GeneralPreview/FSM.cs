using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
public abstract record FSM<TThis> : IDisposable, IHasVersion
    where TThis : FSM<TThis>
{
    [ShowInInspector, PropertyOrder(-10), LabelText(nameof(CurStateName))] protected IState? CurState;
    // ReSharper disable once ConvertToAutoProperty
    [JsonIgnore] double IHasVersion.savedVersion {get => savedVersion; set => savedVersion = value;}
    double savedVersion = Const.Version;
    [JsonIgnore] string CurStateName => CurState?.GetType().Name ?? "Null";
    
    [JsonIgnore] protected readonly CancellationTokenSource Cts = new();
    [JsonIgnore] protected CancellationToken CurCt => Cts.Token;
    protected async UniTask LaunchAsync(IState initState, bool isNewStateFromLoad)
    {
        if (CurState != null && !isNewStateFromLoad)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        MyDebug.Log($"{GetType().GetNiceName()} Launching...");
        await EnterStateAsync(initState, isNewStateFromLoad);
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
        Cts.Cancel();
    }
    public async UniTask EnterStateAsync<TState>(TState newState, bool isNewStateFromLoad) where TState : IState
    {
        if (CurState != null && !isNewStateFromLoad)
        {
            if (CurState.GetType() == typeof(TState) && !CurState.EnableReEnter)
            {
                MyDebug.Log($"FSM {GetType().Name} ReEnter State {typeof(TState).Name} But ReEnter is Not Enabled");
                return;
            }
            CurState.OnExit();
        }
        MyDebug.Log($"{GetType().GetNiceName()} Enter{newState.GetType().GetNiceName()}. isLaunched: {CurState == null}. isNewStateFromLoad: {isNewStateFromLoad}");
        
        CurState = newState;
        CurState.BelongFSM = (TThis)this;
        CurState.RegisterAll();
        await CurState.OnEnterAsync(isNewStateFromLoad);
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
        [DebuggerStepThrough] void FSM<TThis>.IState.OnExit()
        {
            OnExit();
            Cts.Cancel();
        }

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