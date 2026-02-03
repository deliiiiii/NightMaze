//Mono单例
//需要被继承 xxx : Singleton<xxx>
//获取单例 xxx.Instance

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public bool GlobalOnScene;

    static T instance;
    protected static T Instance
    {
        get
        {
            instance ??= FindObjectOfType<T>();
            instance ??= new GameObject().AddComponent<T>();
            return instance;
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
    }

#if UNITY_EDITOR
    void OnPlayModeStateChanged(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.ExitingPlayMode && instance != null)
        {
            // DestroyImmediate(gameObject);
            instance = null;
        }
    }
#endif
    void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }
    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
    }
}