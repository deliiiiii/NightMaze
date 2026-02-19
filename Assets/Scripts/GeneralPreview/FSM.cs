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

    protected void Launch<TState>()
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

    protected void Release()
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
    public void EnterState<TState>()
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
            curState.OnExit();
            curState.UnRegisterAll();
        }
        curState = (IState)Activator.CreateInstance<TState>()!;
        curState.BelongFSM = (TThis)this;
        curState.RegisterAll();
        curState.OnEnter();
    }

    public void EnterStateIfNotIn<TState>() => InState<TState>().MatchA(none: EnterState<TState>);
    public MyOption<TState> InState<TState>() => curState is TState state ? state : None;
    void Tick(float dt) => curState?.OnUpdate(dt);
    interface IState
    {
        public TThis BelongFSM { get; set; }
        public void OnEnter(){}
        public void OnExit(){}
        public void OnUpdate(float dt){}
        public bool EnableReEnter => false;
        
        public IEnumerable<IUniEvt> OnEvt() => [];
        void RegisterAll() => OnEvt().RegAll();
        void UnRegisterAll() => OnEvt().UnRegAll();
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
        public virtual IEnumerable<IUniEvt> OnEvt() => [];
    }
}