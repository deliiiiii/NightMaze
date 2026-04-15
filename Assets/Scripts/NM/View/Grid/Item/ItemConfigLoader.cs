using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.View;

public class ItemConfigLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bind()
    {
        Loader.OnLoad += async ct =>
        {
            MyDebug.Log("加载ItemConfig... 1");
            ConfigList = await Resourcer.LoadAssetsAsyncByLabel<ConfigBase>(Const.AddrResTag.ConfigTag, ct: ct);
            foreach (var config in ConfigList)
            {
                config.OnLoad();
                config.AddTo(ct);
            }
            MyDebug.Log("加载ItemConfig... 2 count:" + ConfigList.Count);
        };
    }
    public static List<ConfigBase> ConfigList = [];
}