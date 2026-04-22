using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;

namespace GeneralPreview;
[DebuggerStepThrough]
public abstract class DataBase
{
    public abstract UniTask OnCreateAsync(bool isThisFromLoad);
    public abstract void OnRemove();
}
[DebuggerStepThrough]
public abstract class DataBase<TThis> : DataBase, IDisposable, IHasCt, IHasVersion where TThis : DataBase<TThis>
{
    protected UniTask _ChangeAsync<TNode, TNodeSub>(ref TNode? field, TNodeSub node, bool isNewFromLoad) 
        where TNode : DataBase
        where TNodeSub : TNode
    {
        field?.OnRemove();
        field = node;
        if(field is IHasBelong<TThis> hasBelong)
            hasBelong.BelongNode = (TThis)this;
        return CallOnCreate();
        async UniTask CallOnCreate()
        {
            await node.OnCreateAsync(isNewFromLoad);
        }
    }
    void IDisposable.Dispose() => OnRemove();
    [JsonIgnore]readonly CancellationTokenSource cts = new();
    public CancellationToken CurCt { [DebuggerStepThrough] get => cts.Token; }

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
        OnReleaseCom();
        cts.Cancel();
        new EvtOnExit().Forget();
    }

    protected virtual void OnSelfTick(float dt)
    {
        new EvtOnTick((TThis)this, dt).Forget();
    }

    /// 仅为了通知UI.
    public record EvtOnEnter(TThis WhoHasCt) : EvtBase<TThis>(WhoHasCt);
    /// 仅为了通知UI.
    public record EvtOnExit : EvtForgetBase;
    public record EvtOnTick(TThis Self, float Dt) : EvtForgetBase;
}

[DebuggerStepThrough]
public abstract class DataBase<TBelong, TThis> : DataBase<TThis>, IHasBelong<TBelong>
    where TBelong : class
    where TThis : DataBase<TBelong, TThis>
{
    [JsonIgnore]TBelong IHasBelong<TBelong>.BelongNode { get => BelongNode; set => BelongNode = value; }
    protected TBelong BelongNode { get; set; } = null!;
}
interface IHasBelong<TBelong> where TBelong : class
{
    TBelong BelongNode { get; set; }
}
public interface ICanAwait
{
    UniTask.Awaiter GetAwaiter();
}