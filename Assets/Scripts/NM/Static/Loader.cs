using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;

namespace NM;

public static class Loader
{
    public static async UniTask LoadAllAsync(CancellationToken? ct = null)
    {
        ct ??= CancellationToken.None;
        var configAll = new List<ConfigBase>(1000);

        foreach (var func in OnLoad?.GetInvocationList() ?? [])
        {
            await ((Func<CancellationToken, UniTask>)func)(ct.Value);
        }
    }

    public static event Func<CancellationToken, UniTask>? OnLoad;
}