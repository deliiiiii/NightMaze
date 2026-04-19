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
        (from x in Range(1, 8) 
                from y in Range(1, 8)
                select new Vector2Int(x, y))
            .ForEach(pos => itemList.Add(new MyItem(50001, pos)));
        toDoList = [
            new ActWaitForClickStartTurn(this)
        ];
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
    
    
    [JsonProperty(Order = 9999)]List<IUniAction> toDoList = [];
    public IEnumerable<IUniAction> ToDoList => toDoList;
    int FindAfterId(Func<IUniAction, bool>? beforeWho = null)
    {
        beforeWho ??= RTrue1;
        int beforeId = toDoList.IndexOf(toDoList.FirstOrDefault(beforeWho));
        return beforeId;
    }
    void InsertAfter(IUniAction act, Func<IUniAction, bool>? afterWho = null) => 
        toDoList.Insert(FindAfterId(afterWho) + 1, act);
    void InsertAfter(IEnumerable<IUniAction> actList, Func<IUniAction, bool>? afterWho = null) => 
        toDoList.InsertRange(FindAfterId(afterWho) + 1, actList);
    
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
    public IEnumerable<Vector2Int> GridPoses =>
        from item in itemList
        where item is
        {
            ReallyInWorld : true,   
            Config.IsGrid : true,
        }
        from cov in item.CoveredPosList
        select cov;
    public IEnumerable<Vector2Int> BuildingOrEvtPoses =>
        from item in itemList
        where item is
        {
            ReallyInWorld: true,
            Config.IsBuildingOrEvent: true
        }
        from cov in item.CoveredPosList
        select cov;
    public IEnumerable<Vector2Int> NotGridBuildingOrEvtPoses =>
        from item in itemList
        where item is
        {
            ReallyInWorld: true,
            Config.IsGrid: false,
            Config.IsBuildingOrEvent: false
        }
        from cov in item.CoveredPosList
        select cov;
    public bool TrySetItem(MyItem item) =>
        item switch
        {
            _ when item.Config.IsGrid => item.CoveredPosList.All(pos => !GridPoses.Contains(pos)),
            _ when item.Config.IsBuildingOrEvent => 
                item.CoveredPosList.All(pos => GridPoses.Contains(pos)) &&
                item.CoveredPosList.All(pos => !BuildingOrEvtPoses.Contains(pos)),
            _ => 
                item.CoveredPosList.All(pos => GridPoses.Contains(pos)) &&
                item.CoveredPosList.All(pos => !NotGridBuildingOrEvtPoses.Contains(pos))
        };

    public IEnumerable<MyItem> GetToRemove(MyItem firstRemove)
    {
        if (!itemList.Contains(firstRemove))
            return [];
        if (!firstRemove.Config.IsGrid)
            return [firstRemove];
        // 如果删除地块，则删去其上面所有的其他物体
        return
        [
            firstRemove, ..
            from onGridItem in itemList
            where onGridItem != firstRemove 
                  && onGridItem.CoveredPosList.Intersect(firstRemove.CoveredPosList).Any()
            select onGridItem
        ];
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
    protected override UniTask OnLaunchCom(bool isThisFromLoad)
    {
        TechTreeData.OnLoad();
        StartTodo().Forget();
        return UniTask.CompletedTask;
    }
    async UniTask StartTodo()
    {
        while (toDoList.Any())
        {
            var first = toDoList[0];
            await first;
            toDoList.Remove(first);
        }
    }
    protected override void OnSelfTick(float dt)
    {
        base.OnSelfTick(dt);
        PlayTime += dt;
    }
    [JsonProperty(Order = 10000)] public PlaySpin? InSpin;
    public MyOption<PlaySpin> GetSpinOptional() => InSpin != null ? InSpin : None;
    public TechTreeData TechTreeData = new();
}