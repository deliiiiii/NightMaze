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
            ItemDesResultAddItemDesToSelf addItemDesToSelf => throw new NotImplementedException(),
            ItemDesResultAddXPropX addXProp =>
                from toItem in ResolveItemSelector(selfItem, addXProp.ItemSelector)
                select new ActEttAddSymbolModifyProp(this)
                {
                    From = selfItem,
                    To = toItem,
                    PropType = addXProp.PropType,
                    Value = ResolveIntSelector(selfItem, addXProp.IntSelector)
                },
            ItemDesResultRemoveItem removeItem =>
                from toRemove in ResolveItemSelector(selfItem, removeItem.ItemSelector)
                from toRemoveInPlay in BelongNode.GetItemByEtt(toRemove.BelongEtt).ToIEnumerable()
                select new GamePlaying.ActRemoveItem(BelongNode)
                {
                    Item = toRemoveInPlay
                },
            ItemDesResultSpawnXAtX spawnXAtX =>
                from toSpawn in ResolveItemSelector(selfItem, spawnXAtX.ItemSelector).FirstOptional().ToIEnumerable()
                from toSpawnInPlay in BelongNode.GetItemByEtt(toSpawn.BelongEtt).ToIEnumerable()
                from pos in ResolvePosSelector(selfItem, spawnXAtX.PosSelector).FirstOptional().ToIEnumerable()
                select new GamePlaying.ActSpawnItemAtPos(BelongNode)
                {
                    Item = toSpawnInPlay,
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
                where (itemInPlay is GamePlaying.Grid grid && selectorTag.GridTag.HasFlag(grid.Config.Tag)) 
                      || (itemInPlay is GamePlaying.Symbol symbol && selectorTag.SymbolTag.HasFlag(symbol.Config.Tag))
                      || (itemInPlay is GamePlaying.Building building && selectorTag.BuildingTag.HasFlag(building.Config.Tag))
                      || (itemInPlay is GamePlaying.Resource resource && selectorTag.ResourceTag.HasFlag(resource.Config.Tag))
                select itemInSpin,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSelectorBase)}类型: {itemSelector.GetType()}.")
        };
        rawItems =
            from itemInSpin in rawItems
            from itemInPlay in BelongNode.GetItemByEtt(itemInSpin.BelongEtt).ToIEnumerable()
            from selfItemInPlay in BelongNode.GetItemByEtt(selfItem.BelongEtt).ToIEnumerable()
            where itemSelector.ItemFilter switch
            {
                null => true,
                ItemFilterIn3X3 in3X3 => Math.Abs(itemInPlay.PivotPos.X - selfItemInPlay.PivotPos.X) <= 1 
                                         && Math.Abs(itemInPlay.PivotPos.Y - selfItemInPlay.PivotPos.Y) <= 1,
                ItemFilterInManDis inManDis => Math.Abs(itemInPlay.PivotPos.X - selfItemInPlay.PivotPos.X) 
                                               + Math.Abs(itemInPlay.PivotPos.Y - selfItemInPlay.PivotPos.Y) 
                                               <= inManDis.Dis,
                ItemFilterNotSelf notSelf => itemInPlay != selfItemInPlay,
                ItemFilterSelf self => itemInPlay == selfItemInPlay,
                _ => throw new InvalidOperationException(
                    $"没有匹配穷尽{nameof(ItemFilterBase)}类型: {itemSelector.ItemFilter.GetType()}.")
            }
            select itemInSpin;
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

    [Obsolete("某物让某物属性变化")]
    UniTask EttAddSymbolModifyPropAsync(IItem from, IItem to, EPropType propType, int value, CancellationToken ct)
    {
        switch (propType)
        {
            case EPropType.Prop1:
                to.ModifyProp1.Add(new ModifyPropInfo
                {
                    Ett = from,
                    Value = value
                });
                break;
            case EPropType.Prop2:
                to.ModifyProp2.Add(new ModifyPropInfo
                {
                    Ett = from,
                    Value = value
                });
                break;
            case EPropType.Prop3:
                to.ModifyProp3.Add(new ModifyPropInfo
                {
                    Ett = from,
                    Value = value
                });
                break;
            default:
                throw new InvalidOperationException($"没有匹配穷尽{nameof(EPropType)}类型: {propType}.");
        }
        return UniTask.CompletedTask;
    }
    
}