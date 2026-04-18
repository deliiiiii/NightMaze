using System.Collections.Generic;
using System.Threading;
using General;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace GeneralPreview;

public abstract class ViewBase : SerializedMonoBehaviour
{
    protected virtual IEnumerable<BindDataBase> BindList() => [];
    bool bind;
    CancellationTokenSource manualCts;

    public void Bind(CancellationToken? ct = null)
    {
        if (bind)
            return;
        // MyDebug.Log($"{GetType().GetNiceName()} Bind");
        manualCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, ct ?? CancellationToken.None);
        BindList().ForEach(b => b.Bind(manualCts.Token));
        IUniEvt.BindAll(this, manualCts.Token);
        bind = true;
    }
    public void Unbind()
    {
        if (!bind)
            return;
        // MyDebug.Log($"{GetType().GetNiceName()} Unbind");
        bind = false;
        manualCts.Cancel();
    }

    void Awake()
    {
        if(!bind)
            Bind();
    }

    void OnDestroy()
    {
        // MyDebug.Log($"{GetType().GetNiceName()} OnDestroy");
        if(bind)
            Unbind();
    }
}

public abstract class ViewBase<TData> : ViewBase where TData : class
{
    [ShowInInspector, ReadOnly] public TData Data { get; set; } = null!;
}