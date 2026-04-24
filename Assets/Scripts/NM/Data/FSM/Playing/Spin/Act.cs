using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;
[ActContainer]
public partial class PlaySpin : IHasCt
{
    [Obsolete("第1轮, 某物体执行 ALL 词条")][MuteActEvt]
    async UniTask CheckItemAsync(GamePlaying.MyItem item, CancellationToken ct)
    {
        if (!BelongNode.Items.Contains(item))
            return;
        await new EvtBeforeCheckSymbolTween(this, item);
        var config = item.Config;
        
        if (config.IsSymbol)
        {
            config.SymbolPropValueList.ForEach(pair =>
            {
                item[this].ModifyPropList.Add(new ModifyPropInfo
                {
                    From = item,
                    PropType = pair.Key,
                    AddValue = pair.Value,
                });
            });
        }
        InsertAfter([..
            from itemDes in item.AllConfigList
            where itemDes.Result != null && 
                  (itemDes.Trigger is ItemDesTriggerEnterSpin
                    || (item.Config.IsEvent && itemDes.Trigger is ItemDesTriggerEventMiKanSei && !item.IsBuildingOrEventKanSei))
            select new ActDoItemDesResult(this)
            {
                Item = item,
                ResultWrap = new ResultWrap(itemDes.Result, null),
            },..
            from itemDes in item.AllConfigList
            where itemDes.Result != null && item.Config.IsBuilding && item.IsBuildingOrEventKanSei
                && itemDes.Trigger is ItemDesTriggerBuildingRun && BelongNode.SatisfyBuildingRun(item)
            from act in (List<IUniAction>)[..
                from pair in item.Config.RunPropValueList
                select new ActDoBuildingRun(this)
                {
                    Item = item,
                    PropType = pair.Key,
                    Value = pair.Value,
                },
                new ActDoItemDesResult(this)
                {
                    Item = item,
                    ResultWrap = new ResultWrap(itemDes.Result, null),
                }
            ]
            select act,
        ]);
    }
    [EvtName("第1轮, 某物体执行 ALL 词条前")]
    public record EvtBeforeCheckSymbolTween(PlaySpin WhoHasCt, GamePlaying.MyItem Item) : EvtBase<PlaySpin>(WhoHasCt);
    [Obsolete("第1轮, 将执行物体单行词条")]
    UniTask DoItemDesResultAsync(GamePlaying.MyItem item, ResultWrap resultWrap, CancellationToken ct)
    {
        if (!BelongNode.Items.Contains(item))
            return UniTask.CompletedTask;
        var result = resultWrap.Result;
        var conditionRet = ResolveCondition(item, result?.Condition, resultWrap);
        if (!resultWrap.DoIfPreFail && (!resultWrap.PreResult?.Success ?? false))
            return UniTask.CompletedTask;
        if (!conditionRet)
        {
            if (result?.ConditionFail != null)
            {
                InsertAfter(new ActDoItemDesResult(this)
                {
                    Item = item,
                    ResultWrap = new ResultWrap(result.ConditionFail, null)
                });
            }
            return UniTask.CompletedTask;
        }
        if (result?.Next != null)
        {
            InsertAfter(new ActDoItemDesResult(this)
            {
                Item = item,
                ResultWrap = new ResultWrap(result.Next, resultWrap)
            });
        }
        if (result?.NextFail != null)
        {
            InsertAfter(new ActDoItemDesResult(this)
            {
                Item = item,
                ResultWrap = new ResultWrap(result.NextFail, resultWrap, DoIfPreFail: true)
            });
        }
        if(resultWrap.DoIfPreFail && (resultWrap.PreResult?.Success ?? true))
            return UniTask.CompletedTask;
        InsertAfter(result switch
        {
            ItemDesResultAddItemDesToSelf addItemDesToSelf => 
                from toEat in ResolveItemSelector(item, addItemDesToSelf.ItemSelector, resultWrap)
                select new GamePlaying.ActItemEatItemConfig(BelongNode)
                {
                    WhoEat = item,
                    ToEat = toEat,
                    ResultWrap = resultWrap
                },
            ItemDesResultAddXPropX addXProp =>
                from toItem in ResolveItemSelector(item, addXProp.ItemSelector, resultWrap)
                select new ActEttAddSymbolModifyProp(this)
                {
                    From = item,
                    To = toItem,
                    PropType = addXProp.PropType,
                    Value = ResolveIntSelector(item, addXProp.IntSelector, resultWrap),
                    ResultWrap = resultWrap
                },
            ItemDesResultMulXPropX mulXPropX => 
                from toItem in ResolveItemSelector(item, mulXPropX.ItemSelector, resultWrap)
                select new ActEttMulSymbolModifyProp(this)
                {
                    From = item,
                    To = toItem,
                    PropType = mulXPropX.PropType,
                    Value = ResolveDoubleSelector(item, mulXPropX.DoubleSelector, resultWrap),
                    ResultWrap = resultWrap
                },
            ItemDesResultRemoveItem removeItem =>
                from toRemove in ResolveItemSelector(item, removeItem.ItemSelector, resultWrap)
                select new GamePlaying.ActRemoveItem(BelongNode)
                {
                    ToRemove = toRemove,
                    ResultWrap = resultWrap
                },
            ItemDesResultSpawnXAtX spawnXAtX =>
                from pos in ResolvePosSelector(item, spawnXAtX.PosSelector, resultWrap)
                // , p =>
                // {
                //     toSpawnInPlay.PivotPos = p;
                //     return BelongNode.TrySetItem(toSpawnInPlay);
                // })
                from toSpawn in ResolveItemSelector(item, spawnXAtX.ItemSelector, resultWrap).FirstOptional().ToIEnumerable()
                select new GamePlaying.ActSpawnItemAtPos(BelongNode)
                {
                    Pos = pos,
                    Id = toSpawn.Config.ID,
                    ResultWrap = resultWrap
                },
            ItemDesResultToTech => [],
            null => [],
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemDesResultBase)}类型: {result.GetType()}.")
        });
        return UniTask.CompletedTask;
    }
    [Obsolete("第1轮, 扣除建筑运营消耗")]
    UniTask DoBuildingRunAsync(GamePlaying.MyItem item, EPropType propType, long value, CancellationToken ct)
    {
        item[this].DistributePropList.Add(new DistributePropInfo
        {
            PropType = propType,
            Value = -value,
            ToItem = item
        });
        item[this].HasGivenBuildingRunCost = true;
        new GamePlaying.ActChangeProp(BelongNode)
        {
            PropType = propType,
            Delta = -value
        }.Forget();
        return UniTask.CompletedTask;
    }
    [Obsolete("第1轮, 结算无来源属性")]
    UniTask DoNoSourcePropAsync(EPropType propType, long value, CancellationToken ct)
    {
        noSourceDistributePropList.Add(new DistributePropInfo
        {
            PropType = propType,
            Value = value,
        });
        new GamePlaying.ActChangeProp(BelongNode)
        {
            PropType = propType,
            Delta = value
        }.Forget();
        return UniTask.CompletedTask;
    }
    [Obsolete("第2轮, 某物体分配属性去向")]
    UniTask DistributePropForItemAsync(GamePlaying.MyItem item, CancellationToken ct)
    {
        var inSpin = item[this];
        var tarBuildingOrEvtList = (
            from pos in item.CoveredPosList
            orderby pos.Y descending, pos.X ascending
            from tarItem in BelongNode.Items
            where tarItem.Config.IsBuildingOrEvent 
                  && !tarItem.IsBuildingOrEventKanSei
                  && tarItem.CoveredPosList.Contains(pos)
            select tarItem).ToList();
        var toTechList = (
            from pos in item.CoveredPosList
            orderby pos.Y descending, pos.X ascending
            from tarItem in BelongNode.Items
            where tarItem.Config.IsBuilding
                  && tarItem.CoveredPosList.Contains(pos)
                  && tarItem[this].HasGivenBuildingRunCost
                  // 这意味着科技树词条不能是嵌套的...
                  && tarItem.AllConfigList.Any(config => config.Result is ItemDesResultToTech)
            select tarItem).ToList();
        EPropType.GetValues().ForEach(propType =>
        {
            var remain = inSpin.GetAllProp(propType);
            foreach (var tarBuildOrEvt in tarBuildingOrEvtList)
            {
                if (remain <= 0)
                    break;
                var inProgress = tarBuildOrEvt.BuildingOrEventProgress.GetValueOrDefault(propType, 0);
                var tarProgress = tarBuildOrEvt.Config.BuildPropValueList.GetValueOrDefault(propType, 0);
                var require = Math.Max(tarProgress - inProgress, 0);
                var use = Math.Min(remain, require);
                if (use > 0)
                {
                    // 副作用, 没有通过Act实现
                    tarBuildOrEvt.BuildingOrEventProgress[propType] += use;
                    inSpin.DistributePropList.Add(new DistributePropInfo
                    {
                        PropType = propType,
                        Value = use,
                        ToItem = tarBuildOrEvt,
                    });
                }
                remain -= use;
            }
            var curTechInfo = BelongNode.GetCurTechInfo();
            if(toTechList.Any() && remain > 0 && curTechInfo.HasValue)
            {
                var require = Math.Max(curTechInfo.Value.TarDic.GetValueOrDefault(propType) 
                                       - curTechInfo.Value.CurDic.GetValueOrDefault(propType), 0);
                var use = Math.Min(remain, require);
                if (use > 0)
                {
                    // 副作用, 没有通过Act实现
                    curTechInfo.Value.Node.CarValueDic[propType] += use;
                    inSpin.DistributePropList.Add(new DistributePropInfo
                    {
                        PropType = propType,
                        Value = use,
                        ToTech = true
                    });
                }
                remain -= use;
            }
            if (remain == 0)
                return;
            new GamePlaying.ActChangeProp(BelongNode)
            {
                PropType = propType,
                Delta = remain,
            }.Forget();
            inSpin.DistributePropList.Add(new DistributePropInfo
            {
                PropType = propType,
                Value = remain,
            });
        });
        
        return UniTask.CompletedTask;    
    }
}