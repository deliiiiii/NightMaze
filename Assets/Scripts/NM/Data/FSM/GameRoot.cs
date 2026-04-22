using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : DataBase<GameRoot>
{
    static GameRoot()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                instance.state = null;
                instance.SettingData = null!;
            }
        };
#endif
    }
    static readonly GameRoot instance = new();
    DataBase? state;

    [field:MaybeNull]SettingData SettingData
    {
        get
        {
            if (field == null)
            {
                var data = Saver.Load<SettingData>(Const.Name.Save.SettingFolder, Const.Name.Save.SettingName);
                if (data == null)
                {
                    data = new SettingData();
                    Saver.SaveAsync(Const.Name.Save.SettingFolder, Const.Name.Save.SettingName, data).Forget();
                }
                field = data;
            }
            return field;
        }
        set;
    }

    public static CancellationTokenRegistration AddTo(CancellationToken ct)
        => instance.AddTo(ct);
    public static UniTask ChangeStateAsync<T>(T node, bool isNewFromLoad) where T : RootStateBase<T>
         => instance._ChangeAsync(ref instance.state, node, isNewFromLoad);
    public static MyOption<T> GetStateOptional<T>() where T : RootStateBase<T>
        => instance.state is T s ? s : None;
    public static bool IsState<T>() where T : RootStateBase<T>
        => instance.state is T;

    public static SettingData Setting => instance.SettingData;
    
    protected override void OnReleaseCom()
    {
        state?.OnRemove();
    }
}

public abstract class RootStateBase<T> : DataBase<GameRoot, T>
    where T : RootStateBase<T>;