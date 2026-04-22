using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace GeneralPreview;

public interface IUniAction : ICanAwait;

[DebuggerStepThrough]
public abstract record UniAction<TThis>(TThis Self) : IUniAction
    where TThis : IHasCt
{
    [JsonProperty] protected TThis Self = Self;
    protected abstract UniTask InvokeAsync();

    public UniTask.Awaiter GetAwaiter()
        => Self.CurCt.IsCancellationRequested ? UniTask.CompletedTask.GetAwaiter() : InvokeAsync().GetAwaiter();

    public void Forget() => InvokeAsync().Forget();
}