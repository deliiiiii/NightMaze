using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial class PlaySpin
{
    [Obsolete("等待点击下一回合按钮")]
    async UniTask WaitForClickNextTurnAsync(CancellationToken ct)
    {
        await Bus.WaitForAsync<GamePlaying.EvtClickNextTurn>("点击下一回合按钮", ct);
    }
}