global using static GeneralPreview.MyPrelude;
using System;
using Cysharp.Threading.Tasks;
using General;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;


namespace NM;

public class Launcher : Singleton<Launcher>, IDisposable
{
    // public List<ViewBase> ViewList = [];
    [SerializeReference, ReadOnly] GamePlaying? gamePlaying;
    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Local
    async UniTask Start()
    {
#if UNITY_EDITOR
        // Binder.FromTick(_ => Sirenix.Utilities.Editor.GUIHelper.RequestRepaint()).Bind();
        // 退出游戏模式
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                gamePlaying?.Release();
            }
        };
#endif
        try
        {
            await Loader.LoadAll();
            gamePlaying = new GamePlaying();
            // ObservableSystem.DefaultFrameProvider = new R3.Unity.UnityFrameProvider(); 
            // Observable.EveryUpdate().Subscribe().AddTo(disposables);
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
        }
    }

    public void Dispose()
    {
        // disposables.Dispose();
    }
    
}