using System;
using System.Diagnostics.CodeAnalysis;
using General;
using General.BindData;
using Newtonsoft.Json;
using UnityEngine;

namespace GeneralPreview;
[Serializable]
public abstract class FSM<TThis>
    where TThis : FSM<TThis>
{
    public event Action<IState>? OnStateEnter;
    public event Action<IState>? OnStateExit;
    [SerializeReference] protected IState? CurState;
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
        selfTickBind = Binder.FromTick(Tick);
        selfTickBind.Bind();
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
        selfTickBind?.UnBind();
        selfTickBind = null;
    }
    public void EnterState<TSubState>(Action<TSubState>? act = null) where TSubState : class, IState
    {
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Enter State But NOT Launched");
            return;
        }
        if (CurState != null)
        {
            OnStateExit?.Invoke(CurState);
            CurState.OnExit();
        }
        var subState = Activator.CreateInstance<TSubState>()!;
        CurState = subState;
        CurState.BelongFSM = (TThis)this;
        act?.Invoke(subState);
        CurState.OnEnter();
        OnStateEnter?.Invoke(CurState);
    }
    public bool IsState<TSubState>([NotNullWhen(true)] out TSubState subState) where TSubState : class, IState
    {
        subState = null!;
        if (CurState is TSubState state)
        {
            subState = state;
            return true;
        }
        return false;
    }
    void Tick(float dt) => CurState?.OnUpdate(dt);
    public interface IState
    {
        public TThis BelongFSM { get; set; }
        public virtual void OnEnter(){}
        public void OnExit(){}
        public void OnUpdate(float dt){}
    }
}
[Serializable]
public abstract class FSMState<TBelong, TThis> : 
    FSM<TThis>, 
    FSM<TBelong>.IState
    where TThis : FSM<TThis>
    where TBelong : FSM<TBelong>
{
    public required TBelong BelongFSM { get; set; }
    public virtual void OnEnter(){}
    public virtual void OnExit(){}
    public virtual void OnUpdate(float dt){}
}