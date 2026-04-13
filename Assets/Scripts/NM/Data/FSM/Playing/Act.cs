using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;

namespace NM.Data;
[ActContainer]
public partial class GamePlaying
{
    [Obsolete("尝试移动某物体")][MuteActEvt]
    async UniTask MoveItemToPosAsync(MyItem item, Vector2Int oldPos, Vector2Int pos, ResultWrap? resultWrap, CancellationToken ct)
    {
        if(oldPos == pos)
            return;
        item.Dragging = true;
        if (TrySetItem(item))
        {
            item.PivotPos = pos;
            await new EvtMoveItem(this, item);
            resultWrap?.Success = true;
            resultWrap?.ItemWraps.Add(new ResultItemWrap(item)
            {
                CtxList = [new ResultItemWrap.CtxSuccessMoved{OldPos = oldPos}]
            });
        }
        item.Dragging = false;
    }
    [EvtName("移动了某物体")]
    public record EvtMoveItem(GamePlaying WhoHasCt, MyItem Item) : EvtBase<GamePlaying>(WhoHasCt);

    [Obsolete("尝试在某位置生成某物体")][MuteActEvt]
    async UniTask SpawnItemAtPosAsync(int id, Vector2Int pos, ResultWrap? resultWrap, CancellationToken ct)
    {
        MyItem item = new MyItem(id, pos);
        item.Spawning = true;
        if (!TrySetItem(item))
        {
            return;
        }
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

    public bool TrySetItem(MyItem item) =>
        item switch
        {
            _ when item.Config.IsGrid => item.CoveredPosList.All(pos => !GridPoses.Contains(pos)),
            _ => item.CoveredPosList.All(pos => EmptyGrids.Any(g => g.PivotPos == pos))
        };

    [Obsolete("移除某物体")]
    UniTask RemoveItemAsync(MyItem toRemove, ResultWrap? resultWrap, CancellationToken ct)
    {
        if (itemList.Remove(toRemove))
        {
            resultWrap?.Success = true;
            resultWrap?.ItemWraps.Add(new ResultItemWrap(toRemove)
            {
                CtxList = [new ResultItemWrap.CtxRemoved()]
            });
        }
        return UniTask.CompletedTask;
    }

    [Obsolete("某物添加某物的词条")]
    UniTask ItemEatItemConfigAsync(MyItem whoEat, MyItem toEat, ResultWrap? resultWrap, CancellationToken ct)
    {
        whoEat.EatConfigList.AddRange(toEat.Config.DesList);
        resultWrap?.Success = true;
        return UniTask.CompletedTask;
    }
}