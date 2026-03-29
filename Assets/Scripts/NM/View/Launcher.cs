using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;

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
                // Instance.ViewList.Where(v => v != null).ForEach(v => v.Unbind());
                ViewStatic.UnBindAll();
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
            MyInput.Init(destroyCancellationToken);
            await Loader.LoadAllAsync(destroyCancellationToken);
            MigrateStepRegister.Init();
            ViewStatic.BindAll();
            GameRoot.AddTo(destroyCancellationToken);
            await GameRoot.ChangeStateAsync(new GameTitle(), false);
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
        }
    }
}