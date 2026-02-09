using System;
using GeneralPreview;
using UnityEngine;

namespace NM.Data;

[Serializable]
public class GameFSM : FSM<GameFSM>
{
    public GameFSM()
    {
        Launch<GameTitle>();
    }
}
[Serializable]
public class GameTitle : GameFSM.IState
{
    public required GameFSM BelongFSM { get; set; }
    public int T;
    public void OnUpdate(float dt)
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            BelongFSM.EnterState<GamePlaying>();
        }
    }
}
[Serializable]
public class GamePlaying : GameFSM.IState
{
    public required GameFSM BelongFSM { get; set; }
    public int T2;
    public void OnUpdate(float dt)
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            BelongFSM.EnterState<GameTitle>();
        }
    }
}