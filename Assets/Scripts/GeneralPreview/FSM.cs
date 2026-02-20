using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

    public async UniTask ReleaseAsync()
    {
        if (!isLaunched)
        {
            MyDebug.LogError($"FSM {GetType().Name} Release But NOT Launched"); 
            return;
        }
        isLaunched = false;
        if (curState != null)
        {
            await curState.OnExitAsync();
            curState.UnRegisterAll();
            curState = null;
        }
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
            await curState.OnExitAsync();
            curState.UnRegisterAll();
        }
        curState = (IState)Activator.CreateInstance<TState>()!;
        curState.BelongFSM = (TThis)this;
        curState.RegisterAll();
        await curState.OnEnterAsync();
    }

    public MyOption<TState> InState<TState>() => curState is TState state ? state : None;
    void Tick(float dt) => curState?.OnUpdate(dt);

    public interface IState
    {
        public TThis BelongFSM { get; set; }
        public UniTask OnEnterAsync() => UniTask.CompletedTask;
        public UniTask OnExitAsync() => UniTask.CompletedTask;
        public void OnUpdate(float dt){}
        public bool EnableReEnter => false;
        
        // public IEnumerable<IUniEvt> OnEvt() => [];
        void RegisterAll();
        void UnRegisterAll();
    }
    [Serializable]
    public abstract class StateFSM<TSub> : FSM<TSub>, IState
        where TSub : FSM<TSub>
    {
        public required TThis BelongFSM { get; set; }
        public virtual UniTask OnEnterAsync() => UniTask.CompletedTask;
        public virtual UniTask OnExitAsync() => UniTask.CompletedTask;
        public virtual void OnUpdate(float dt){}
        public virtual bool EnableReEnter => false;
        public virtual void RegisterAll(){}
        public virtual void UnRegisterAll(){}
    }
}