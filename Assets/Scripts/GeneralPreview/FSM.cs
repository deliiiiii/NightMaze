using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using General.BindData;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
public abstract class FSM<TThis> : IDisposable
    where TThis : FSM<TThis>
{
    
    [JsonIgnore] static bool StateHasSubClass => typeof(IState).SubTypeList().Any();
    [JsonIgnore, ShowInInspector, PropertyOrder(-11)] public string CurStateName => curState?.GetType().Name ?? "Null";
    [JsonIgnore] bool isLaunched;
    [JsonIgnore] BindDataUpdate? selfTickBind;
    [JsonIgnore] readonly CancellationTokenSource cts = new();
    [JsonIgnore] protected CancellationToken CurCt => cts.Token;
    [ShowInInspector, PropertyOrder(-10)] IState? curState;
    protected abstract IState InitState { get; }

    protected async UniTask LaunchAsync()
    {
        if (isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        await EnterStateAsync(curState ?? InitState);
        Binder.FromTick(Tick).Bind(CurCt);
    }
    void Release()
    {
        if (!isLaunched && StateHasSubClass)
        {
            MyDebug.LogError($"FSM {GetType().Name} Release But NOT Launched");
            return;
        }
        isLaunched = false;
        if (curState != null)
        {
            curState.OnExit();
            curState = null;
        }
        cts.Cancel();
    }
    public async UniTask EnterStateAsync<TState>(TState stateData) where TState : IState
    {
        if (curState != null && isLaunched)
        {
            if (curState.GetType() == typeof(TState) && !curState.EnableReEnter)
            {
                MyDebug.Log($"FSM {GetType().Name} ReEnter State {typeof(TState).Name} But ReEnter is Not Enabled");
                return;
            }
            curState.OnExit();
        }
        MyDebug.Log($"{GetType().GetNiceName()} Enter{typeof(TState).GetNiceName()}");

        isLaunched = true;
        curState = stateData;
        curState.BelongFSM = (TThis)this;
        curState.RegisterAll();
        await curState.OnEnterAsync();
    }
    [DebuggerStepThrough]
    public MyOption<TState> InState<TState>() => curState is TState state ? state : None;
    void Tick(float dt) => curState?.OnUpdate(dt);

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
        void FSM<TThis>.IState.OnExit()
        {
            OnExit();
            Release();
        }
        protected virtual void OnExit(){}
        void FSM<TThis>.IState.OnUpdate(float dt) => OnUpdate(dt);
        protected virtual void OnUpdate(float dt){}
        bool FSM<TThis>.IState.EnableReEnter => EnableReEnter;
        protected virtual bool EnableReEnter => true;
        void FSM<TThis>.IState.RegisterAll() => IUniEvt.BindAll(this, CurCt);
    }
    
    public abstract record UniAction
    {
        [HideInInspector]
        public required TThis Ctx;
        [ShowInInspector]
        public abstract string Des { get; }
        protected abstract UniTask InvokeAsync(CancellationToken ct);

        public UniTask.Awaiter GetAwaiter()
        {
            return InvokeAsync(Ctx.CurCt).GetAwaiter();
        }
        public void Forget() => InvokeAsync(CancellationToken.None).Forget();
    }

    public void Dispose() => Release();
}