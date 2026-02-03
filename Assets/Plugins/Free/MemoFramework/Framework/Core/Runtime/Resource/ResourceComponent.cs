using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace MemoFramework
{
    /// <summary>
    /// MemoFramework资源模组
    /// </summary>
    public class ResourceComponent : MemoFrameworkComponent
    {
        private Dictionary<string, AsyncOperationHandle> _assetHandleCache = new();
        private Dictionary<string, IList<IResourceLocation>> _labelLocationsCache = new();

        /// <summary>
        /// 使用Addressable同步加载资源
        /// </summary>
        /// <param name="address">资源路径</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>加载的资源</returns>
        public T LoadAsset<T>(string address) where T : Object
        {
            if (TryGetAssetFromCache(address, out T asset))
            {
                return asset;
            }

            var handle = Addressables.LoadAssetAsync<Object>(address);

            // 等待加载完成
            handle.WaitForCompletion();

            // 检查是否成功加载
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _assetHandleCache[address] = handle;
                return (T)handle.Result;
            }
            else
            {
                MFLogger.LogError($"加载路径为:{address}的资源失败");
                return null;
            }
        }

        /// <summary>
        /// 使用Addressable异步加载资源
        /// </summary>
        /// <param name="address">资源路径</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>加载的资源</returns>
        public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
        {
            if (TryGetAssetFromCache(address, out T asset))
            {
                return asset;
            }

            // Stopwatch st = new Stopwatch();
            // st.Start();
            AsyncOperationHandle<T> assetHandle = default;
            try
            {
                assetHandle = Addressables.LoadAssetAsync<T>(address);
                await assetHandle.ToUniTask();
            }
            catch (Exception e)
            {
                MFLogger.LogError($"加载路径为:{address}的资源失败:{e.Message},{e.StackTrace}");
                return null;
            }

            if (assetHandle.Status != AsyncOperationStatus.Succeeded)
            {
                MFLogger.LogError($"加载路径为:{address}的资源失败");
            }

            _assetHandleCache[address] = assetHandle;
            // st.Stop();
            // MFLogger.LogInfo($"加载资源{address}用时:{st.Elapsed.TotalMilliseconds}ms");
            return assetHandle.Result;
        }

        /// <summary>
        /// 通过Label加载一组资源
        /// </summary>
        /// <param name="label"></param>
        /// <param name="parallel">并行加载</param>
        /// <exception cref="MFException"></exception>
        public async UniTask LoadAssetsAsyncByLabel(string label, bool parallel = false)
        {
            IList<IResourceLocation> resourceLocations = null;
            // 先试图从缓存中获取Locations
            if (!_labelLocationsCache.ContainsKey(label))
            {
                resourceLocations = await LoadResourceLocationsAsync(label);
            }
            else
            {
                resourceLocations = _labelLocationsCache[label];
            }

            if (resourceLocations.Count == 0)
            {
                throw new MFException($"标签[{label}]定位到的资源地址数量为0");
            }

            Stopwatch st = new Stopwatch();
            st.Start();
            // 加载资源
            List<UniTask<Object>> tasks = parallel ? new List<UniTask<Object>>() : null;
            foreach (var location in resourceLocations)
            {
                if (!parallel)
                {
                    try
                    {
                        await LoadAssetAsync<Object>(location.PrimaryKey);
                    }
                    catch (MFException e)
                    {
                        MFLogger.LogError(e.Message);
                        continue;
                    }
                }
                else
                {
                    tasks.Add(LoadAssetAsync<Object>(location.PrimaryKey));
                }
            }

            if (parallel) await UniTask.WhenAll(tasks);
            st.Stop();
            // MFLogger.LogInfo($"加载标签组{label}资源用时:{st.Elapsed.TotalMilliseconds}ms");
        }

        /// <summary>
        /// 尝试从缓存中获取资源
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="asset">资源</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public bool TryGetAssetFromCache<T>(string address, out T asset) where T : Object
        {
            if (_assetHandleCache.TryGetValue(address, out var handle))
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    asset = (T)handle.Result;
                    return true;
                }
            }

            asset = null;
            return false;
        }

        public T GetAssetFromCache<T>(string address) where T : Object
        {
            if (_assetHandleCache.TryGetValue(address, out var handle))
            {
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return (T)handle.Result;
                }
            }

            return null;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="address">资源地址</param>
        public void Release(string address)
        {
            if (_assetHandleCache.TryGetValue(address, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    _assetHandleCache.Remove(address);
                }
                else
                {
                    MFLogger.LogError("资源句柄无效，可能已经被释放");
                    return;
                }
            }
        }

        /// <summary>
        /// 释放已经加载的含有指定标签的资源
        /// </summary>
        /// <param name="label">标签</param>
        public void ReleaseLabel(string label)
        {
            IList<IResourceLocation> resourceLocations = null;
            // 先试图从缓存中获取Locations
            if (!_labelLocationsCache.ContainsKey(label))
            {
                resourceLocations = LoadResourceLocations(label);
            }
            else
            {
                resourceLocations = _labelLocationsCache[label];
            }

            foreach (var location in resourceLocations)
            {
                if (_assetHandleCache.ContainsKey(location.PrimaryKey))
                {
                    Release(location.PrimaryKey);
                }
            }
        }

        public async UniTask<IList<IResourceLocation>> LoadResourceLocationsAsync(string label)
        {
            if (_labelLocationsCache.ContainsKey(label))
            {
                return _labelLocationsCache[label];
            }

            // 通过标签获取所有资源的位置
            var locatorsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(object));
            await locatorsHandle.ToUniTask();
            if (locatorsHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new MFException($"使用标签[{label}]定位资源地址失败");
            }

            _labelLocationsCache[label] = locatorsHandle.Result.DistinctBy(x => x.PrimaryKey).ToList();
            return _labelLocationsCache[label];
        }

        public IList<IResourceLocation> LoadResourceLocations(string label)
        {
            if (_labelLocationsCache.ContainsKey(label))
            {
                return _labelLocationsCache[label];
            }

            // 通过标签获取所有资源的位置
            var locatorsHandle = Addressables.LoadResourceLocationsAsync(label, typeof(object));
            locatorsHandle.WaitForCompletion();
            if (locatorsHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new MFException($"使用标签[{label}]定位资源地址失败");
            }

            _labelLocationsCache[label] = locatorsHandle.Result;
            return locatorsHandle.Result;
        }
    }
}