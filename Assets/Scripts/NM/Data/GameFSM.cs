using System;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.Data;

public class GameFSM : FSM<GameFSM>
{
    public GameFSM()
    {
        Launch<GamePlaying>();
    }
}

public class GamePlaying : GameFSM.StateFSM<GamePlaying>
{
    public override void OnEnter()
    {
        Launch<PlayingInit>();
    }
}

public class PlayingInit : GamePlaying.IState
{
    public required GamePlaying BelongFSM { get; set; }

    public void OnEnter()
    {
        MyDebug.Log($"{nameof(PlayingInit)} OnEnter");
    }
}