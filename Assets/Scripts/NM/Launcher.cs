using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using GeneralProj;
using UnityEngine;

namespace NM;

public class Launcher : Singleton<Launcher>
{
    public List<ViewBase> ViewList = [];
    [SerializeReference] GameFSM gameFSM = null!;
    public static GameFSM GameFSM => Instance.gameFSM;
    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Local
    async UniTask Start()
    {
#if UNITY_EDITOR
        Binder.FromTick(_ => Sirenix.Utilities.Editor.GUIHelper.RequestRepaint()).Bind();
#endif
        try
        {
            await Loader.LoadAll();
            ViewList.ForEach(v => v.Bind());
            gameFSM = new();
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
        }
    }
}