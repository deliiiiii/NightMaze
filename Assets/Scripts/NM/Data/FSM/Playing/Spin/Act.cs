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
            Value = value
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
        var tarBuildingOrEvtList =
            (from pos in item.CoveredPosList
                orderby pos.Y descending, pos.X ascending
                from tarItem in BelongNode.Items
                where tarItem.Config.IsBuildingOrEvent && tarItem.CoveredPosList.Contains(pos)
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
            if (remain != 0)
            {
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
            }
        });
        
        return UniTask.CompletedTask;    
    }
    
    bool ResolveCondition(GamePlaying.MyItem selfItem, ItemDesConditionBase? conditionBase, ResultWrap? resultWrap)
    {
        if (conditionBase == null)
            return true;
        var thisRet = conditionBase switch
        {
            ItemDesConditionAlwaysFalse => false,
            ItemDesConditionCollectXItem collectXItem => 
                ResolveItemSelector(selfItem, collectXItem.ItemSelector, resultWrap).Sum(_ => 1) 
                >= ResolveIntSelector(selfItem, collectXItem.MinValueSelector, resultWrap),
            _ => throw new InvalidOperationException
                ($"没有匹配穷尽{nameof(ItemDesConditionBase)}类型: {conditionBase.GetType()}.")
        };
        var nextRet = ResolveCondition(selfItem, conditionBase.Next, resultWrap);
        return thisRet && nextRet;
    }
    static IEnumerable<GamePlaying.MyItem> ResolveItemSelectorFromResult(ItemSelectorFromResultFilterBase? fromResultFilter, ResultWrap? resultWrap)
    {
        if (resultWrap == null)
            return [];
        return 
            from itemWrap in resultWrap.ItemWraps
            where fromResultFilter switch
            {
                ItemSelectorFromResultFilterAddPropX => itemWrap.CtxList.OfType<ResultItemWrap.CtxAddPropX>().Any(),
                ItemSelectorFromResultFilterFailMoved => itemWrap.CtxList.OfType<ResultItemWrap.CtxFailMoved>().Any(),
                ItemSelectorFromResultFilterMulPropX => itemWrap.CtxList.OfType<ResultItemWrap.CtxMulPropX>().Any(),
                ItemSelectorFromResultFilterRemoved => itemWrap.CtxList.OfType<ResultItemWrap.CtxRemoved>().Any(),
                ItemSelectorFromResultFilterSpawned => itemWrap.CtxList.OfType<ResultItemWrap.CtxSpawned>().Any(),
                ItemSelectorFromResultFilterSuccessMoved => itemWrap.CtxList.OfType<ResultItemWrap.CtxSuccessMoved>().Any(),
                null => true,
                _ => throw new InvalidOperationException(
                    $"没有匹配穷尽{nameof(ItemSelectorFromResultFilterBase)}类型: {fromResultFilter.GetType()}.")
            }
            select itemWrap.Item;
    }
    static IEnumerable<Vector2Int> ResolvePosSelectorFromResult(PosSelectorFromResultFilterBase? fromResultFilter,
        ResultWrap? resultWrap)
    {
        if (resultWrap == null)
            return [];
        return
            from posWrap in resultWrap.PosWraps
            where fromResultFilter switch
            {
                PosSelectorFromResultFilterFalse => posWrap.CtxList.OfType<ResultPosWrap.CtxFalse>().Any(),
                null => true,
                _ => throw new InvalidOperationException(
                    $"没有匹配穷尽{nameof(PosSelectorFromResultFilterBase)}类型: {fromResultFilter.GetType()}.")
            }
            select posWrap.Pos;
    }
    IEnumerable<GamePlaying.MyItem> ResolveItemSelector(GamePlaying.MyItem selfItem, ICanSelectItem? iCanSelectItem, ResultWrap? resultWrap)
    {
        if (iCanSelectItem == null)
            return [];
        var rawItems = iCanSelectItem switch
        {
            ItemSelectorAtPresentAll => BelongNode.Items,
            ItemSelectorAtPresentSelf => [selfItem],
            ItemSelectorFromResult fromResult => [
                ..fromResult.LastStepCount == 1 ? ResolveItemSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult) : [],
                ..fromResult.LastStepCount == 2 ? ResolveItemSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult) : [],
                ..fromResult.LastStepCount == 3 ? ResolveItemSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult?.PreResult) : [],
            ],
            ItemSelectorFromConfigCustom fromConfigCustom => 
                from itemConfig in fromConfigCustom.ItemList
                where itemConfig != null
                select new GamePlaying.MyItem(itemConfig!.ID, Vector2Int.Zero),
            ItemSelectorItemFromConfigSet fromConfigSet => 
                from itemInPlay in BelongNode.Items
                where fromConfigSet.Set != null && fromConfigSet.Set.ItemList.Contains(itemInPlay.Config)
                select itemInPlay,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSelectorBase)}类型: {iCanSelectItem.GetType()}.")
        };
        var filter = iCanSelectItem.ItemFilter;
        while (filter != null)
        {
            var filter1 = filter;
            rawItems =
                from item in rawItems
                where filter1 switch
                {
                    ItemFilterIsItemType isItemType => isItemType.ItemType != 0 && isItemType.ItemType.HasFlag(item.ItemType),
                    ItemFilterNotSelf => item != selfItem,
                    ItemFilterSelf => item == selfItem,
                    ItemFilterTag filterTag =>
                        filterTag.ItemTagList.Intersect(item.Config.TagList).Any()
                         || filterTag.GridTagList.Intersect(item.Config.GridTagList).Any()
                         || filterTag.SymbolTagList.Intersect(item.Config.SymbolTagList).Any()
                         || filterTag.BuildingTagList.Intersect(item.Config.BuildingTagList).Any()
                         || filterTag.ResourceTagList.Intersect(item.Config.ResourceTagList).Any()
                         || filterTag.EventTagList.Intersect(item.Config.EventTagList).Any(),
                    null => true,
                    ItemFilterIsItemCustom itemCustom => itemCustom.ItemList.Select(i => i?.ID).Contains(item.Config.ID),
                    ItemFilterIsItemSet itemSet => itemSet.Set?.ItemList.Select(i => i?.ID).Contains(item.Config.ID) ?? false,
                    _ => throw new InvalidOperationException(
                        $"没有匹配穷尽{nameof(ItemFilterBase)}类型: {filter1.GetType()}.")
                }
                select item;
            filter = filter.ItemFilter;
        }
       
        if (iCanSelectItem.Random)
            rawItems = rawItems.ToList().ShuffleTo();
        switch (iCanSelectItem.ItemSort)
        {
            case null:
                break;
            default:
                throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSortBase)}类型: {iCanSelectItem.ItemSort.GetType()}.");
        }
        rawItems = rawItems.Take(ResolveIntSelector(selfItem, iCanSelectItem.TakeMax, resultWrap));
        return ApplyPosFilterAndSort(rawItems, p => p.PivotPos, iCanSelectItem, selfItem, resultWrap);
    }
    int ResolveIntSelector(GamePlaying.MyItem selfItem, IntSelectorBase? intSelector, ResultWrap? resultWrap) =>
        intSelector switch
        {
            IntSelectorConst selectorConst => selectorConst.Value,
            IntSelectorInfinite selectorInfinite => int.MaxValue,
            IntSelectorSumBy selectorSumBy => ResolveItemSelector(selfItem, selectorSumBy.ItemSelector, resultWrap)
                .Sum(_ => selectorSumBy.Value),
            null => 0,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(IntSelectorBase)}类型: {intSelector.GetType()}.")
        };
    double ResolveDoubleSelector(GamePlaying.MyItem selfItem, DoubleSelectorBase? doubleSelector, ResultWrap? resultWrap) =>
        doubleSelector switch
        {
            DoubleSelectorConst selectorConst => selectorConst.Value,
            null => 1f,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(DoubleSelectorBase)}类型: {doubleSelector.GetType()}.")
        };
    IEnumerable<Vector2Int> ResolvePosSelector(GamePlaying.MyItem selfItem, ICanSelectPos? iCanSelectPos, ResultWrap? resultWrap, Func<Vector2Int, bool>? extraFilter = null)
    {
        var rawList = iCanSelectPos switch
        {
            PosSelector3X3 =>
                from dx in Range(-1, 3)
                from dy in Range(-1, 3)
                select new Vector2Int(selfItem.PivotPos.X + dx, selfItem.PivotPos.Y + dy),
            PosSelectorConst @const => [@const.Value],
            PosSelectorFromResult fromResult => [
                .. fromResult.LastStepCount == 1 ? ResolvePosSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult) : [],
                .. fromResult.LastStepCount == 2 ? ResolvePosSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult) : [],
                .. fromResult.LastStepCount == 3 ? ResolvePosSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult?.PreResult) : [],
            ],
            PosSelectorFromResultItem fromResultItem => 
                from item in
                (IEnumerable<GamePlaying.MyItem>)[
                    .. fromResultItem.LastStepCount == 1 ? ResolveItemSelectorFromResult(fromResultItem.FromResultFilter, resultWrap?.PreResult) : [],
                    .. fromResultItem.LastStepCount == 2 ? ResolveItemSelectorFromResult(fromResultItem.FromResultFilter, resultWrap?.PreResult?.PreResult) : [],
                    .. fromResultItem.LastStepCount == 3 ? ResolveItemSelectorFromResult(fromResultItem.FromResultFilter, resultWrap?.PreResult?.PreResult?.PreResult) : [],
                ]
                select item.PivotPos,
            null => [],
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(PosSelectorBase)}类型: {iCanSelectPos.GetType()}.")
        };
        return ApplyPosFilterAndSort(rawList, p => p, iCanSelectPos, selfItem, resultWrap);
    }
    IEnumerable<T> ApplyPosFilterAndSort<T>(IEnumerable<T> source, Func<T, Vector2Int> getPos, ICanSelectPos? iCanSelectPos, GamePlaying.MyItem selfItem, ResultWrap? resultWrap)
    {
        if(iCanSelectPos == null)
            return source;
        switch (iCanSelectPos.PosFilter)
        {
            case PosFilterIn3X3 in3X3:
                if (in3X3.ItemSelector == null) 
                    break;
                source =
                    from element in source
                    where ResolveItemSelector(selfItem, in3X3.ItemSelector, resultWrap)
                        .All(item => 
                            Math.Abs(getPos(element).X - item.PivotPos.X) <= 1 
                            && Math.Abs(getPos(element).Y - item.PivotPos.Y) <= 1) // 注意：这里修复了你原来代码里的 item.PivotPos.X - item.PivotPos.X 的问题
                    select element;
                break;
            case PosFilterInManDis inManDis:
                if (inManDis.ItemSelector == null) 
                    break;
                source =
                    from element in source
                    where ResolveItemSelector(selfItem, inManDis.ItemSelector, resultWrap)
                        .All(item => 
                            Math.Abs(getPos(element).X - item.PivotPos.X) 
                            + Math.Abs(getPos(element).Y - item.PivotPos.Y) 
                            <= inManDis.Dis) // 同上
                    select element;
                break;
            case PosFilterIsEmpty:
                source =
                    from element in source
                    where !BelongNode.GridPoses.Contains(getPos(element))
                    select element;
                break;
            // case PosFilterIsEmptyGrid:
            //     source =
            //         from 
            //     break;
            case null:
                break;
            default:
                throw new InvalidOperationException(
                    $"没有匹配穷尽{nameof(PosFilterBase)}类型: {iCanSelectPos.PosFilter.GetType()}.");
        }

        if(iCanSelectPos.Random)
            source = source.ToList().ShuffleTo();
        
        switch (iCanSelectPos.PosSort)
        {
            case PosSortPosLeftDown itemSortPosLeftDown:
                source =
                    from element in source
                    orderby getPos(element).X * itemSortPosLeftDown.DescendingValue, getPos(element).Y * itemSortPosLeftDown.DescendingValue
                    select element;
                break;
            case PosSortPosUpLeft sort:
                source =
                    from element in source
                    orderby -getPos(element).Y * sort.DescendingValue, getPos(element).X * sort.DescendingValue
                    select element;
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException(
                    $"没有匹配穷尽{nameof(PosSortBase)}类型: {iCanSelectPos.PosSort.GetType()}.");
        }
        return source.Take(ResolveIntSelector(selfItem, iCanSelectPos.TakeMax, resultWrap));
    }
    
    [Obsolete("某物让某物属性变化(加算)")]
    UniTask EttAddSymbolModifyPropAsync(GamePlaying.MyItem from, GamePlaying.MyItem to, EPropType propType, long value, ResultWrap? resultWrap, CancellationToken ct)
    {
        to[this].ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            From = from,
            AddValue = value,
        });
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to)
        {
            CtxList = [new ResultItemWrap.CtxAddPropX{PropType = propType,Value = value}]
        });
        return UniTask.CompletedTask;
    }
    [Obsolete("某物让某物属性变化(乘算)")]
    UniTask EttMulSymbolModifyPropAsync(GamePlaying.MyItem from, GamePlaying.MyItem to, EPropType propType, double value, ResultWrap? resultWrap, CancellationToken ct)
    {
        to[this].ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            From = from,
            MultiValue = value
        });
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to)
        {
            CtxList = [new ResultItemWrap.CtxMulPropX{PropType = propType,Value = value}]
        });
        return UniTask.CompletedTask;
    }

    [Obsolete("等待点击下一回合按钮")]
    async UniTask WaitForClickNextTurnAsync(CancellationToken ct)
    {
        await Bus.WaitForAsync<GamePlaying.EvtClickNextTurn>("点击下一回合按钮", ct);
    }
}