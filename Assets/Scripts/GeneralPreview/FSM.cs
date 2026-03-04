using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using General.BindData;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeneralPreview;
public abstract class FSM<TThis>
    where TThis : FSM<TThis>
{
    [JsonIgnore] static bool StateHasSubClass => typeof(IState).SubTypeList().Any();
    
    [JsonIgnore, ShowInInspector, PropertyOrder(0)] public string CurStateName => curState?.GetType().Name ?? "Null";
    [JsonIgnore, ShowInInspector, PropertyOrder(1)] IState? curState;
    [JsonIgnore] bool isLaunched;
    [JsonIgnore] BindDataUpdate? selfTickBind;
    [JsonIgnore] readonly CancellationTokenSource cts = new();
    [JsonIgnore] protected CancellationToken CurCt => cts.Token;

    public async UniTask LaunchAsync<TState>(TState stateData) where TState : IState
    {
        if (isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        isLaunched = true;
        await EnterStateAsync(stateData);
        Binder.FromTick(Tick).Bind(CurCt);
    }

    public void Release()
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
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Enter State But NOT Launched");
            return;
        }
        if (curState != null)
        {
            if (curState.GetType() == typeof(TState) && !curState.EnableReEnter)
            {
                MyDebug.Log($"FSM {GetType().Name} ReEnter State {typeof(TState).Name} But ReEnter is Not Enabled");
                return;
            }
            curState.OnExit();
        }
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
        public TThis BelongFSM { get; set; }
        public UniTask OnEnterAsync() => UniTask.CompletedTask;
        public void OnExit(){}
        public void OnUpdate(float dt){}
        public bool EnableReEnter => true;
        public void RegisterAll(){}
    }
    [Serializable]
    public abstract class StateFSM<TSub> : FSM<TSub>, IState
        where TSub : StateFSM<TSub>
    {
        [field: JsonIgnore, NonSerialized] public TThis BelongFSM { get; set; } = null!;
        public virtual UniTask OnEnterAsync() => UniTask.CompletedTask;

        void FSM<TThis>.IState.OnExit()
        {
            OnExit();
            Release();
        }
        public virtual void OnExit(){}
        public virtual void OnUpdate(float dt){}
        public virtual bool EnableReEnter => true;

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
}