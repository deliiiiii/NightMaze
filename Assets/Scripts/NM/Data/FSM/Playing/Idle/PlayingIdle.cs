using System;

namespace NM.Data;

[Serializable]
public partial record PlayingIdle : GamePlaying.StateFSM<PlayingIdle>;