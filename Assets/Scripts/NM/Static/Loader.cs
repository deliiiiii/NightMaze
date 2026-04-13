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
        configAll.AddRange(await Resourcer.LoadAssetsAsyncByLabel<ConfigBase>(Const.AddrResTag.ConfigTag, ct: ct));
        
        foreach (var config in configAll)
        {
            config.OnLoad();
            config.AddTo(ct.Value);
        }
    }
}