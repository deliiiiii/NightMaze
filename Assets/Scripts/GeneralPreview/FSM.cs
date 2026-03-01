using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using General.BindData;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace GeneralPreview;
public abstract class FSM<TThis>
    where TThis : FSM<TThis>
{
    [ShowInInspector, PropertyOrder(0)] public string CurStateName => curState?.GetType().Name ?? "Null";
    [ShowInInspector, PropertyOrder(1)] IState? curState;
    bool isLaunched;
    [JsonIgnore] BindDataUpdate? selfTickBind;
    readonly CancellationTokenSource cts = new();
    protected CancellationToken CurCt => cts.Token;

    protected async UniTask LaunchAsync<TState>() where TState : IState
    {
        if (isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        isLaunched = true;
        await EnterStateAsync<TState>();
        // selfTickBind = Binder.FromTick(Tick);
        // selfTickBind.Bind();
    }

    public void Release()
    {
        // if (!isLaunched)
        // {
            // MyDebug.LogError($"FSM {GetType().Name} Release But NOT Launched");
        // }
        isLaunched = false;
        if (curState != null)
        {
            // await curState.OnExitAsync(ct);
            // curState.UnRegisterAll();
            curState.TryRelease();
            curState = null;
        }
        cts.Cancel();
        // selfTickBind?.UnBind();
        // selfTickBind = null;
    }
    public async UniTask EnterStateAsync<TState>() where TState : IState
    {
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Enter State But NOT Launched");
            return;
        }
        if (curState != null)
        {
            if(curState.GetType() == typeof(TState) && !curState.EnableReEnter)
                return;
            // await curState.OnExitAsync(ct);
            // curState.UnRegisterAll();
            curState.TryRelease();
        }
        curState = Activator.CreateInstance<TState>()!;
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
        public void TryRelease(){}
        public void OnUpdate(float dt){}
        public bool EnableReEnter => false;
        public void RegisterAll(){}
    }
    [Serializable]
    public abstract class StateFSM<TSub> : FSM<TSub>, IState
        where TSub : StateFSM<TSub>
    {
        public required TThis BelongFSM { get; set; }
        public virtual UniTask OnEnterAsync() => UniTask.CompletedTask;
        void FSM<TThis>.IState.TryRelease() => Release();
        public virtual void OnUpdate(float dt){}
        public virtual bool EnableReEnter => false;

        void FSM<TThis>.IState.RegisterAll() => IUniEvt.BindAll(this, CurCt);
    }
}