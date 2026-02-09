using GeneralPreview;

namespace NM.Data;
public class GameFSM : FSM<GameFSM>
{
    public GameFSM()
    {
        Launch<GamePlaying>();
    }
}

public class GameTitle : GameFSM.StateFSM<GameTitle>
{
    public override void OnEnter()
    {
    }
    public override void OnExit()
    {
    }
}

public partial class GamePlaying : GameFSM.StateFSM<GamePlaying>;

