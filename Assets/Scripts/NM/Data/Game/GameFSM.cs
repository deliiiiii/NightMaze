using System;
using System.Collections.Generic;
using General;
using GeneralPreview;

namespace NM.Data;
[Serializable]
public class GameFSM : FSM<GameFSM>
{
    public GameFSM()
    {
        Launch<GamePlaying>();
    }
}

[Serializable]
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

