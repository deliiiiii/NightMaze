using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.Config;
[DebuggerStepThrough]
public class ConfigLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    static void Bind()
    {
        async UniTask Func(CancellationToken ct)
        {
            configList = await Resourcer.LoadAssetsAsyncByLabel<ConfigBase>(Const.Res.AddrTag.ConfigTag, ct: ct);
            foreach (var config in configList)
            {
                // config.OnLoad();
                config.AddTo(ct);
            }
        }
        Loader.OnLoad += Func;
#if UNITY_EDITOR
        Resourcer.OnReloadEditorResource += Func;
#endif
    }
    static List<ConfigBase> configList = [];
    public static T Acquire<T>() where T : ConfigSingle<T>
    {
        return configList.OfType<T>().FirstOrDefault()
               ?? throw new Exception($"没有任何{typeof(T)}的配置.");
    }

    public static T Acquire<T>(int id) where T : ConfigMulti<T> =>
        configList.OfType<T>().FirstOrDefault(c => c.ID == id)
        ?? configList.OfType<T>().FirstOrDefault()
        ?? throw new Exception($"没有任何{typeof(T)}的配置.");
    public static IEnumerable<T> AcquireSome<T>(Func<T, bool> predicate) where T : ConfigMulti<T> =>
        configList.OfType<T>().Where(predicate);
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