using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.ViewEvt;

namespace NM.Data;

public partial class PlaySpin
{
    UniEvt<GamePlaying.EvtRemoveItem> OnRemoveItem => new()
    {
        Invoke = (evt, ct) =>
        {
            toDoList.RemoveAll(act => act is ActCheckItem checkItem && checkItem.Item == evt.ToRemove);
            return UniTask.CompletedTask;
        },
        Des = "移除物体时, 移除\"执行它的词条\"行为.",
    };
    
    UniEvt<EvtPlayViewClickNextTurn> OnEvtClickHarvestAsync => new()
    {
        Invoke = (evt, ct) =>
        {
            if (!ToDoList.Any())
            {
                InsertAfter(new GamePlaying.ActEnterNextTurnAndIdle(BelongNode));
            }
            return UniTask.CompletedTask;
        },
        Des = "收获"
    };
}