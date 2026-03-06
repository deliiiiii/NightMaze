using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial record GameRoot : FSM<GameRoot>
{
    public static readonly GameRoot Root = new();
    public UniTask LaunchAsync() => base.LaunchAsync(new GameTitle(), false);
}