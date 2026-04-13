using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [DebuggerStepThrough]public override string ToString() => nameof(GamePlaying);
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
    [JsonProperty(IsReference = false, Order = 9999)]List<MyItem> itemList = [];
    
    public IEnumerable<MyItem> Items => itemList;
    public IEnumerable<MyItem> Grids => itemList.Where(item => item.Config.IsGrid);
    
    public IEnumerable<Vector2Int> GridPoses =>
        from item in itemList
        where item.Config.IsGrid && item.ReallyInWorld
        from coveredPos in item.CoveredPosList
        select coveredPos;
    public IEnumerable<Vector2Int> NonGridPoses =>
        from item in itemList
        where !item.Config.IsGrid && item.ReallyInWorld
        from coveredPos in item.CoveredPosList
        select coveredPos;
    public IEnumerable<MyItem> EmptyGrids
    {
        get
        {
            var occupiedPoses = NonGridPoses.ToHashSet();
            return 
                from grid in Grids
                where !occupiedPoses.Contains(grid.PivotPos)
                select grid;
        }
    }

    public IEnumerable<ItemDesConfig> ItemDesConfigs =>
        from item in itemList
        orderby item.PivotPos.Y descending, item.PivotPos.X
        from desConfig in item.Config.DesList
        select desConfig;

    protected override void OnCreateFreshData()
    {
        (from x in Range(1, 8) 
            from y in Range(1, 8)
            select new Vector2Int(x, y))
            .ForEach(pos => itemList.Add(new MyItem(1, pos)));
        // EmptyGrids
        //     .ToList()
        //     .Take(5)
        //     .ForEach(grid => AddEttCom<EttSymbol, Symbol>(new Symbol(EttSymbol.Create(), 1, grid.PivotPos)));
        
        // EmptyGrids
        //     .ToList()
        //     .Take(2)
        //     .ForEach(grid => itemList.Add(new MyItem(1111, grid.PivotPos)));
        
        // EmptyGrids
        //     .ToList()
        //     .Take(5)
        //     .ForEach(grid => itemList.Add(new MyItem(1, grid.PivotPos)));
        
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