using GeneralPreview;

namespace NM.Data;

public class GameRoot : FSM<GameRoot>
{
    public static readonly GameRoot Root = new();
}