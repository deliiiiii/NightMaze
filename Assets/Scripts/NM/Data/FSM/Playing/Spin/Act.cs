using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using NM.Config;
namespace NM.Data;

public record ResultWrap(ItemDesResultBase Result, ResultWrap? PreResult)
{
    public readonly ItemDesResultBase Result = Result;
    public bool Success = true;
    public readonly ResultWrap? PreResult = PreResult;
    public List<ResultItemWrap> ItemWraps = [];
    public List<ResultPosWrap> PosWraps = [];
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
}
public record ResultPosWrap(Vector2Int Pos)
{
    public Vector2Int Pos = Pos;
    public List<CtxBase> CtxList = [];
    public abstract record CtxBase;
}

[ActContainer]
public partial class PlaySpin
{
    [Obsolete("准备执行物体整个词条")]
    async UniTask CheckItemAsync(IItem item, CancellationToken ct)
    {
        if (!BelongNode.Items.Contains(item.InPlay))
            return;
        // MyDebug.Log($"执行物体 pos:{item.PivotPos}");
        await item.OnSpin(ct);
        await UniTask.Delay(1000, cancellationToken: ct);
    }
    public record EvtBeforeCheckSymbol(PlaySpin WhoHasCt, IItem Item) : EvtBase<PlaySpin>(WhoHasCt);

   
    [Obsolete("准备执行物体单个词条Result")]
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
                from toSpawn in ResolveItemSelector(selfItem, spawnXAtX.ItemSelector, resultWrap).FirstOptional().ToIEnumerable()
                let toSpawnInPlay = toSpawn.InPlay
                from pos in ResolvePosSelector(selfItem, spawnXAtX.PosSelector, resultWrap, p =>
                {
                    toSpawnInPlay.PivotPos = p;
                    return BelongNode.TrySetItem(toSpawnInPlay);
                }).FirstOptional().ToIEnumerable()
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
            ItemDesConditionCollectXItem collectXItem => 
                ResolveItemSelector(selfItem, collectXItem.ItemSelector, resultWrap).Sum(_ => 1) 
                >= ResolveIntSelector(selfItem, collectXItem.MinValueSelector, resultWrap),
            _ => throw new InvalidOperationException
                ($"没有匹配穷尽{nameof(ItemDesConditionBase)}类型: {conditionBase.GetType()}.")
        };
        var nextRet = ResolveCondition(selfItem, conditionBase.Next, resultWrap);
        return thisRet && nextRet;
    }
    IEnumerable<IItem> ResolveItemSelector(IItem selfItem, ItemSelectorBase itemSelector, ResultWrap? resultWrap)
    {
        var rawItems = itemSelector switch
        {
            ItemSelectorAllPresentItem allPresentItem => Items,
            // TODO 丰富结果选项
            ItemSelectorFromResult selectorFromResult => resultWrap?.PreResult?.ItemWraps.Select(w => w.Item.InSpin(this)) ?? [],
            ItemSelectorItem selectorItem => 
                from iItemConfig in (List<IItemConfig>)[
                ..selectorItem.GridList,
                ..selectorItem.SymbolList, 
                ..selectorItem.BuildingList,
                ..selectorItem.ResourceList]
                select iItemConfig switch
                {
                    GridConfig gridConfig => new GamePlaying.Grid(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    BuildingConfig buildingConfig => new GamePlaying.Building(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    ResourceConfig resourceConfig => new GamePlaying.Resource(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    SymbolConfig symbolConfig => new GamePlaying.Symbol(iItemConfig.ID, Vector2Int.Zero).InSpin(this),
                    _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(IItemConfig)}类型: {iItemConfig.GetType()}.")
                },
            ItemSelectorItemSet selectorItemSet => 
                from itemInSpin in Items
                let itemInPlay = itemInSpin.InPlay
                let set = selectorItemSet.Set
                where set.GridList.Contains(itemInPlay.Config) 
                      || set.SymbolList.Contains(itemInPlay.Config) 
                      || set.BuildingList.Contains(itemInPlay.Config)
                      || set.ResourceList.Contains(itemInPlay.Config)
                select itemInSpin,
            ItemSelectorSelf selectorSelf => [selfItem],
            ItemSelectorTag selectorTag => from itemInSpin in Items
                let itemInPlay = itemInSpin.InPlay
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
                let itemInPlay = itemInSpin.InPlay
                let selfItemInPlay = selfItem.InPlay
                where filter1 switch
                {
                    null => true,
                    ItemFilterIn3X3 in3X3 => Math.Abs(itemInPlay.PivotPos.X - selfItemInPlay.PivotPos.X) <= 1 
                                             && Math.Abs(itemInPlay.PivotPos.Y - selfItemInPlay.PivotPos.Y) <= 1,
                    ItemFilterInManDis inManDis => Math.Abs(itemInPlay.PivotPos.X - selfItemInPlay.PivotPos.X) 
                                                   + Math.Abs(itemInPlay.PivotPos.Y - selfItemInPlay.PivotPos.Y) 
                                                   <= inManDis.Dis,
                    ItemFilterIsItemType isItemType => isItemType.ItemType != 0 && isItemType.ItemType.HasFlag(itemInPlay.ItemType),
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
                    let itemInPlay = itemInSpin.InPlay
                    orderby itemInPlay.PivotPos.X * sort.DescendingValue, itemInPlay.PivotPos.Y * sort.DescendingValue
                    select itemInSpin;
                break;
            case ItemSortPosUpLeft sort:
                rawItems =
                    from itemInSpin in rawItems
                    let itemInPlay = itemInSpin.InPlay
                    orderby -itemInPlay.PivotPos.Y * sort.DescendingValue, itemInPlay.PivotPos.X * sort.DescendingValue
                    select itemInSpin;
                break;
            case null:
                break;
            default:
                throw new InvalidOperationException($"没有匹配穷尽{nameof(ItemSortBase)}类型: {itemSelector.ItemSort.GetType()}.");
        }
        return rawItems.Take(ResolveIntSelector(selfItem, itemSelector.TakeMax, resultWrap));
    }
    int ResolveIntSelector(IItem selfItem, IntSelectorBase intSelector, ResultWrap? resultWrap) =>
        intSelector switch
        {
            IntSelectorConst selectorConst => selectorConst.Value,
            IntSelectorInfinite selectorInfinite => int.MaxValue,
            IntSelectorSumBy selectorSumBy => ResolveItemSelector(selfItem, selectorSumBy.ItemSelector, resultWrap)
                .Sum(_ => selectorSumBy.Value),
            _ => throw new InvalidOperationException($"没有匹配穷尽{nameof(IntSelectorBase)}类型: {intSelector.GetType()}.")
        };

    IEnumerable<Vector2Int> ResolvePosSelector(IItem selfItem, PosSelectorBase posSelector, ResultWrap? resultWrap, Func<Vector2Int, bool>? extraFilter = null)
    {
        var rawList = posSelector switch
        {
            PosSelector3X3 selector3X3 =>
                from dx in Range(-1, 3)
                from dy in Range(-1, 3)
                let itemInPlay = selfItem.InPlay
                select new Vector2Int(itemInPlay.PivotPos.X + dx, itemInPlay.PivotPos.Y + dy),
            PosSelectorConst selectorConst => [selectorConst.Value],
            // TODO 丰富结果选项
            PosSelectorFromResult selectorFromResult => resultWrap?.PreResult?.PosWraps.Select(w => w.Pos) ?? [],
            // TODO 丰富结果选项
            PosSelectorFromResultItem selectorFromResultItem => resultWrap?.PreResult?.ItemWraps.Select(w => w.Item.PivotPos) ?? [],
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

        extraFilter ??= RTrue1;
        rawList = rawList.Where(extraFilter);
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
        return rawList.Take(ResolveIntSelector(selfItem, posSelector.TakeMax, resultWrap));
    }

    [Obsolete("某物让某物属性变化(加算)")]
    UniTask EttAddSymbolModifyPropAsync(IItem from, IItem to, EPropType propType, long value, ResultWrap? resultWrap, CancellationToken ct)
    {
        to.ModifyPropList.Add(new ModifyPropInfo
        {
            PropType = propType,
            Ett = from,
            AddValue = value
        });
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
            Ett = from,
            MultiValue = value
        });
        resultWrap?.ItemWraps.Add(new ResultItemWrap(to.InPlay)
        {
            CtxList = [new ResultItemWrap.CtxMulPropX{PropType = propType,Value = value}]
        });
        return UniTask.CompletedTask;
    }
}