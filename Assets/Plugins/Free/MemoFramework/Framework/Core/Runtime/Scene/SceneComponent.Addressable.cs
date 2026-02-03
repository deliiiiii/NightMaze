using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace MemoFramework
{
    public partial class SceneComponent
    {
        public class AddressableSceneLoader
        {
            public AddressableSceneLoader(SceneComponent sceneComponent)
            {
                _sceneComponent = sceneComponent;
            }

            private SceneComponent _sceneComponent;
            private Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedSceneHandles = new();

            /// <summary>
            /// 同步加载场景。
            /// </summary>
            /// <param name="sceneAddress">Scene的Address</param>
            /// <param name="mode">加载Scene的模式</param>
            /// <param name="onComplete">加载Scene完成的回调</param>
            public void LoadScene(string sceneAddress, LoadSceneMode mode = LoadSceneMode.Single,
                Action onComplete = null)
            {
                _sceneComponent.IsLoadingScene = true;
                try
                {
                    LoadSceneAsync(sceneAddress, mode).ContinueWith(_ =>
                    {
                        _sceneComponent.IsLoadingScene = false;
                        onComplete?.Invoke();
                    }).Forget();
                }
                catch (Exception)
                {
                    _sceneComponent.IsLoadingScene = false;
                    throw;
                }
            }

            /// <summary>
            /// 使用Addressable异步加载场景。
            /// </summary>
            /// <param name="sceneAddress">Scene的Address</param>
            /// <param name="mode">加载Scene的模式</param>
            /// <returns>异步Task</returns>
            public async UniTask<SceneInstance> LoadSceneAsync(string sceneAddress,
                LoadSceneMode mode = LoadSceneMode.Single)
            {
                _sceneComponent.IsLoadingScene = true;
                try
                {
                    var handle = Addressables.LoadSceneAsync(sceneAddress, mode);
                    _loadedSceneHandles[sceneAddress] = handle;
                    return await handle.ToUniTask().ContinueWith(sceneInstance =>
                    {
                        _sceneComponent.IsLoadingScene = false;
                        return sceneInstance;
                    });
                }
                catch (Exception)
                {
                    _sceneComponent.IsLoadingScene = false;
                    throw;
                }
            }

            /// <summary>
            /// 同步卸载场景。
            /// </summary>
            /// <param name="sceneAddress">Scene的Address</param>
            /// <param name="onComplete">卸载场景完成后</param>
            /// <exception cref="MFException"></exception>
            public void UnloadScene(string sceneAddress, UnityAction onComplete = null)
            {
                if (_loadedSceneHandles.TryGetValue(sceneAddress, out var handle))
                {
                    var unloadHandle = Addressables.UnloadSceneAsync(handle);
                    unloadHandle.Completed += operationHandle =>
                    {
                        _loadedSceneHandles.Remove(sceneAddress);
                        onComplete?.Invoke();
                    };
                }
                else
                {
                    throw new MFException("Address为" + sceneAddress + "的场景未被加载过");
                }
            }

            /// <summary>
            /// 使用Addressable卸载用Addressable加载的场景。
            /// </summary>
            /// <param name="sceneAddress">Scene的Address</param>
            /// <returns>异步Task</returns>
            public async UniTask UnloadSceneAsync(string sceneAddress)
            {
                if (_loadedSceneHandles.TryGetValue(sceneAddress, out var handle))
                {
                    _loadedSceneHandles.Remove(sceneAddress);
                    await Addressables.UnloadSceneAsync(handle).ToUniTask();
                }
                else
                {
                    throw new MFException("Address为" + sceneAddress + "的场景未被加载过");
                }
            }
        }
    }
}