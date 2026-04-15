#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace General
{
    public static class Resourcer
    {
        static Resourcer()
        {
            #if UNITY_EDITOR
            // 退出状态时清空缓存，防止编辑器状态下资源泄漏
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    assetHandleCache.Clear();
                    labelLocationsCache.Clear();
                }
            };
            #endif
        }
        
        static readonly Dictionary<string, AsyncOperationHandle> assetHandleCache = new();
        static readonly Dictionary<string, IList<IResourceLocation>> labelLocationsCache = new();

        /// <summary>
        /// 使用Addressable同步加载资源
        /// </summary>
        /// <param name="address">资源路径</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>加载的资源</returns>
        public static T? LoadAsset<T>(string address) where T : Object
        {
            if (TryGetAssetFromCache(address, out T? asset))
                return asset;
            var handle = Addressables.LoadAssetAsync<T>(address);
            handle.WaitForCompletion();
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                assetHandleCache[address] = handle;
                return handle.Result ?? null;
            }
            MyDebug.LogError($"加载路径为:{address}的资源失败");
            return null;
        }

        /// <summary>
        /// 使用Addressable异步加载资源
        /// </summary>
        /// <param name="address">资源路径</param>
        /// <param name="ct">token</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>加载的资源</returns>
        static async UniTask<T?> LoadAssetAsync<T>(string address, CancellationToken? ct = null) where T : Object
        {
            if (TryGetAssetFromCache(address, out T? asset))
                return asset;
            // Stopwatch st = new Stopwatch();
            // st.Start();
            var assetHandle = Addressables.LoadAssetAsync<T>(address);
            await assetHandle.ToUniTask(cancellationToken: ct ?? CancellationToken.None);
            if (assetHandle.Status != AsyncOperationStatus.Succeeded)
                MyDebug.LogError($"加载路径为:{address}的资源失败");
            assetHandleCache[address] = assetHandle;
            // st.Stop();
            // MyDebug.LogInfo($"加载资源{address}用时:{st.Elapsed.TotalMilliseconds}ms");
            return assetHandle.Result as T ?? null;
        }
        
        /// <summary>
        /// 通过Label异步加载一组资源，并返回资源列表
        /// </summary>
        /// <param name="label">资源标签</param>
        /// <param name="ct">token</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载的资源列表</returns>
        public static async UniTask<List<T>> LoadAssetsAsyncByLabel<T>(string label, CancellationToken? ct = null) where T : Object
        {
            // 先试图从缓存中获取Locations
            var resourceLocations = labelLocationsCache.TryGetValue(label, out var value) 
                ? value 
                : await LoadResourceLocationsAsync(label, ct);
            if (!resourceLocations.Any())
            {
                MyDebug.LogWarning($"标签[{label}]定位到的资源地址数量为0");
                return new List<T>();
            }
            Stopwatch st = new Stopwatch();
            st.Start();
            var tasks = resourceLocations.Select(location => LoadAssetAsync<T>(location.PrimaryKey, ct));
            var results = (await UniTask.WhenAll(tasks))
                .Where(asset => asset != null)
                .Select(x=> x!)
                .ToList();
            st.Stop();
            MyDebug.Log($"加载标签组{label}资源用时:{st.Elapsed.TotalMilliseconds}ms");
            return results;
        }
        

        /// <summary>
        /// 尝试从缓存中获取资源
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="asset">资源</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        static bool TryGetAssetFromCache<T>(string address,[NotNullWhen(true)] out T? asset) where T : Object
        {
            if (assetHandleCache.TryGetValue(address, out var handle))
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded && handle.Result is T ret)
                {
                    asset = ret;
                    return true;
                }
            }
            asset = null;
            return false;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="address">资源地址</param>
        static void Release(string address)
        {
            if (!assetHandleCache.TryGetValue(address, out var handle)) 
                return;
            if (!handle.IsValid())
            {
                MyDebug.LogError("资源句柄无效，可能已经被释放");
                return;
            }
            Addressables.Release(handle);
            assetHandleCache.Remove(address);
        }

        /// <summary>
        /// 释放已经加载的含有指定标签的资源
        /// </summary>
        /// <param name="label">标签</param>
        public static void ReleaseLabel(string label)
        {
            // 先试图从缓存中获取Locations
            var resourceLocations = labelLocationsCache.TryGetValue(label, out var value) ? value : LoadResourceLocations(label);
            foreach (var location in resourceLocations)
            {
                if (assetHandleCache.ContainsKey(location.PrimaryKey))
                {
                    Release(location.PrimaryKey);
                }
            }
        }

        static async UniTask<IList<IResourceLocation>> LoadResourceLocationsAsync(string label, CancellationToken? ct = null)
        {
            if (labelLocationsCache.TryGetValue(label, out var value))
                return value;
            // 通过标签获取所有资源的位置
            var locatorsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(object));
            await locatorsHandle.ToUniTask(cancellationToken: ct ?? CancellationToken.None);
            if (locatorsHandle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"使用标签[{label}]定位资源地址失败");
            labelLocationsCache[label] = locatorsHandle.Result.DistinctBy(x => x.PrimaryKey).ToList();
            return labelLocationsCache[label];
        }

        static IList<IResourceLocation> LoadResourceLocations(string label)
        {
            if (labelLocationsCache.TryGetValue(label, out var value))
                return value;
            // 通过标签获取所有资源的位置
            var locatorsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(object));
            locatorsHandle.WaitForCompletion();
            if (locatorsHandle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"使用标签[{label}]定位资源地址失败");
            labelLocationsCache[label] = locatorsHandle.Result;
            return locatorsHandle.Result;
        }
    }
    
    public static class IEnumerableExt
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}

