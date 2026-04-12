//Mono单例

using System;
using System.Diagnostics;
using UnityEngine;

namespace General
{
    [DebuggerStepThrough]
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public bool GlobalOnScene;

        static T instance;
        public static bool HasInstance => instance != null;
        protected static T Instance
        {
            get
            {
                instance ??= FindAnyObjectByType<T>();
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return instance;
#endif
                return instance ??= new GameObject(typeof(T).Name).AddComponent<T>();
            }
        }

        protected virtual void Awake()
        {
            if(name == "New Game Object")
                name = GetType().ToString();
            if(Instance && Instance != this)
            {
                // duplicate!!!
                MyDebug.LogError($"{typeof(T)} already exists on {name}, destroying the new instance.");
                Destroy(Instance.gameObject);
            }
            if (GlobalOnScene)
            {
                DontDestroyOnLoad(gameObject);
            }
            destroyCancellationToken.Register(() =>
            {
                // MyDebug.Log($"{typeof(T)} Destroy ins");
                instance = null;
            });
        }
    }
}