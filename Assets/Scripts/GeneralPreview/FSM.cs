using System;
using General;
using General.BindData;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeneralPreview;
[Serializable]
public abstract class FSM<TThis>
    where TThis : FSM<TThis>
{
    public event Action<IState>? OnStateEnter;
    public event Action<IState>? OnStateExit;
    [ShowInInspector][PropertyOrder(0)] public string CurStateName => CurState?.GetType().Name ?? "Null";
    [SerializeReference][ReadOnly][PropertyOrder(1)] protected IState? CurState;
    bool isLaunched;
    [JsonIgnore] BindDataUpdate? selfTickBind;

    public void Launch<TSubState>() where TSubState : class, IState
    {
        if (isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        isLaunched = true;
        EnterState<TSubState>();
        // selfTickBind = Binder.FromTick(Tick);
        // selfTickBind.Bind();
    }
    public void Release()
    {
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Release But NOT Launched"); 
            return;
        }
        isLaunched = false;
        CurState?.OnExit();
        CurState = null;
        // selfTickBind?.UnBind();
        // selfTickBind = null;
    }
    public TSubState EnterState<TSubState>() where TSubState : class, IState
    {
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Enter State But NOT Launched");
            return null!;
        }
        if (CurState != null)
        {
            if(CurState.GetType() == typeof(TSubState) && !CurState.EnableReEnter)
                return (TSubState)CurState;
            OnStateExit?.Invoke(CurState);
            CurState.OnExit();
        }
        var subState = Activator.CreateInstance<TSubState>()!;
        CurState = subState;
        CurState.BelongFSM = (TThis)this;
        CurState.OnEnter();
        OnStateEnter?.Invoke(CurState);
        return subState;
    }
    public MyOption<TSubState> InState<TSubState>() where TSubState : class, IState
    {
        if (CurState is TSubState state)
        {
            return state;
        }
        return None;
    }
    void Tick(float dt) => CurState?.OnUpdate(dt);
    public interface IState
    {
        public TThis BelongFSM { get; set; }
        public void OnEnter(){}
        public void OnExit(){}
        public void OnUpdate(float dt){}
        public bool EnableReEnter => false;
    }
    [Serializable]
    public abstract class StateFSM<TSub> : FSM<TSub>, IState
        where TSub : FSM<TSub>
    {
        public required TThis BelongFSM { get; set; }
        public abstract void OnEnter();
        public abstract void OnExit();
        public virtual void OnUpdate(float dt){}
        public virtual bool EnableReEnter => false;
    }
}