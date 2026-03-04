using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : FSM<GameRoot>
{
    public static readonly GameRoot Root = new();
    protected override IState InitState => new GameTitle();
    
    public new UniTask LaunchAsync() => base.LaunchAsync();
}