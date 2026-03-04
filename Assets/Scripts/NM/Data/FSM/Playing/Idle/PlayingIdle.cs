using System;

namespace NM.Data;

[Serializable]
public class PlayingIdle : GamePlaying.StateFSM<PlayingIdle>
{
    protected override IState InitState => null!;
    public override string ToString() => nameof(PlayingIdle);
}