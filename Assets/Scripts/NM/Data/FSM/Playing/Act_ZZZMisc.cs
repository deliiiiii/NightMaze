using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;

[ActContainer]
public partial class GamePlaying
{
    [Obsolete("Do Nothing")]
    UniTask DoNothingAsync(CancellationToken ct) => UniTask.CompletedTask;
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
        InsertButCancelFirstAndDoFirst([
            new ActRemoveItem(this)
            {
                ToRemove = item,
                ResultWrap = null
            },
            ..item.Config.EvtDesResultList.Select<EvtDesResultBase, IUniAction>(des => des switch
            {
                ItemDesResultClearHostility => new ActClearProp(this) { PropType = EPropType.PropA2 },
                ItemDesResultUnlockAdjacentArea adj => new ActUnlockArea(this)
                {
                    AreaPos = GetAreaPos(item.PivotPos) + adj.Delta,
                },
                // 弃置
                ItemDesResultUnlockNextLayer => new ActDoNothing(this),
                _ => throw new ArgumentOutOfRangeException(nameof(des))
            }),
        ]);
        return UniTask.CompletedTask;
    }
    
    [Obsolete("解锁指定区域")]
    UniTask UnlockAreaAsync(Vector2Int areaPos, CancellationToken ct)
    {
        if(RevealedAreaSet.Contains(areaPos))
            return UniTask.CompletedTask;
        if(!InAreaMaxRange(areaPos))
            return UniTask.CompletedTask;
        RevealedAreaSet.Add(areaPos);
        // 揭露地块.
        var toRevealItems =
            from item in itemList
            join inAreaPos in InAreaPoses(areaPos) on item.PivotPos equals inAreaPos
            select item;
        toRevealItems.ForEach(item => item.GridRevealed = true);
        if (areaPos.Y == RevealedAreaSet.Max(p => p.Y))
            CurLayer++;

        foreach (var areaDx in Range(-1, 3))
        {
            foreach (var areaDy in Range(-1, 3))
            {
                var unrevealedAreaPos = areaPos + new Vector2Int(areaDx, areaDy);
                if(RevealedAreaSet.Contains(unrevealedAreaPos))
                    continue;
                if (!InAreaMaxRange(unrevealedAreaPos))
                    continue;
                // 生成未揭露地块.
                InsertAfter(InAreaPoses(unrevealedAreaPos).Select(pos => 
                        new ActSpawnItemAtPos(this)
                        {
                            Id = 50001,
                            Pos = pos,
                            ResultWrap = null
                        }));
            }
        }
        return UniTask.CompletedTask;
    }
}