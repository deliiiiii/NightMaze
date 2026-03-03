using GeneralPreview;

namespace NM.Data;

public partial class GameRoot : FSM<GameRoot>
{
    public static readonly GameRoot Root = new();
}