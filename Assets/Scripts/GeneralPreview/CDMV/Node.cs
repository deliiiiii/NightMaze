using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
namespace GeneralPreview;


public class NodeUnit : Node<NodeUnit>
{
    Node? state;
    public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : RootStateBase<T>
        => _ChangeAsync(this, ref state, com, isNewFromLoad);
    
    public abstract class RootStateBase<T> : Node<NodeUnit, T> where T : RootStateBase<T>;
    public class StateTitle : RootStateBase<StateTitle>;
    public class StatePlay : RootStateBase<StatePlay>
    {
        Node? state;
        Node? env;
        public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : PlayStateBase<T>
            => _ChangeAsync(this, ref state, com, isNewFromLoad);
        public UniTask ChangeEnv<T>(T com, bool isNewFromLoad) where T : EnvBase<T>
            => _ChangeAsync(this, ref env, com, isNewFromLoad);

        protected internal override void OnCreateFreshData()
        {
            state = new StateSpin();
            env = new EnvSunState();
        }

        protected internal override async UniTask OnLaunchCom(bool isThisFromLoad)
        {
            await state!.OnCreateAsync(isThisFromLoad);
            await env!.OnCreateAsync(isThisFromLoad);
        }

        protected internal override void OnReleaseCom()
        {
            state?.OnRemove();
            env?.OnRemove();
        }

        public abstract class PlayStateBase<T> : Node<StatePlay, T> where T : PlayStateBase<T>;
        public class StateIdle : PlayStateBase<StateIdle>;
        public class StateSpin : PlayStateBase<StateSpin>
        {
            SpinStateBase? curState;

            public UniTask ChangeState<T>(T com, bool isNewFromLoad) where T : SpinStateBase
                => _ChangeAsync(this, ref curState, com, isNewFromLoad);
            public abstract class SpinStateBase : Node<SpinStateBase>;
            public class StateBefore : SpinStateBase;
            public class StateAfter : SpinStateBase;
        }
        
        public abstract class EnvBase<T> : Node<StatePlay, T> where T : EnvBase<T>;
        public class EnvSunState : EnvBase<EnvSunState>;
        public class EnvRainState : EnvBase<EnvRainState>;
    }
}

public abstract class EttBase //: IDisposable, IHasCt, IHasVersion
{
    internal static int CurID;
    int ettID = CurID++;
}

public abstract class Node
{
    public abstract UniTask OnCreateAsync(bool isThisFromLoad);
    public abstract void OnRemove();
}
public abstract class Node<TThis> : Node, IDisposable, IHasCt, IHasVersion where TThis : Node<TThis>
{
    protected static UniTask _ChangeAsync<TComBase, TComSub>(TThis @this, ref TComBase? field, TComSub com, bool isNewFromLoad) 
        where TComBase : Node
        where TComSub : TComBase
    {
        field?.OnRemove();
        field = com;
        if(field is IHasBelong<TThis> hasBelong)
            hasBelong.BelongData = @this;
        return CallOnCreate();
        async UniTask CallOnCreate()
        {
            await com.OnCreateAsync(isNewFromLoad);
        }
    }
    void IDisposable.Dispose() => OnRemove();
    [JsonIgnore]readonly CancellationTokenSource cts = new();
    public CancellationToken CurCt => cts.Token;
    public double savedVersion { get; set; } = Const.Version;
    public sealed override async UniTask OnCreateAsync(bool isThisFromLoad)
    {
        IUniEvt.BindAll(this, CurCt);
        var tick = OnSelfTick;
        tick.ToBinder().Bind(CurCt);
        if (!isThisFromLoad)
        {
            OnCreateFreshData();
        }
        await new EvtOnEnter((TThis)this);
        await OnLaunchCom(isThisFromLoad);
    }
    /// 如果是全新数据, 初始化子状态和非子状态的数据
    protected internal virtual void OnCreateFreshData(){}
    /// 根据反序列化与否、把数据初始化完后, 启动子状态.
    protected internal virtual UniTask OnLaunchCom(bool isThisFromLoad) => UniTask.CompletedTask;
    /// 清理子状态和非子状态的数据
    protected internal virtual void OnReleaseCom(){}
    public sealed override void OnRemove()
    {
        new EvtOnExit().Forget();
        OnReleaseCom();
        cts.Cancel();
    }
    protected virtual void OnSelfTick(float dt){}

    /// 仅为了通知UI.
    public record EvtOnEnter(TThis WhoHasCt) : EvtBase<TThis>(WhoHasCt);
    /// 仅为了通知UI.
    public record EvtOnExit : EvtForgetBase;
    public abstract record UniAction(TThis Self) : ICanAwait
    {
        [UnityEngine.HideInInspector] protected readonly TThis Self = Self;
        [DebuggerStepThrough] protected abstract UniTask InvokeAsync();
        public UniTask.Awaiter GetAwaiter() 
            => Self.CurCt.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();
        [DebuggerStepThrough] public void Forget() => InvokeAsync().Forget();
    }
}

public abstract class Node<TBelong, TThis> : Node<TThis>, IHasBelong<TBelong>
    where TBelong : class
    where TThis : Node<TBelong, TThis>
{
    public TBelong BelongData { get; set; } = null!;
    // [JsonIgnore]TBelong IHasBelong<TBelong>.BelongData { get => BelongData; set => BelongData = value; }
    // protected TBelong BelongData { get; set; } = null!;
}

public interface IHasBelong<TBelong> where TBelong : class
{
    TBelong BelongData { get; set; }
}


public interface ICanAwait
{
    UniTask.Awaiter GetAwaiter();
}

public static class NodeExt
{
    extension<T>(List<T> self) where T : Node<T>
    {
        public void EachOnCreateFreshData()
        {
            foreach (var node in self) 
                node.OnCreateFreshData();
        }

        public async UniTask EachOnLaunchCom(bool isThisFromLoad)
        {
            foreach (var node in self) 
                await node.OnLaunchCom(isThisFromLoad);
        }
        public void EachOnReleaseCom()
        {
            foreach (var node in self) 
                node.OnReleaseCom();
        }
    }
}