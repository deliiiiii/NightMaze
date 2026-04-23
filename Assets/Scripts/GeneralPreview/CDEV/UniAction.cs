using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace GeneralPreview;

public interface IUniAction : ICanAwait
{
    void CancelSelfly();
    bool IsCancelledSelfly { get; }
}

[DebuggerStepThrough]
public abstract record UniAction<TThis>(TThis Self) : IUniAction
    where TThis : IHasCt
{
    [JsonProperty] protected TThis Self = Self;
    [JsonIgnore] CancellationTokenSource cts = new();
    [JsonIgnore] bool isCancelledSelfly = false;
    bool IUniAction.IsCancelledSelfly => isCancelledSelfly;

    protected CancellationTokenSource LinkedCts =>
        field ??= CancellationTokenSource.CreateLinkedTokenSource(cts.Token, Self.CurCt);
    protected abstract UniTask InvokeAsync();

    public UniTask.Awaiter GetAwaiter()
        => LinkedCts.IsCancellationRequested ? UniTask.FromCanceled(LinkedCts.Token).GetAwaiter() : InvokeAsync().GetAwaiter();

    public void Forget() => InvokeAsync().Forget();
    public void CancelSelfly()
    {
        if(LinkedCts.IsCancellationRequested)
            return;
        isCancelledSelfly = true;
        LinkedCts.Cancel();
    }
}