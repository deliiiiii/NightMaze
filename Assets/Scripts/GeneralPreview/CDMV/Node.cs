using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using Newtonsoft.Json;
using Sirenix.Utilities;

namespace GeneralPreview;

public abstract record EttBase
{
    internal static int CurID;
}
public abstract record EttBase<T> : EttBase where T : EttBase<T>, new()
//: IDisposable, IHasCt, IHasVersion
{
    // public abstract class ComBase
    // {
    //     [JsonIgnore] public T BelongEtt { get; internal set; } = null!;
    // }
    public int EttID { get; init; } = CurID++;
    public static T Create() => new T();
}

public abstract class Node
{
    public abstract UniTask OnCreateAsync(bool isThisFromLoad);
    public abstract void OnRemove();
}
public abstract class Node<TThis> : Node, IDisposable, IHasCt, IHasVersion where TThis : Node<TThis>
{
    protected interface INodeCom
    {
        EttBase BelongEtt { get; set; }
    }
    public abstract class INodeCom<TEtt, TCom> : INodeCom
        where TEtt : EttBase<TEtt>, new()
        where TCom : INodeCom<TEtt, TCom>
    {
        // public TEtt BelongEtt { get; set; } = null!;
        [JsonIgnore]public TEtt BelongEtt { get; set; } = null!;
        [JsonIgnore]EttBase INodeCom.BelongEtt
        {
            get => BelongEtt;
            set => BelongEtt = (TEtt)value;
        }
        // EttBase INodeCom.BelongEttBase
        // {
        //     get => BelongEtt;
        //     set => value = BelongEtt;
        // }
    }
    // [0 : XxxInXxx, ...]
    [JsonIgnore]Dictionary<EttBase, INodeCom> comDic = [];
    [JsonProperty("comDic")]
    List<KeyValuePair<EttBase, INodeCom>> SerializableComDic
    {
        get => comDic.ToList();
        set
        {
            comDic = value.ToDictionary(x => x.Key, x => x.Value);
            comDic.ForEach(pair =>
            {
                pair.Value.BelongEtt = pair.Key;
            });
        }
    }

    protected MyOption<TCom> GetEttCom<TEtt, TCom>(TEtt ett)
        where TEtt : EttBase<TEtt>, new()
        where TCom : INodeCom<TEtt, TCom>
    {
        if (comDic.TryGetValue(ett, out var com))
                return (TCom)com;
        MyDebug.LogError($"{GetType().GetNiceName()}中未找到EttID:{ett.EttID}的组件. 将提供默认值");
        return None;
    }

    protected TCom AddEttCom<TEtt, TCom>(TEtt ett, TCom com)
        where TEtt : EttBase<TEtt>, new()
        where TCom : INodeCom<TEtt, TCom>
    {
        if(comDic.TryGetValue(ett, out var oldCom))
        {
            MyDebug.LogError($"在{GetType().GetNiceName()}中EttID:{ett.EttID}已有组件{oldCom.GetType().GetNiceName()}.");
            return (TCom)oldCom;
        }
        comDic[ett] = com;
        com.BelongEtt = ett;
        return com;
    }

    protected void RemoveEttCom<TEtt>(TEtt ett)
        where TEtt : EttBase<TEtt>, new()
    // where TCom : EttBase<TEtt>.ComBase, INodeCom
    {
        if (comDic.Remove(ett))
            return;
        MyDebug.LogError($"在{GetType().GetNiceName()}中未找到EttID:{ett.EttID}的组件，无法移除.");
    }

    protected IEnumerable<TCom> GetComs<TCom>() 
        => comDic.Values.OfType<TCom>();

    protected UniTask _ChangeAsync<TComBase, TComSub>(ref TComBase? field, TComSub com, bool isNewFromLoad) 
        where TComBase : Node
        where TComSub : TComBase
    {
        field?.OnRemove();
        field = com;
        if(field is IHasBelong<TThis> hasBelong)
            hasBelong.BelongNode = (TThis)this;
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
        OnReleaseCom();
        cts.Cancel();
        new EvtOnExit().Forget();
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
    public TBelong BelongNode { get; set; } = null!;
    // [JsonIgnore]TBelong IHasBelong<TBelong>.BelongData { get => BelongData; set => BelongData = value; }
    // protected TBelong BelongData { get; set; } = null!;
}

public interface IHasBelong<TBelong> where TBelong : class
{
    TBelong BelongNode { get; set; }
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