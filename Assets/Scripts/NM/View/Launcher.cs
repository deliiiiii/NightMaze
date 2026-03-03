using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;

namespace NM.View;

public class Launcher : Singleton<Launcher>
{
    public List<ViewBase> ViewList = [];
    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Local
    async UniTask Start()
    {
#if UNITY_EDITOR
        Binder.FromTick(_ => Sirenix.Utilities.Editor.GUIHelper.RequestRepaint()).Bind();
        // 退出游戏模式
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                GameRoot.Root.Release();
            }
        };
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
}