using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : Node<GameRoot>
{
    static readonly GameRoot root = new();
    static Node? state;

    public static CancellationTokenRegistration AddTo(CancellationToken ct)
        => root.AddTo(ct);
    public static UniTask ChangeStateAsync<T>(T com, bool isNewFromLoad) where T : RootStateBase<T>
         => _ChangeAsync(root, ref state, com, isNewFromLoad);
    public static MyOption<T> GetStateOptional<T>() where T : RootStateBase<T>
        => state is T s ? s : None;
    public static bool IsState<T>() where T : RootStateBase<T>
        => state is T;
}

public abstract class RootStateBase<T> : Node<GameRoot, T>
    where T : RootStateBase<T>;