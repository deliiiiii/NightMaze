using System;
using Cysharp.Threading.Tasks;
using General;
using NM.Data;
using UnityEngine;

namespace NM;

public class Launcher : Singleton<Launcher>, IDisposable
{
    // public List<ViewBase> ViewList = [];
    [SerializeReference] GameFSM gameFSM = null!;
    // readonly CompositeDisposable disposables = new();
    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Local
    async UniTask Start()
    {
#if UNITY_EDITOR
        // Binder.FromTick(_ => Sirenix.Utilities.Editor.GUIHelper.RequestRepaint()).Bind();
#endif
        try
        {
            await Loader.LoadAll();
            // ViewList.ForEach(v => v.Bind());
            gameFSM = new GameFSM();
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