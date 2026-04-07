using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;
using Sirenix.Utilities;

namespace NM.Data;

[Serializable]
public partial class GamePlaying : RootStateBase<GamePlaying>
{
    [JsonConstructor] GamePlaying() { } 
    public GamePlaying(string playerName)
    {
        PlayerName = playerName;
    }
    public override string ToString() => nameof(GamePlaying);
    public string PlayerName { get; private set;}= "Deli";
    public double PlayTime { get; private set;}
    [EvtChanged]public partial long Prop1 { get;private set; }
    [EvtChanged]public partial long Prop2 { get;private set; }
    [EvtChanged]public partial long Prop3 { get;private set; }
    [EvtChanged]public partial long Prop4 { get;private set; }
    [EvtChanged]public partial long Prop5 { get;private set; }
    [EvtChanged]public partial long Prop6 { get;private set; }
    [EvtChanged]
    public partial long Coin { get; private set;}
    // 标注[EvtChanged]则源生↓↓↓
    // public long Coin
    // {
    //     get;
    //     private set
    //     {
    //         field = value;
    //         Bus.FireAndForget(new EvtCoinChanged(value));
    //     }
    // }
    // public record EvtCoinChanged(GamePlaying gamePlaying,
    //              long OldValue,
    //              long NewValue): EvtForgetBase;
    public IEnumerable<Grid> Grids => Items.OfType<Grid>();
    public IEnumerable<Symbol> Symbols => Items.OfType<Symbol>();
    public IEnumerable<Building> Buildings => Items.OfType<Building>();
    public IEnumerable<Resource> Resources => Items.OfType<Resource>();

    public IEnumerable<IItem> Items
    {
        get
        {
            foreach (var grid in GetComs<Grid>())
            {
                yield return grid;
            }
            foreach (var symbol in GetComs<Symbol>())
            {
                yield return symbol;
            }
            foreach (var building in GetComs<Building>())
            {
                yield return building;
            }
            foreach (var resource in GetComs<Resource>())
            {
                yield return resource;
            }
        }
    }
    
    public MyOption<IItem> GetItemByEtt(EttBase ett)
    {
        return ett switch
        {
            EttGrid grid => GetEttComOptional<EttGrid, Grid>(grid).Map<IItem>(x => x),
            EttSymbol symbol => GetEttComOptional<EttSymbol, Symbol>(symbol).Map<IItem>(x => x),
            EttBuilding building => GetEttComOptional<EttBuilding, Building>(building).Map<IItem>(x => x),
            EttResource resource => GetEttComOptional<EttResource, Resource>(resource).Map<IItem>(x => x),
            _ => throw new System.Exception($"没有匹配穷尽EttBase{nameof(EttBase)}类型: {ett.GetType()}.")
        };
    }

    public IEnumerable<Grid> EmptyGrids
    {
        get
        {
            var posSet = (
                from item in Items
                where item is not Grid
                from coveredPos in item.CoveredPosList
                select coveredPos).ToHashSet();
            return 
                from grid in Grids
                where !posSet.Contains(grid.PivotPos)
                select grid;
        }
    }

    public IEnumerable<ItemDesConfig> ItemDesConfigs =>
        from item in Items
        orderby item.PivotPos.Y descending, item.PivotPos.X
        from desConfig in item.Config.DesList
        select desConfig;

    protected override void OnCreateFreshData()
    {
        (from x in Range(1, 8) 
            from y in Range(1, 8)
            select new Vector2Int(x, y))
            .ForEach(pos => AddEttCom<EttGrid, Grid>(new Grid(EttGrid.Create(), 1, pos)));
        EmptyGrids
            .ToList()
            .Take(5)
            .ForEach(grid => AddEttCom<EttSymbol, Symbol>(new Symbol(EttSymbol.Create(), 1, grid.PivotPos)));
        
        EmptyGrids
            .ToList()
            .Take(5)
            .ForEach(grid => AddEttCom<EttResource, Resource>(new Resource(EttResource.Create(), 1, grid.PivotPos)));
        
        state = new PlayIdle();
    }
    
    protected override async UniTask OnLaunchCom(bool isThisFromLoad)
    {
        // await symbolDeckList.EachOnLaunchCom(isThisFromLoad);
        await state!.OnCreateAsync(isThisFromLoad);
    }
    protected override void OnReleaseCom()
    {
        state?.OnRemove();
        // symbolDeckList.EachOnReleaseCom();
    }

    protected override void OnSelfTick(float dt)
    {
        PlayTime += dt;
    }

    Node? state;
    public UniTask ChangeStateAsync<T>(T node, bool isNewFromLoad) where T : PlayStateBase<T>
        => _ChangeAsync(ref state, node, isNewFromLoad);
    public MyOption<T> GetStateOptional<T>() where T : PlayStateBase<T>
        => state is T s ? s : None;
    public bool IsState<T>() where T : PlayStateBase<T>
        => state is T;
}

public abstract class PlayStateBase<T> : Node<GamePlaying, T> where T : PlayStateBase<T>
{
    
}