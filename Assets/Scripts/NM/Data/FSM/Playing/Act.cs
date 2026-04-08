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
    [Obsolete("移动 某物体 到 某位置")][MuteActEvt]
    async UniTask MoveItemToPosAsync(IItem item, Vector2Int oldPos, Vector2Int pos, ResultWrap? resultWrap, CancellationToken ct)
    {
        if(oldPos == pos)
            return;
        item.Dragging = true;
        if (TrySetItem(item))
        {
            item.PivotPos = pos;
            await new EvtMoveItem(this, item);
            resultWrap?.ItemWraps.Add(new ResultItemWrap(item)
            {
                CtxList = [new ResultItemWrap.CtxSuccessMoved{OldPos = oldPos}]
            });
        }
        else
        {
            resultWrap?.Success = false;
        }
        item.Dragging = false;
    }   
    public record EvtMoveItem(GamePlaying WhoHasCt, IItem Item) : EvtBase<GamePlaying>(WhoHasCt);

    [Obsolete("在某位置 生成 某物体")][MuteActEvt]
    async UniTask SpawnItemAtPosAsync(EItemType type, int id, Vector2Int pos, ResultWrap? resultWrap, CancellationToken ct)
    {
        IItem item = type switch
        {
            EItemType.Grid => new Grid(id, pos),
            EItemType.Symbol => new Symbol(id, pos),
            EItemType.Building => new Building(id, pos),
            EItemType.Resource => new Resource(id, pos),
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(EItemType)}类型: {type}.")
        };
        item.Spawning = true;
        if (!TrySetItem(item))
        {
            resultWrap?.Success = false;
            return;
        }
        item.Spawning = false;
        itemList.Add(item);
        await new EvtSpawnItem(this, item);
        resultWrap?.ItemWraps.Add(new ResultItemWrap(item)
        {
            CtxList = [new ResultItemWrap.CtxSpawned()]
        });
    }
    public record EvtSpawnItem(GamePlaying WhoHasCt, IItem Item) : EvtBase<GamePlaying>(WhoHasCt);

    public bool TrySetItem(IItem item)
    {
        return item switch
        {
            Grid => item.CoveredPosList.All(pos => !GridPoses.Contains(pos)),
            _ => item.CoveredPosList.All(pos => EmptyGrids.Any(g => ((IItem)g).PivotPos == pos))
        };
    }
    
    [Obsolete("移除某物体")]
    UniTask RemoveItemAsync(IItem toRemove, ResultWrap? resultWrap, CancellationToken ct)
    {
        if (itemList.Remove(toRemove))
        {
            resultWrap?.ItemWraps.Add(new ResultItemWrap(toRemove)
            {
                CtxList = [new ResultItemWrap.CtxRemoved()]
            });
        }
        else
        {
            resultWrap?.Success = false;
        }
        return UniTask.CompletedTask;
    }

    [Obsolete("某物添加某物的词条")]
    UniTask ItemEatItemConfigAsync(IItem whoEat, IItem toEat, ResultWrap? resultWrap, CancellationToken ct)
    {
        whoEat.EatConfigList.AddRange(toEat.Config.DesList);
        return UniTask.CompletedTask;
    }
}