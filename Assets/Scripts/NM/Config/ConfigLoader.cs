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
public static class ConfigLoader
{
    static async UniTask<(ELogLevel, string)> LoadToRuntimeAsync(CancellationToken ct)
    {
        var (tempList, logLevel, info) = await Resourcer.LoadAssetsByTagAsync<ConfigBase>(Const.Res.AddrTag.ConfigTag, ct: ct);
        configList = tempList;
        foreach (var config in configList)
        {
            // config.OnLoad();
            config.AddTo(ct);
        }
        return (logLevel, info);
    }
    
    // 进入 Play Mode
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
// #if UNITY_EDITOR
    // 打开项目或代码重新编译
    // [UnityEditor.InitializeOnLoadMethod]
// #endif
    static void Bind()
    {
        Loader.OnLoad -= LoadToRuntimeAsync;
        Loader.OnLoad += LoadToRuntimeAsync;
#if UNITY_EDITOR
        Resourcer.OnReloadEditorResource += ct => LoadToRuntimeAsync(ct);
#endif
    }
    static List<ConfigBase> configList = [];
    public static T Acquire<T>() where T : ConfigSingle<T>
    {
        return configList.OfType<T>().FirstOrDefault()
               ?? throw new Exception($"没有任何{typeof(T)}的配置.");
    }

    public static T Acquire<T>(long id) where T : ConfigMulti<T> =>
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
    public static MyOption<T> AcquireOptional<T>(long id) where T : ConfigMulti<T>
    {
        var ret = configList.OfType<T>().FirstOrDefault(c => c.ID == id);
        return ret != null ? ret : MyOption<T>.None;
    }
}