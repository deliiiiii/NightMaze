using System;
using System.Collections.Generic;
using General;
using General.BindData;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;
public abstract class FSM<TThis>
    where TThis : FSM<TThis>
{
    [ShowInInspector, PropertyOrder(0)] public string CurStateName => curState?.GetType().Name ?? "Null";
    [ShowInInspector, PropertyOrder(1)] IState? curState;
    bool isLaunched;
    [JsonIgnore] BindDataUpdate? selfTickBind;

    public void Launch<TState>() where TState : class, IState
    {
        if (isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Has Already Launched");
            return;
        }
        isLaunched = true;
        EnterState<TState>();
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
        curState?.OnExit();
        curState?.UnRegisterAll();
        curState = null;
        // selfTickBind?.UnBind();
        // selfTickBind = null;
    }
    public TState EnterState<TState>() where TState : class, IState
    {
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Enter State But NOT Launched");
            return null!;
        }
        if (curState != null)
        {
            if(curState.GetType() == typeof(TState) && !curState.EnableReEnter)
                return (TState)curState;
            curState.OnExit();
            curState.UnRegisterAll();
        }
        var subState = Activator.CreateInstance<TState>()!;
        curState = subState;
        curState.BelongFSM = (TThis)this;
        curState.RegisterAll();
        curState.OnEnter();
        return subState;
    }

    public TState EnterStateIfNotIn<TState>() where TState : class, IState
    {
        return InState<TState>().Match(some => some, EnterState<TState>);
    }
    public MyOption<TState> InState<TState>() where TState : class, IState
    {
        if (curState is TState state)
        {
            return state;
        }
        return None;
    }
    void Tick(float dt) => curState?.OnUpdate(dt);
    public interface IState
    {
        public TThis BelongFSM { get; set; }
        public void OnEnter(){}
        public void OnExit(){}
        public void OnUpdate(float dt){}
        public bool EnableReEnter => false;
        
        public IEnumerable<IFuncWrap> OnEvtList() => [];
        void RegisterAll() => OnEvtList().ForEach(func => func.Register());
        void UnRegisterAll() => OnEvtList().ForEach(func => func.UnRegister());
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
        public virtual IEnumerable<IFuncWrap> OnEvtList() => [];
    }
}