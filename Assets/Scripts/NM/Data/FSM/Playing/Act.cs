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
    [Obsolete("清空属性")]
    [MuteActEvt]
    UniTask ClearPropAsync(EPropType propType, CancellationToken ct)
    {
        new ActChangeProp(this)
        {
            Delta = -GetProp(propType),
            PropType = propType
        }.Forget();
        return UniTask.CompletedTask;
    }
    [Obsolete("写入属性")]
    UniTask ChangePropAsync(EPropType propType, long delta, CancellationToken ct)
    {
        switch (propType)
        {
            case EPropType.Prop1: PropBody += delta; break;
            case EPropType.Prop2: PropSans += delta; break;
            case EPropType.Prop3: PropLore += delta; break;
            case EPropType.PropA1: PropLoyalty += delta; break;
            case EPropType.PropA2: PropHostility += delta; break;
            default: throw new ArgumentOutOfRangeException(nameof(propType), propType, null);
        }
        return UniTask.CompletedTask;
    }
    [Obsolete("尝试移动某物体")][MuteActEvt]
    async UniTask MoveItemToPosAsync(MyItem item, Vector2Int oldPos, Vector2Int pos, ResultWrap? resultWrap, CancellationToken ct)
    {
        if(oldPos == pos)
            return;
        item.Dragging = true;
        item.PivotPos = pos;
        if (TrySetItem(item))
        {
            await new EvtMoveItem(this, item);
            resultWrap?.Success = true;
            resultWrap?.ItemWraps.Add(new ResultItemWrap(item)
            {
                CtxList = [new ResultItemWrap.CtxSuccessMoved{OldPos = oldPos}]
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

    [Obsolete("尝试在某位置生成某物体")][MuteActEvt]
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

    [Obsolete("领取事件奖励")]
    UniTask ObtainEvtAsync(MyItem item, CancellationToken ct)
    {
        if (!item.Config.IsEvent || !item.IsBuildingOrEventKanSei)
            return UniTask.CompletedTask;
        item.Config.EvtDesResultList.ForEach(des =>
        {
            switch (des)
            {
                case ItemDesResultClearHostility:
                    new ActClearProp(this) { PropType = EPropType.PropA2 }.Forget();
                    break;
                case ItemDesResultUnlockNextLayer:
                    new ActUnlockNextLayer(this).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(des));
            }
        });
        new ActRemoveItem(this)
        {
            ToRemove = item,
            ResultWrap = null
        }.Forget();
        return UniTask.CompletedTask;
    }

    [Obsolete("增加回合数")]
    async UniTask EnterNextTurnAndIdleAsync(CancellationToken ct)
    {
        TurnCount++;
        await ChangeStateAsync(new PlayIdle(), false);
    }
    [Obsolete("解锁下一层")]
    UniTask UnlockNextLayerAsync(CancellationToken ct)
    {
        CurLayer++;
        return UniTask.CompletedTask;
    }
}