using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;

namespace NM;

public static class Loader
{
    public static async UniTask LoadAllAsync(CancellationToken ct)
    {
        foreach (var func in OnLoad?.GetInvocationList() ?? [])
        {
            await ((Func<CancellationToken, UniTask<(ELogLevel, string)>>)func)(ct);
        }
    }
    public static event Func<CancellationToken, UniTask<(ELogLevel, string)>>? OnLoad;
}