using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public string PlayerName { get; private set;} = "Deli";
    public double PlayTime { get; private set;}
    [EvtChanged] public partial int TurnCount { get; private set; } = 1;
    public int CurLayer { get; private set; } = 1;
    public long PropBody { get;private set; }
    public long PropSans { get;private set; }
    public long PropLore { get;private set; }
    public long PropLoyalty { get;private set; }
    public long PropLoyaltyMax { get; private set; } = 1000;
    [EvtChanged] public partial long PropHostility { get; private set; } = 100;
    [EvtChanged] public partial long PropHostilityMax { get; private set; } = 1000;
    public const int AddHostilityPerTurn = 1;
    [JsonProperty(IsReference = false, Order = 9000)]List<MyItem> itemList = [];
    
    public long GetProp(EPropType propType)
        => propType switch
        {
            EPropType.Prop1 => PropBody,
            EPropType.Prop2 => PropSans,
            EPropType.Prop3 => PropLore,
            EPropType.PropA1 => PropLoyalty,
            EPropType.PropA2 => PropHostility,
            _ => throw new ArgumentOutOfRangeException(nameof(propType), propType, null)
        };
    public long GetMaxProp(EPropType propType)
        => propType switch
        {
            EPropType.PropA1 => PropLoyaltyMax,
            EPropType.PropA2 => PropHostilityMax,
            _ => throw new ArgumentOutOfRangeException(nameof(propType), propType, null)
        };
    
    public IEnumerable<MyItem> Items => itemList;
    public IEnumerable<MyItem> Grids => itemList.Where(item => item.Config.IsGrid);
    
    public IEnumerable<Vector2Int> GridPoses =>
        from item in itemList
        where item.Config.IsGrid && item.ReallyInWorld
        from coveredPos in item.CoveredPosList
        select coveredPos;
    public IEnumerable<MyItem> EmptyGrids
    {
        get
        {
            var occupiedPoses = 
                // 非grid的, 且不能放置的位置
                (from item in itemList
                where item is { 
                    ReallyInWorld: true, 
                    Config.IsGrid: false, 
                    Config.IsBuildingOrEvent: false }
                from coveredPos in item.CoveredPosList
                select coveredPos).ToHashSet();
            return 
                from grid in Grids
                where !occupiedPoses.Contains(grid.PivotPos)
                select grid;
        }
    }
    public bool SatisfyBuildingRun(MyItem item)
    {
        // if (!item.Config.IsBuilding)
        //     return false;
        var toRun = (from runProp in item.Config.RunPropValueList
            let playerProp = GetProp(runProp.Key)
            select (propType: runProp.Key, cur: playerProp, tar: runProp.Value)).ToList();
        if(toRun.Any(r => r.cur < r.tar))
            return false;
        return true;
    }

    protected override void OnCreateFreshData()
    {
        (from x in Range(1, 8) 
            from y in Range(1, 8)
            select new Vector2Int(x, y))
            .ForEach(pos => itemList.Add(new MyItem(50001, pos)));
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
        await state!.OnCreateAsync(isThisFromLoad);
    }
    protected override void OnReleaseCom()
    {
        state?.OnRemove();
    }
    protected override void OnSelfTick(float dt)
    {
        base.OnSelfTick(dt);
        PlayTime += dt;
    }
    #region state
    [JsonProperty(Order = 10000)]
    Node? state;
    UniTask ChangeStateAsync<T>(T node, bool isNewFromLoad) where T : PlayStateBase<T>
        => _ChangeAsync(ref state, node, isNewFromLoad);
    public MyOption<T> GetStateOptional<T>() where T : PlayStateBase<T>
        => state is T s ? s : None;
    public bool IsState<T>() where T : PlayStateBase<T>
        => state is T;
    #endregion
}

public abstract class PlayStateBase<T> : Node<GamePlaying, T> where T : PlayStateBase<T>;