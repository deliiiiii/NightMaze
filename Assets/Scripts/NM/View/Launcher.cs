using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.Utilities;

namespace NM.View;
public class Launcher : Singleton<Launcher>
{
    static Launcher()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                GameRoot.Root.Release();
            }
        };
#endif
    }
    
    public List<ViewBase> ViewList = [];
    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Local
    async UniTask Start()
    {
#if UNITY_EDITOR
        Binder.FromTick(_ => Sirenix.Utilities.Editor.GUIHelper.RequestRepaint()).Bind(destroyCancellationToken);
#endif
        try
        {
            await Loader.LoadAll();
            ViewList.ForEach(v => v.Bind());
            await GameRoot.Root.LaunchAsync(new GamePlaying());
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
        }
    }
    void OnDestroy()
    {
        ViewList.Where(v => v != null).ForEach(v => v.Unbind());
    }
}