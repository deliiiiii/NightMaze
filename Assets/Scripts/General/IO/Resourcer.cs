using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

public static class Resourcer
    {
        static Dictionary<string, AsyncOperationHandle<Object>> _assetHandleCache = new();
        static Dictionary<string, IList<IResourceLocation>> _labelLocationsCache = new();

        /// <summary>
        /// 使用Addressable同步加载资源
        /// </summary>
        /// <param name="address">资源路径</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>加载的资源</returns>
        [CanBeNull]
        public static T LoadAsset<T>(string address) where T : Object
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
                MyDebug.LogError($"加载路径为:{address}的资源失败");
                return null;
            }
        }

        /// <summary>
        /// 使用Addressable异步加载资源
        /// </summary>
        /// <param name="address">资源路径</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>加载的资源</returns>
        public static async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
        {
            if (TryGetAssetFromCache(address, out T asset))
            {
                return asset;
            }

            // Stopwatch st = new Stopwatch();
            // st.Start();
            AsyncOperationHandle<Object> assetHandle = default;
            try
            {
                assetHandle = Addressables.LoadAssetAsync<Object>(address);
                await assetHandle.ToUniTask();
            }
            catch (Exception e)
            {
                MyDebug.LogError($"加载路径为:{address}的资源失败:{e.Message},{e.StackTrace}");
                return null;
            }

            if (assetHandle.Status != AsyncOperationStatus.Succeeded)
            {
                MyDebug.LogError($"加载路径为:{address}的资源失败");
            }

            _assetHandleCache[address] = assetHandle;
            // st.Stop();
            // MyDebug.LogInfo($"加载资源{address}用时:{st.Elapsed.TotalMilliseconds}ms");
            return (T)assetHandle.Result;
        }

        // /// <summary>
        // /// 通过Label加载一组资源
        // /// </summary>
        // /// <param name="label"></param>
        // /// <param name="parallel">并行加载</param>
        // /// <exception cref="MFException"></exception>
        // public static async UniTask LoadAssetsAsyncByLabel(string label, bool parallel = false)
        // {
        //     IList<IResourceLocation> resourceLocations = null;
        //     // 先试图从缓存中获取Locations
        //     if (!_labelLocationsCache.ContainsKey(label))
        //     {
        //         resourceLocations = await LoadResourceLocationsAsync(label);
        //     }
        //     else
        //     {
        //         resourceLocations = _labelLocationsCache[label];
        //     }
        //
        //     if (resourceLocations.Count == 0)
        //     {
        //         throw new Exception($"标签[{label}]定位到的资源地址数量为0");
        //     }
        //
        //     Stopwatch st = new Stopwatch();
        //     st.Start();
        //     // 加载资源
        //     List<UniTask<Object>> tasks = parallel ? new List<UniTask<Object>>() : null;
        //     foreach (var location in resourceLocations)
        //     {
        //         if (!parallel)
        //         {
        //             try
        //             {
        //                 await LoadAssetAsync<Object>(location.PrimaryKey);
        //             }
        //             catch (Exception e)
        //             {
        //                 MyDebug.LogError(e.Message);
        //                 continue;
        //             }
        //         }
        //         else
        //         {
        //             tasks.Add(LoadAssetAsync<Object>(location.PrimaryKey));
        //         }
        //     }
        //
        //     if (parallel) await UniTask.WhenAll(tasks);
        //     st.Stop();
        //     // MyDebug.LogInfo($"加载标签组{label}资源用时:{st.Elapsed.TotalMilliseconds}ms");
        // }
        
        /// <summary>
        /// 通过Label异步加载一组资源，并返回资源列表
        /// </summary>
        /// <param name="label">资源标签</param>
        /// <param name="parallel">是否并行加载, 默认为true</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns>加载的资源列表</returns>
        public static async UniTask<List<T>> LoadAssetsAsyncByLabel<T>(string label, bool parallel = true) where T : Object
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
                throw new Exception($"标签[{label}]定位到的资源地址数量为0");
            }

            Stopwatch st = new Stopwatch();
            st.Start();
            List<T> loadedAssets = new List<T>();
            List<UniTask<T>> tasks = parallel ? new List<UniTask<T>>() : null;
            try
            {
                foreach (var location in resourceLocations)
                {
                    
                        if (!parallel)
                        {
                            var asset = await LoadAssetAsync<T>(location.PrimaryKey);
                            if (asset != null)
                            {
                                loadedAssets.Add(asset);
                            }
                            continue;
                        }
                        tasks.Add(LoadAssetAsync<T>(location.PrimaryKey));
                    
                }

                if (parallel)
                {
                    var results = await UniTask.WhenAll(tasks);
                    loadedAssets.AddRange(results.Where(asset => asset != null));
                }
            }
            catch (Exception e)
            {
                MyDebug.LogError($"{typeof(T)} {e.Message}");
            }

            st.Stop();
            MyDebug.Log($"加载标签组{label}资源用时:{st.Elapsed.TotalMilliseconds}ms");
            return loadedAssets;
        }
        

        /// <summary>
        /// 尝试从缓存中获取资源
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="asset">资源</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static bool TryGetAssetFromCache<T>(string address, out T asset) where T : Object
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

        public static T GetAssetFromCache<T>(string address) where T : Object
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
        public static void Release(string address)
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
                    MyDebug.LogError("资源句柄无效，可能已经被释放");
                    return;
                }
            }
        }

        /// <summary>
        /// 释放已经加载的含有指定标签的资源
        /// </summary>
        /// <param name="label">标签</param>
        public static void ReleaseLabel(string label)
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

        public static async UniTask<IList<IResourceLocation>> LoadResourceLocationsAsync(string label)
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
                throw new Exception($"使用标签[{label}]定位资源地址失败");
            }

            _labelLocationsCache[label] = locatorsHandle.Result.DistinctBy(x => x.PrimaryKey).ToList();
            return _labelLocationsCache[label];
        }

        public static IList<IResourceLocation> LoadResourceLocations(string label)
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
                throw new Exception($"使用标签[{label}]定位资源地址失败");
            }

            _labelLocationsCache[label] = locatorsHandle.Result;
            return locatorsHandle.Result;
        }
        
        
    }

