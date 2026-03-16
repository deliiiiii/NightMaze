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
public abstract record FSM<TThis> : IDisposable, IHasVersion, IHasCt
    where TThis : FSM<TThis>
{
    [ShowInInspector, PropertyOrder(-10), LabelText(nameof(CurStateName))] protected IState? CurState;
    [DebuggerStepThrough] public void Dispose() => Release();
    CancellationToken IHasCt.Ct
    {
        [DebuggerStepThrough] get => Cts.Token;
    }

    // ReSharper disable once ConvertToAutoProperty
    [JsonIgnore]
    double IHasVersion.savedVersion
    {
        [DebuggerStepThrough] get => savedVersion; 
        [DebuggerStepThrough] set => savedVersion = value;
    }
    double savedVersion = Const.Version;
    [JsonIgnore] string CurStateName
    {
        [DebuggerStepThrough] get => CurState?.GetType().Name ?? "Null";
    }

    [JsonIgnore] protected readonly CancellationTokenSource Cts = new();
    [JsonIgnore] protected CancellationToken CurCt
    {
        [DebuggerStepThrough] get => Cts.Token;
    }


    protected async UniTask LaunchAsync(IState initState, bool isNewStateFromLoad)
    {
        if (CurState != null && !isNewStateFromLoad)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        MyDebug.Log($"{GetType().GetNiceName()} Launching...");
        await EnterStateAsync(initState, isNewStateFromLoad);
        Tick.ToBinder().Bind(CurCt);
    }
    protected void Release()
    {
        if (CurState == null)
        {
            MyDebug.LogError($"FSM {GetType().Name} Release But NOT Launched");
            Cts.Cancel();
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
    Action<float> Tick => dt => CurState?.OnUpdate(dt);

    public interface IState
    {
        TThis BelongFSM { get; set; }
        [DebuggerStepThrough] UniTask OnEnterAsync(bool isThisFromLoad) => UniTask.CompletedTask;
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
        [JsonIgnore] public TThis BelongFSM { get; set; } = null!;
        [DebuggerStepThrough] UniTask FSM<TThis>.IState.OnEnterAsync(bool isThisFromLoad) => OnEnterAsync(isThisFromLoad);
        [DebuggerStepThrough] protected virtual UniTask OnEnterAsync(bool isThisFromLoad) => UniTask.CompletedTask;
        void FSM<TThis>.IState.OnExit()
        {
            OnExit();
            Cts.Cancel();
        }

        protected virtual void OnExit(){}
        [DebuggerStepThrough] void FSM<TThis>.IState.OnUpdate(float dt) => OnUpdate(dt);
        [DebuggerStepThrough] protected virtual void OnUpdate(float dt){}
        bool FSM<TThis>.IState.EnableReEnter => EnableReEnter;
        protected virtual bool EnableReEnter => true;
        [DebuggerStepThrough] void FSM<TThis>.IState.RegisterAll() => IUniEvt.BindAll(this, CurCt);
    }

    public abstract record UniAction(TThis Self) : ICanAwait
    {
        [HideInInspector] protected readonly TThis Self = Self;

        [DebuggerStepThrough] protected abstract UniTask InvokeAsync();
        public UniTask.Awaiter GetAwaiter() 
            => Self.Cts.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();
        [DebuggerStepThrough] public void Forget() => InvokeAsync().Forget();
    }
}

public interface ICanAwait
{
    UniTask.Awaiter GetAwaiter();
}