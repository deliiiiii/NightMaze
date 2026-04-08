using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;
namespace NM.Data;
[ActContainer]
public partial class PlaySpin
{
    [Obsolete("准备执行物体整个词条")]
    async UniTask CheckItemAsync(IItem item, CancellationToken ct)
    {
        // MyDebug.Log($"执行物体 pos:{item.PivotPos}");
        await item.OnSpin(this, ct);
        await UniTask.Delay(1000, cancellationToken: ct);
    }
    public record EvtBeforeCheckSymbol(PlaySpin WhoHasCt, IItem Item) : EvtBase<PlaySpin>(WhoHasCt);

    [Obsolete("准备执行物体单个词条Result")]
    UniTask DoItemDesResultAsync(IItem selfItem, ItemDesResultBase result, CancellationToken ct)
    {
        var conditionRet = ResolveCondition(selfItem, result.Condition);
        if (!conditionRet)
            return UniTask.CompletedTask;
        if (result.Next != null)
        {
            InsertAfter(new ActDoItemDesResult(this)
            {
                SelfItem = selfItem,
                Result = result.Next
            });
        }
        InsertAfter(result switch
        {
            ItemDesResultAddItemDesToSelf addItemDesToSelf => 
                from toEat in ResolveItemSelector(selfItem, addItemDesToSelf.ItemSelector)
                from whoEatInPlay in BelongNode.GetItemByEtt(selfItem.BelongEtt).ToIEnumerable()
                from toEatInPlay in BelongNode.GetItemByEtt(toEat.BelongEtt).ToIEnumerable()
                select new GamePlaying.ActItemEatItemConfig(BelongNode)
                {
                    WhoEat = whoEatInPlay,
                    ToEat = toEatInPlay
                },
            ItemDesResultAddXPropX addXProp =>
                from toItem in ResolveItemSelector(selfItem, addXProp.ItemSelector)
                select new ActEttAddSymbolModifyProp(this)
                {
                    From = selfItem,
                    To = toItem,
                    PropType = addXProp.PropType,
                    Value = ResolveIntSelector(selfItem, addXProp.IntSelector)
                },
            ItemDesResultMulXPropX mulXPropX => 
                from toItem in ResolveItemSelector(selfItem, mulXPropX.ItemSelector)
                select new ActEttMulSymbolModifyProp(this)
                {
                    From = selfItem,
                    To = toItem,
                    PropType = mulXPropX.PropType,
                    Value = ResolveIntSelector(selfItem, mulXPropX.IntSelector)
                },
            ItemDesResultRemoveItem removeItem =>
                from toRemove in ResolveItemSelector(selfItem, removeItem.ItemSelector)
                from toRemoveInPlay in BelongNode.GetItemByEtt(toRemove.BelongEtt).ToIEnumerable()
                select new GamePlaying.ActRemoveItem(BelongNode)
                {
                    ToRemove = toRemoveInPlay
                },
            ItemDesResultSpawnXAtX spawnXAtX =>
                from toSpawn in ResolveItemSelector(selfItem, spawnXAtX.ItemSelector).FirstOptional().ToIEnumerable()
                from toSpawnInPlay in BelongNode.GetItemByEtt(toSpawn.BelongEtt).ToIEnumerable()
                from pos in ResolvePosSelector(selfItem, spawnXAtX.PosSelector).FirstOptional().ToIEnumerable()
                select new GamePlaying.ActSpawnItemAtPos(BelongNode)
                {
                    ToSpawn = toSpawnInPlay,
                    Pos = pos
                },
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemDesResultBase)}类型: {result.GetType()}.")
        });
        return UniTask.CompletedTask;
    }

    bool ResolveCondition(IItem selfItem, ItemDesConditionBase? conditionBase)
    {
        if (conditionBase == null)
            return true;
        var thisRet = conditionBase switch
        {
            ItemDesConditionCollectXItem collectXItem => 
                ResolveItemSelector(selfItem, collectXItem.ItemSelector).Sum(_ => 1) 
                >= ResolveIntSelector(selfItem, collectXItem.MinValueSelector),
            _ => throw new InvalidOperationException
                ($"没有匹配穷尽{nameof(ItemDesConditionBase)}类型: {conditionBase.GetType()}.")
        };
        var nextRet = ResolveCondition(selfItem, conditionBase.Next);
        return thisRet && nextRet;
    }
    IEnumerable<IItem> ResolveItemSelector(IItem selfItem, ItemSelectorBase itemSelector)
    {
        var rawItems = itemSelector switch
        {
            ItemSelectorFromResult selectorFromResult => ResolveItemSelector(selfItem, selectorFromResult.IOutItem.ItemSelector),
            ItemSelectorItem selectorItem => from itemInSpin in Items
                from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
                where (itemInPlay is GamePlaying.Grid grid && selectorItem.GridList.Contains(grid.Config)) 
                      || (itemInPlay is GamePlaying.Symbol symbol && selectorItem.SymbolList.Contains(symbol.Config)) 
                      || (itemInPlay is GamePlaying.Building building && selectorItem.BuildingList.Contains(building.Config)) 
                      || (itemInPlay is GamePlaying.Resource resource && selectorItem.ResourceList.Contains(resource.Config))
                select itemInSpin,
            ItemSelectorItemSet selectorItemSet => from itemInSpin in Items
                from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
                let set = selectorItemSet.Set
                where set.GridList.Contains(itemInPlay.Config) 
                      || set.SymbolList.Contains(itemInPlay.Config) 
                      || set.BuildingList.Contains(itemInPlay.Config)
                      || set.ResourceList.Contains(itemInPlay.Config)
                select itemInSpin,
            ItemSelectorSelf selectorSelf => [selfItem],
            ItemSelectorTag selectorTag => from itemInSpin in Items
                from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
                where (itemInPlay is GamePlaying.Grid grid && selectorTag.GridTag != 0 && selectorTag.GridTag.HasFlag(grid.Config.GridTag)) 
                      || (itemInPlay is GamePlaying.Symbol symbol && selectorTag.SymbolTag != 0 && selectorTag.SymbolTag.HasFlag(symbol.Config.SymbolTag))
                      || (itemInPlay is GamePlaying.Building building && selectorTag.BuildingTag != 0 && selectorTag.BuildingTag.HasFlag(building.Config.BuildingTag))
                      || (itemInPlay is GamePlaying.Resource resource && selectorTag.ResourceTag != 0 && selectorTag.ResourceTag.HasFlag(resource.Config.ResourceTag))
                select itemInSpin,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSelectorBase)}类型: {itemSelector.GetType()}.")
        };
        var filter = itemSelector.ItemFilter;
        while (filter != null)
        {
            var filter1 = filter;
            rawItems =
                from itemInSpin in rawItems
                from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
                from selfItemInPlay in BelongNode.GetItemByEtt(selfItem.BelongEtt).ToIEnumerable()
                where filter1 switch
                {
                    null => true,
                    ItemFilterIn3X3 in3X3 => Math.Abs(itemInPlay.PivotPos.X - selfItemInPlay.PivotPos.X) <= 1 
                                             && Math.Abs(itemInPlay.PivotPos.Y - selfItemInPlay.PivotPos.Y) <= 1,
                    ItemFilterInManDis inManDis => Math.Abs(itemInPlay.PivotPos.X - selfItemInPlay.PivotPos.X) 
                                                   + Math.Abs(itemInPlay.PivotPos.Y - selfItemInPlay.PivotPos.Y) 
                                                   <= inManDis.Dis,
                    ItemFilterNotSelf notSelf => itemInSpin != selfItem,
                    ItemFilterSelf self => itemInSpin == selfItem,
                    _ => throw new InvalidOperationException(
                        $"没有匹配穷尽{nameof(ItemFilterBase)}类型: {filter1.GetType()}.")
                }
                select itemInSpin;
            filter = filter.ItemFilter;
        }
       
        if (itemSelector.Random)
            rawItems = rawItems.ToList().ShuffleTo();
        switch (itemSelector.ItemSort)
        {
            case ItemSortPosLeftDown sort:
                rawItems =
                    from itemInSpin in rawItems
                    from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
                    orderby itemInPlay.PivotPos.X * sort.DescendingValue, itemInPlay.PivotPos.Y * sort.DescendingValue
                    select itemInSpin;
                break;
            case ItemSortPosUpLeft sort:
                rawItems =
                    from itemInSpin in rawItems
                    from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
                    orderby -itemInPlay.PivotPos.Y * sort.DescendingValue, itemInPlay.PivotPos.X * sort.DescendingValue
                    select itemInSpin;
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSortBase)}类型: {itemSelector.ItemSort.GetType()}.");
        }
        return rawItems.Take(ResolveIntSelector(selfItem, itemSelector.TakeMax));
    }
    int ResolveIntSelector(IItem selfItem, IntSelectorBase intSelector) =>
        intSelector switch
        {
            IntSelectorConst selectorConst => selectorConst.Value,
            IntSelectorInfinite selectorInfinite => int.MaxValue,
            IntSelectorSumBy selectorSumBy => ResolveItemSelector(selfItem, selectorSumBy.ItemSelector)
                .Sum(_ => selectorSumBy.Value),
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(IntSelectorBase)}类型: {intSelector.GetType()}.")
        };

    IEnumerable<Vector2Int> ResolvePosSelector(IItem selfItem, PosSelectorBase posSelector)
    {
        var rawList = posSelector switch
        {
            PosSelector3X3 selector3X3 =>
                from itemInPlay in BelongNode.GetItemByEtt(selfItem.BelongEtt).ToIEnumerable()
                from dx in Range(-1, 3)
                from dy in Range(-1, 3)
                select new Vector2Int(itemInPlay.PivotPos.X + dx, itemInPlay.PivotPos.Y + dy),
            PosSelectorConst selectorConst => [selectorConst.Value],
            PosSelectorFromResult selectorFromResult => ResolvePosSelector(selfItem,
                selectorFromResult.IOutPos.PosSelector),
            PosSelectorFromResultItem selectorFromResultItem =>
                from itemInResult in ResolveItemSelector(selfItem, selectorFromResultItem.IOutItem.ItemSelector)
                from itemInResultInPlay in BelongNode.GetItemByEtt(itemInResult.BelongEtt).ToIEnumerable()
                select itemInResultInPlay.PivotPos,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(PosSelectorBase)}类型: {posSelector.GetType()}.")
        };
        
        switch (posSelector.PosFilter)
        {
            case null:
                break;
            default:
                throw new InvalidOperationException
                    ($"没有匹配穷尽{nameof(PosFilterBase)}类型: {posSelector.PosFilter.GetType()}.");
        }
        if(posSelector.Random)
            rawList = rawList.ToList().ShuffleTo();
        switch (posSelector.PosSort)
        {
            case null:
                break;
            default:
                throw new InvalidOperationException
                    ($"没有匹配穷尽{nameof(PosSortBase)}类型: {posSelector.PosSort.GetType()}.");
        }
        return rawList.Take(ResolveIntSelector(selfItem, posSelector.TakeMax));
    }

    [Obsolete("某物让某物属性变化(加算)")]
    UniTask EttAddSymbolModifyPropAsync(IItem from, IItem to, EPropType propType, long value, CancellationToken ct)
    {
        to.ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            Ett = from,
            AddValue = value
        });
        return UniTask.CompletedTask;
    }
    [Obsolete("某物让某物属性变化(乘算)")]
    UniTask EttMulSymbolModifyPropAsync(IItem from, IItem to, EPropType propType, long value, CancellationToken ct)
    {
        to.ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            Ett = from,
            MultiValue = value
        });
        return UniTask.CompletedTask;
    }
}