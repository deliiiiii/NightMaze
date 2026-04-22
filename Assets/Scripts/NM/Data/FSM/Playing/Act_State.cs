using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Sirenix.Utilities;

namespace NM.Data;

public partial class GamePlaying
{
    [Obsolete("等待点击开始回合"), MuteActEvt]
    async UniTask WaitForClickStartTurnAsync(CancellationToken ct)
    {
        await Bus.WaitForAsync<EvtClickStartTurn>("等待点击开始回合", ct);
        InsertAfter((List<IUniAction>)
        [
            new ActStartSpin(this),
            new ActWaitForSpin(this),
            new ActEndSpin(this),
        ]);
    }
    [Obsolete("开始回合")]
    UniTask StartSpinAsync(CancellationToken ct)
    {
        InSpin = new PlaySpin(this);
        return UniTask.CompletedTask;
    }
    [Obsolete("等待回合.."), MuteActEvt]
    UniTask WaitForSpinAsync(CancellationToken ct)
        => InSpin!.WaitForTodoAsync();

    [Obsolete("回合结束")]
    UniTask EndSpinAsync(CancellationToken ct)
    {
        TurnCount++;
        Items.ForEach(item => item.DestroyInSpin());
        InSpin = null;
        InsertAfter(new ActWaitForClickStartTurn(this), act => act is ActEndSpin);
        return UniTask.CompletedTask;
    }
}