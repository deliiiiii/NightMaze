using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : Node<GameRoot>
{
    static GameRoot()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                instance.state = null;
            }
        };
#endif
    }
    static readonly GameRoot instance = new();
    Node? state;

    public static CancellationTokenRegistration AddTo(CancellationToken ct)
        => instance.AddTo(ct);
    public static UniTask ChangeStateAsync<T>(T com, bool isNewFromLoad) where T : RootStateBase<T>
         => instance._ChangeAsync(ref instance.state, com, isNewFromLoad);
    public static MyOption<T> GetStateOptional<T>() where T : RootStateBase<T>
        => instance.state is T s ? s : None;
    public static bool IsState<T>() where T : RootStateBase<T>
        => instance.state is T;
    
    protected override void OnReleaseCom()
    {
        state?.OnRemove();
    }
}

public abstract class RootStateBase<T> : Node<GameRoot, T>
    where T : RootStateBase<T>;