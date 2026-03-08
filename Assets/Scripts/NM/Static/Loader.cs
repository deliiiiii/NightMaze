using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;

namespace NM;

public static class Loader
{
    public static async UniTask LoadAll()
    {
        var configAll = new List<ConfigBase>(1000);
        configAll.AddRange(await Resourcer.LoadAssetsAsyncByLabel<ConfigBase>(NameC.ConfigTag));
        
        foreach (var config in configAll)
        {
            config.OnLoad();
        }
    }
}