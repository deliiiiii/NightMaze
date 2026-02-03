using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MemoFramework
{
    public partial class SceneComponent
    {
        public class BuiltInSceneLoader
        {
            public BuiltInSceneLoader(SceneComponent sceneComponent)
            {
                _sceneComponent = sceneComponent;
            }

            private SceneComponent _sceneComponent;

            public void LoadScene(int index, LoadSceneMode mode = LoadSceneMode.Single)
            {
                _sceneComponent.IsLoadingScene = true;
                try
                {
                    SceneManager.LoadScene(index, mode);
                }
                catch (Exception)
                {
                    _sceneComponent.IsLoadingScene = false;
                    throw;
                }

                _sceneComponent.IsLoadingScene = false;
            }

            public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
            {
                _sceneComponent.IsLoadingScene = true;
                try
                {
                    SceneManager.LoadScene(sceneName, mode);
                    _sceneComponent.IsLoadingScene = false;
                }
                catch (Exception)
                {
                    _sceneComponent.IsLoadingScene = false;
                    throw;
                }
            }

            public UniTask LoadSceneAsync(int index, LoadSceneMode mode = LoadSceneMode.Single)
            {
                _sceneComponent.IsLoadingScene = true;
                try
                {
                    return SceneManager.LoadSceneAsync(index, mode).ToUniTask()
                        .ContinueWith(() => _sceneComponent.IsLoadingScene = false);
                }
                catch (Exception)
                {
                    _sceneComponent.IsLoadingScene = false;
                    throw;
                }
            }

            public UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
            {
                _sceneComponent.IsLoadingScene = true;
                try
                {
                    return SceneManager.LoadSceneAsync(sceneName, mode).ToUniTask()
                        .ContinueWith(() => _sceneComponent.IsLoadingScene = false);
                }
                catch (Exception)
                {
                    _sceneComponent.IsLoadingScene = false;

                    throw;
                }
            }

            public void UnloadScene(string sceneName, Action onComplete)
            {
                UnloadSceneAsync(sceneName).ContinueWith(onComplete).Forget();
            }

            public void UnloadScene(int index, Action onComplete)
            {
                UnloadSceneAsync(index).ContinueWith(onComplete).Forget();
            }

            public UniTask UnloadSceneAsync(string sceneName)
            {
                return SceneManager.UnloadSceneAsync(sceneName).ToUniTask();
            }

            public UniTask UnloadSceneAsync(int index)
            {
                return SceneManager.UnloadSceneAsync(index).ToUniTask();
            }
        }
    }
}