using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;

namespace NM.Data;

public partial class GamePlaying
{
    [Obsolete("尝试移动某物体")]
    [MuteActEvt]
    async UniTask MoveItemToPosAsync(MyItem item, Vector2Int oldPos, Vector2Int pos, ResultWrap? resultWrap,
        CancellationToken ct)
    {
        if (oldPos == pos)
            return;
        item.Dragging = true;
        item.PivotPos = pos;
        if (TrySetItem(item))
        {
            await new EvtMoveItem(this, item);
            resultWrap?.Success = true;
            resultWrap?.ItemWraps.Add(new ResultItemWrap(item)
            {
                CtxList = [new ResultItemWrap.CtxSuccessMoved { OldPos = oldPos }]
            });
        }
        else
        {
            item.PivotPos = oldPos;
        }

        item.Dragging = false;
    }

    [EvtName("移动了某物体")]
    public record EvtMoveItem(GamePlaying WhoHasCt, MyItem Item) : EvtBase<GamePlaying>(WhoHasCt);

    [Obsolete("尝试在某位置生成某物体")]
    [MuteActEvt]
    async UniTask SpawnItemAtPosAsync(int id, Vector2Int pos, ResultWrap? resultWrap, CancellationToken ct)
    {
        MyItem item = new MyItem(id, pos)
        {
            Spawning = true
        };
        if (!TrySetItem(item))
            return;
        item.Spawning = false;
        itemList.Add(item);
        await new EvtSpawnItem(this, item);
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(item)
        {
            CtxList = [new ResultItemWrap.CtxSpawned()]
        });
    }

    [EvtName("生成了某物体")]
    public record EvtSpawnItem(GamePlaying WhoHasCt, MyItem Item) : EvtBase<GamePlaying>(WhoHasCt);

    [Obsolete("移除某物体")]
    UniTask RemoveItemAsync(MyItem toRemove, ResultWrap? resultWrap, CancellationToken ct)
    {
        var toRemoves = GetToRemove(toRemove);
        toRemoves.ToList().ForEach(trueToRemove =>
        {
            if (itemList.Remove(trueToRemove))
            {
                resultWrap?.Success = true;
                resultWrap?.ItemWraps.Add(new ResultItemWrap(trueToRemove)
                {
                    CtxList = [new ResultItemWrap.CtxRemoved()]
                });
            }
        });
        return UniTask.CompletedTask;
    }
    
}