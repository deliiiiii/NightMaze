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
                Instance.ViewList.Where(v => v != null).ForEach(v => v.Unbind());
            }
        };
#endif
    }
    
    public List<ViewBase> ViewList = [];
    protected override void Awake()
    {
        base.Awake();
        Bus.TryClear = true;
    }

    // ReSharper disable once Unity.IncorrectMethodSignature
    // ReSharper disable once UnusedMember.Local
    async UniTask Start()
    {
#if UNITY_EDITOR
        var act = (float _) => Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
        act.ToBinder().Bind(destroyCancellationToken);
#endif
        try
        {
            await Loader.LoadAll();
            MigrateStepRegister.Init();
            ViewList.ForEach(v => v.Bind());
            GameRoot.Root.AddTo(destroyCancellationToken);
            await GameRoot.Root.LaunchAsync();
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
        }
    }
}