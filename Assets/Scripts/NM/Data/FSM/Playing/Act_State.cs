using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
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
            new ActWaitForSelectSymbol(this)
            {
                ToSelectConfigs = [..ConfigLoader
                    .AcquireSome<ItemConfig>(config => config.IsSymbol)
                    .ToList().ShuffleTo()
                    .Take(3)
                    .Select(x => x.ID)
                ]
            },
            new ActEndSpin(this),
        ]);
    }
    [Obsolete("开始回合")]
    UniTask StartSpinAsync(CancellationToken ct)
    {
        InSpin = new PlaySpin(this);
        Items.ForEach(item => item.DestroyInSpin());
        return UniTask.CompletedTask;
    }
    [Obsolete("等待回合.."), MuteActEvt]
    UniTask WaitForSpinAsync(CancellationToken ct) => 
        InSpin!.WaitForTodoAsync();
    [Obsolete("等待选择棋子")]
    async UniTask WaitForSelectSymbolAsync(List<int> toSelectConfigs, CancellationToken ct)
    {
        new EvtStartSelectSymbol(toSelectConfigs).Forget();
        var evt = await Bus.WaitForAsync<EvtClickSelectSymbol>("等待选择棋子", ct);
        if (evt.SelectedID != null)
        {
            MyDebug.LogWarning("选择了棋子：" + evt.SelectedID);
        }
    }

    public record EvtStartSelectSymbol(List<int> ToSelectIDs) : EvtForgetBase;
    public record EvtClickSelectSymbol(int? SelectedID) : EvtForgetBase;
    [Obsolete("回合结束")]
    UniTask EndSpinAsync(CancellationToken ct)
    {
        TurnCount++;
        InsertAfter(new ActWaitForClickStartTurn(this), act => act is ActEndSpin);
        return UniTask.CompletedTask;
    }
}