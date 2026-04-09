using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;
namespace NM.Data;

public record ResultWrap(ItemDesResultBase Result, ResultWrap? PreResult)
{
    public readonly ItemDesResultBase Result = Result;
    public bool Success;
    [JsonIgnore] bool hasNext;
    public readonly ResultWrap? PreResult = PreResult;
    public List<ResultItemWrap> ItemWraps = [];
    public List<ResultPosWrap> PosWraps = [];

    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append($"Result = {Result.GetType()}, ");
        if (PreResult != null)
        {
            PreResult.hasNext = true;
            var preSb = new StringBuilder();
            PreResult.PrintMembers(preSb);
            sb.Append($"PreResult = {{ {preSb} }}, ");
        }
        if (hasNext)
        {
            sb.Append($"Success = {Success}, ");
            sb.Append($"ItemWraps = [{string.Join(", ", ItemWraps.Select(w => w))}], ");
            sb.Append($"PosWraps = [{string.Join(", ", PosWraps.Select(w => w))}]");
        }
        return true;
    }
}

public record ResultItemWrap(GamePlaying.IItem Item)
{
    public GamePlaying.IItem Item = Item;
    public List<CtxBase> CtxList = [];
    public abstract record CtxBase;
    
    public record CtxSpawned : CtxBase;
    public record CtxRemoved : CtxBase;
    public record CtxSuccessMoved : CtxBase
    {
        public Vector2Int OldPos;
    }
    public record CtxFailMoved : CtxBase;
    public record CtxAddPropX : CtxBase
    {
        public EPropType PropType;
        public long Value;
    }
    public record CtxMulPropX : CtxBase
    {
        public EPropType PropType;
        public long Value;
    }
    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append($"Item = {Item}, ");
        sb.Append($"CtxList = [{string.Join(", ", CtxList.Select(c => c.GetType()))}], ");
        return true;
    }
}
public record ResultPosWrap(Vector2Int Pos)
{
    public Vector2Int Pos = Pos;
    public List<CtxBase> CtxList = [];
    public abstract record CtxBase;
    public record CtxFalse : CtxBase;
    protected virtual bool PrintMembers(StringBuilder sb)
    {
        sb.Append($"Pos = {Pos}, ");
        sb.Append($"CtxList = [{string.Join(", ", CtxList.Select(c => c.GetType()))}], ");
        return true;
    }
}

[ActContainer]
public partial class PlaySpin
{
    [Obsolete("将执行物体 ALL 词条")]
    async UniTask CheckItemAsync(IItem item, CancellationToken ct)
    {
        if (!BelongNode.Items.Contains(item.InPlay))
            return;
        await item.OnSpin(ct);
        await UniTask.Delay(200, cancellationToken: ct);
    }
    [EvtName("物体执行 ALL 词条前.")]
    public record EvtBeforeCheckSymbol(PlaySpin WhoHasCt, IItem Item) : EvtBase<PlaySpin>(WhoHasCt);

   
    [Obsolete("将执行物体单行词条")]
    UniTask DoItemDesResultAsync(IItem selfItem, ResultWrap resultWrap, CancellationToken ct)
    {
        if (!BelongNode.Items.Contains(selfItem.InPlay))
            return UniTask.CompletedTask;
        var result = resultWrap.Result;
        var conditionRet = ResolveCondition(selfItem, result.Condition, resultWrap);
        if (!conditionRet)
        {
            resultWrap.Success = false;
            return UniTask.CompletedTask;
        }
        if (result.Next != null)
        {
            InsertAfter(new ActDoItemDesResult(this)
            {
                SelfItem = selfItem,
                ResultWrap = new ResultWrap(result.Next, resultWrap)
            });
        }
        if (!(resultWrap.PreResult?.Success ?? true))
            return UniTask.CompletedTask;
        InsertAfter(result switch
        {
            ItemDesResultAddItemDesToSelf addItemDesToSelf => 
                from toEat in ResolveItemSelector(selfItem, addItemDesToSelf.ItemSelector, resultWrap)
                let whoEatInPlay = selfItem.InPlay
                let toEatInPlay = toEat.InPlay
                select new GamePlaying.ActItemEatItemConfig(BelongNode)
                {
                    WhoEat = whoEatInPlay,
                    ToEat = toEatInPlay,
                    ResultWrap = resultWrap
                },
            ItemDesResultAddXPropX addXProp =>
                from toItem in ResolveItemSelector(selfItem, addXProp.ItemSelector, resultWrap)
                select new ActEttAddSymbolModifyProp(this)
                {
                    From = selfItem,
                    To = toItem,
                    PropType = addXProp.PropType,
                    Value = ResolveIntSelector(selfItem, addXProp.IntSelector, resultWrap),
                    ResultWrap = resultWrap
                },
            ItemDesResultMulXPropX mulXPropX => 
                from toItem in ResolveItemSelector(selfItem, mulXPropX.ItemSelector, resultWrap)
                select new ActEttMulSymbolModifyProp(this)
                {
                    From = selfItem,
                    To = toItem,
                    PropType = mulXPropX.PropType,
                    Value = ResolveIntSelector(selfItem, mulXPropX.IntSelector, resultWrap),
                    ResultWrap = resultWrap
                },
            ItemDesResultRemoveItem removeItem =>
                from toRemove in ResolveItemSelector(selfItem, removeItem.ItemSelector, resultWrap)
                select new GamePlaying.ActRemoveItem(BelongNode)
                {
                    ToRemove = toRemove.InPlay,
                    ResultWrap = resultWrap
                },
            ItemDesResultSpawnXAtX spawnXAtX =>
                from pos in ResolvePosSelector(selfItem, spawnXAtX.PosSelector, resultWrap)
                    // , p =>
                // {
                    // toSpawnInPlay.PivotPos = p;
                    // return BelongNode.TrySetItem(toSpawnInPlay);
                // })
                from toSpawn in ResolveItemSelector(selfItem, spawnXAtX.ItemSelector, resultWrap).FirstOptional().ToIEnumerable()
                let toSpawnInPlay = toSpawn.InPlay
                select new GamePlaying.ActSpawnItemAtPos(BelongNode)
                {
                    Pos = pos,
                    Type = toSpawnInPlay.ItemType,
                    Id = toSpawnInPlay.Config.ID,
                    ResultWrap = resultWrap
                },
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemDesResultBase)}类型: {result.GetType()}.")
        });
        return UniTask.CompletedTask;
    }

    bool ResolveCondition(IItem selfItem, ItemDesConditionBase? conditionBase, ResultWrap? resultWrap)
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

    IEnumerable<IItem> ResolveItemSelectorFromResult(ItemSelectorFromResultFilterBase? fromResultFilter, ResultWrap? resultWrap)
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
            select itemWrap.Item.InSpin(this);
    }

    IEnumerable<Vector2Int> ResolvePosSelectorFromResult(PosSelectorFromResultFilterBase? fromResultFilter,
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
    
    IEnumerable<IItem> ResolveItemSelector(IItem selfItem, ICanSelectItem? iCanSelectItem, ResultWrap? resultWrap)
    {
        if (iCanSelectItem == null)
            return [];
        var rawItems = iCanSelectItem switch
        {
            ItemSelectorAtPresentAll => Items,
            ItemSelectorAtPresentSelf => [selfItem],
            ItemSelectorFromResult fromResult => [
                ..fromResult.LastStepCount == 1 ? ResolveItemSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult) : [],
                ..fromResult.LastStepCount == 2 ? ResolveItemSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult) : [],
                ..fromResult.LastStepCount == 3 ? ResolveItemSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult?.PreResult) : [],
            ],
            ItemSelectorFromConfigCustom fromConfigCustom => 
                from iItemConfig in (List<IItemConfig>)[
                ..fromConfigCustom.GridList,
                ..fromConfigCustom.SymbolList, 
                ..fromConfigCustom.BuildingList,
                ..fromConfigCustom.ResourceList]
                where iItemConfig != null
                select iItemConfig switch
                {
                    GridConfig gridConfig => new GamePlaying.Grid(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    BuildingConfig buildingConfig => new GamePlaying.Building(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    ResourceConfig resourceConfig => new GamePlaying.Resource(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    SymbolConfig symbolConfig => new GamePlaying.Symbol(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(IItemConfig)}类型: {iItemConfig.GetType()}.")
                },
            ItemSelectorItemFromConfigSet fromConfigSet => 
                from itemInSpin in Items
                let itemInPlay = itemInSpin.InPlay
                where fromConfigSet.Set != null &&
                        (fromConfigSet.Set.GridList.Contains(itemInPlay.Config)
                       || fromConfigSet.Set.SymbolList.Contains(itemInPlay.Config)
                       || fromConfigSet.Set.BuildingList.Contains(itemInPlay.Config)
                       || fromConfigSet.Set.ResourceList.Contains(itemInPlay.Config))
                select itemInSpin,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSelectorBase)}类型: {iCanSelectItem.GetType()}.")
        };
        var filter = iCanSelectItem.ItemFilter;
        while (filter != null)
        {
            var filter1 = filter;
            rawItems =
                from itemInSpin in rawItems
                let itemInPlay = itemInSpin.InPlay
                let selfItemInPlay = selfItem.InPlay
                where filter1 switch
                {
                    ItemFilterIsItemType isItemType => isItemType.ItemType != 0 && isItemType.ItemType.HasFlag(itemInPlay.ItemType),
                    ItemFilterNotSelf => itemInSpin != selfItem,
                    ItemFilterSelf => itemInSpin == selfItem,
                    ItemFilterTag filterTag => 
                         (itemInPlay is GamePlaying.Grid grid && filterTag.GridTag != 0 && filterTag.GridTag.HasFlag(grid.Config.GridTag)) 
                              || (itemInPlay is GamePlaying.Symbol symbol && filterTag.SymbolTag != 0 && filterTag.SymbolTag.HasFlag(symbol.Config.SymbolTag))
                              || (itemInPlay is GamePlaying.Building building && filterTag.BuildingTag != 0 && filterTag.BuildingTag.HasFlag(building.Config.BuildingTag))
                              || (itemInPlay is GamePlaying.Resource resource && filterTag.ResourceTag != 0 && filterTag.ResourceTag.HasFlag(resource.Config.ResourceTag)),
                    null => true,
                    _ => throw new InvalidOperationException(
                        $"没有匹配穷尽{nameof(ItemFilterBase)}类型: {filter1.GetType()}.")
                }
                select itemInSpin;
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
        return ApplyPosFilterAndSort(rawItems, p => p.InPlay.PivotPos, iCanSelectItem , selfItem, resultWrap);
    }
    int ResolveIntSelector(IItem selfItem, IntSelectorBase? intSelector, ResultWrap? resultWrap) =>
        intSelector switch
        {
            IntSelectorConst selectorConst => selectorConst.Value,
            IntSelectorInfinite selectorInfinite => int.MaxValue,
            IntSelectorSumBy selectorSumBy => ResolveItemSelector(selfItem, selectorSumBy.ItemSelector, resultWrap)
                .Sum(_ => selectorSumBy.Value),
            null => 0,
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(IntSelectorBase)}类型: {intSelector.GetType()}.")
        };

    IEnumerable<Vector2Int> ResolvePosSelector(IItem selfItem, ICanSelectPos? iCanSelectPos, ResultWrap? resultWrap, Func<Vector2Int, bool>? extraFilter = null)
    {
        var rawList = iCanSelectPos switch
        {
            PosSelector3X3 =>
                from dx in Range(-1, 3)
                from dy in Range(-1, 3)
                let itemInPlay = selfItem.InPlay
                select new Vector2Int(itemInPlay.PivotPos.X + dx, itemInPlay.PivotPos.Y + dy),
            PosSelectorConst @const => [@const.Value],
            PosSelectorFromResult fromResult => [
                .. fromResult.LastStepCount == 1 ? ResolvePosSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult) : [],
                .. fromResult.LastStepCount == 2 ? ResolvePosSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult) : [],
                .. fromResult.LastStepCount == 3 ? ResolvePosSelectorFromResult(fromResult.FromResultFilter, resultWrap?.PreResult?.PreResult?.PreResult) : [],
            ],
            PosSelectorFromResultItem fromResultItem => 
                from item in
                (IEnumerable<IItem>)[
                    .. fromResultItem.LastStepCount == 1 ? ResolveItemSelectorFromResult(fromResultItem.FromResultFilter, resultWrap?.PreResult) : [],
                    .. fromResultItem.LastStepCount == 2 ? ResolveItemSelectorFromResult(fromResultItem.FromResultFilter, resultWrap?.PreResult?.PreResult) : [],
                    .. fromResultItem.LastStepCount == 3 ? ResolveItemSelectorFromResult(fromResultItem.FromResultFilter, resultWrap?.PreResult?.PreResult?.PreResult) : [],
                ]
                select item.InPlay.PivotPos,
            null => [],
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(PosSelectorBase)}类型: {iCanSelectPos.GetType()}.")
        };
        return ApplyPosFilterAndSort(rawList, p => p, iCanSelectPos, selfItem, resultWrap);
    }
    
    IEnumerable<T> ApplyPosFilterAndSort<T>(IEnumerable<T> source, Func<T, Vector2Int> getPos, ICanSelectPos? iCanSelectPos, IItem selfItem, ResultWrap? resultWrap)
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
                    where ResolveItemSelector(selfItem, in3X3.ItemSelector, resultWrap).Select(item => item.InPlay)
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
                    where ResolveItemSelector(selfItem, inManDis.ItemSelector, resultWrap).Select(item => item.InPlay)
                        .All(item => 
                            Math.Abs(getPos(element).X - item.PivotPos.X) 
                            + Math.Abs(getPos(element).Y - item.PivotPos.Y) 
                            <= inManDis.Dis) // 同上
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
    UniTask EttAddSymbolModifyPropAsync(IItem from, IItem to, EPropType propType, long value, ResultWrap? resultWrap, CancellationToken ct)
    {
        to.ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            From = from,
            AddValue = value
        });
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to.InPlay)
        {
            CtxList = [new ResultItemWrap.CtxAddPropX{PropType = propType,Value = value}]
        });
        return UniTask.CompletedTask;
    }
    [Obsolete("某物让某物属性变化(乘算)")]
    UniTask EttMulSymbolModifyPropAsync(IItem from, IItem to, EPropType propType, long value, ResultWrap? resultWrap, CancellationToken ct)
    {
        to.ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            From = from,
            MultiValue = value
        });
        resultWrap?.Success = true;
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to.InPlay)
        {
            CtxList = [new ResultItemWrap.CtxMulPropX{PropType = propType,Value = value}]
        });
        return UniTask.CompletedTask;
    }
}