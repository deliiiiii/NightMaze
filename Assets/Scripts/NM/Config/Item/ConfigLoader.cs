using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.Config;
[DebuggerStepThrough]
public class ConfigLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bind()
    {
        Loader.OnLoad += async ct =>
        {
            configList = await Resourcer.LoadAssetsAsyncByLabel<ConfigBase>(Const.Res.AddrTag.ConfigTag, ct: ct);
            foreach (var config in configList)
            {
                // config.OnLoad();
                config.AddTo(ct);
            }
        };
    }
    static List<ConfigBase> configList = [];
    public static T Acquire<T>() where T : ConfigSingle<T> =>
        configList.OfType<T>().FirstOrDefault()
        ?? throw new Exception($"没有任何{typeof(T)}的配置.");
    public static T Acquire<T>(int id) where T : ConfigMulti<T> =>
        configList.OfType<T>().FirstOrDefault(c => c.ID == id)
        ?? configList.OfType<T>().FirstOrDefault()
        ?? throw new Exception($"没有任何{typeof(T)}的配置.");
    public static MyOption<T> AcquireOptional<T>() where T : ConfigSingle<T>
    {
        var ret = configList.OfType<T>().FirstOrDefault();
        return ret != null ? ret : MyOption<T>.None;
    }
    public static MyOption<T> AcquireOptional<T>(int id) where T : ConfigMulti<T>
    {
        var ret = configList.OfType<T>().FirstOrDefault(c => c.ID == id);
        return ret != null ? ret : MyOption<T>.None;
    }
}