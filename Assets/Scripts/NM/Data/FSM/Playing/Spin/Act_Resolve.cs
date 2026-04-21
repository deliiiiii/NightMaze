using System;
using System.Collections.Generic;
using GeneralPreview;
using NM.Config;

namespace NM.Data;

public partial class PlaySpin
{
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
    IEnumerable<T> ApplyPosFilterAndSort<T>(IEnumerable<T> source, Func<T, Vector2Int> getPoses,
        ICanSelectPos? iCanSelectPos, GamePlaying.MyItem selfItem, ResultWrap? resultWrap)
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
                            Math.Abs(getPoses(element).X - item.PivotPos.X) <= 1 
                            && Math.Abs(getPoses(element).Y - item.PivotPos.Y) <= 1) // 注意：这里修复了你原来代码里的 item.PivotPos.X - item.PivotPos.X 的问题
                    select element;
                break;
            case PosFilterInManDis inManDis:
                if (inManDis.ItemSelector == null) 
                    break;
                source =
                    from element in source
                    where ResolveItemSelector(selfItem, inManDis.ItemSelector, resultWrap)
                        .All(item => 
                            Math.Abs(getPoses(element).X - item.PivotPos.X) 
                            + Math.Abs(getPoses(element).Y - item.PivotPos.Y) 
                            <= inManDis.Dis) // 同上
                    select element;
                break;
            case PosFilterIsEmpty:
                source =
                    from element in source
                    where !BelongNode.GridPoses.Contains(getPoses(element))
                    select element;
                break;
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
                    orderby getPoses(element).X * itemSortPosLeftDown.DescendingValue, getPoses(element).Y * itemSortPosLeftDown.DescendingValue
                    select element;
                break;
            case PosSortPosUpLeft sort:
                source =
                    from element in source
                    orderby -getPoses(element).Y * sort.DescendingValue, getPoses(element).X * sort.DescendingValue
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
    
}