using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using Newtonsoft.Json;
using NM.Config;

namespace NM.Data;

[Serializable]
public partial class GamePlaying : RootStateBase<GamePlaying>
{
    [JsonConstructor] GamePlaying() { } 
    public GamePlaying(string playerName)
    {
        PlayerName = playerName;
        toDoList = [..
            from pos in InAreaPoses(new(2, 1))
            select new ActSpawnItemAtPos(this)
            {
                Id = 50001,
                Pos = pos,
                ResultWrap = null
            },
            new ActUnlockArea(this) { AreaPos = new Vector2Int(2,1) },
            new ActWaitForClickStartTurn(this)
        ];
    }
    [DebuggerStepThrough]public override string ToString() => nameof(GamePlaying);
    public string PlayerName { get; set;} = "Deli";
    public double PlayTime { get; private set;}
    [EvtChanged] public partial int TurnCount { get; private set; } = 1;
    [EvtChanged] public partial int CurLayer { get; private set; } = 0;
    public long PropBody { get;private set; }
    public long PropSans { get;private set; }
    public long PropLore { get;private set; }
    public long PropLoyalty { get;private set; }
    public long PropLoyaltyMax { get; private set; } = 1000;
    [EvtChanged] public partial long PropHostility { get; private set; } = 100;
    [EvtChanged] public partial long PropHostilityMax { get; private set; } = 1000;
    public const int AddHostilityPerTurn = 1;
    public HashSet<Vector2Int> RevealedAreaSet = [];
    public const int AreaMinX = 1;
    public const int AreaMaxX = 3;
    public const int AreaMinY = 1;
    public const int AreaMaxY = 6;
    public const int AreaWidth = 8;
    public const int AreaHeight = 8;
    public const int CorridorWidth = 1;
    [JsonProperty(IsReference = false, Order = 9000)]List<MyItem> itemList = [];
    
    
    [JsonProperty(Order = 9999)]List<IUniAction> toDoList = [];
    public IEnumerable<IUniAction> ToDoList => toDoList;
    [JsonIgnore] UniTask curtask;
    int FindAfterId(Func<IUniAction, bool>? beforeWho = null)
    {
        beforeWho ??= RFalse1;
        int beforeId = toDoList.IndexOf(toDoList.FirstOrDefault(beforeWho));
        // 未找到返回-1, 代表头插
        return beforeId;
    }
    void InsertAfter(IUniAction act, Func<IUniAction, bool>? afterWho = null)
    {
        // MyDebug.LogError("insert:" + act.GetType().Name);
        toDoList.Insert(FindAfterId(afterWho) + 1, act);
    }

    void InsertAfter(IEnumerable<IUniAction> actList, Func<IUniAction, bool>? afterWho = null)
    {
        // MyDebug.LogError("insert:" + string.Join(',', actList.Select(ac => ac.GetType().Name)));
        toDoList.InsertRange(FindAfterId(afterWho) + 1, actList);
    }

    void InsertButCancelFirstAndDoFirst(IEnumerable<IUniAction> actList)
    {
        var first = toDoList.First();
        IUniAction newFirst = first switch
        {
            ActWaitForClickStartTurn => new ActWaitForClickStartTurn(this),
            _ => new ActDoNothing(this)
        };
        first.CancelSelfly();
        InsertAfter([..actList, newFirst]);
    }
    
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
            EPropType.Prop1 => long.MaxValue,
            EPropType.Prop2 => long.MaxValue,
            EPropType.Prop3 => long.MaxValue,
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
    Vector2Int GetAreaPos(Vector2Int pivotPos)
    {
        int areaX = (pivotPos.X - 1) / (AreaWidth + CorridorWidth) + 1;
        int areaY = (pivotPos.Y - 1) / (AreaHeight + CorridorWidth) + 1;
        return new Vector2Int(areaX, areaY);
    }

    IEnumerable<Vector2Int> InAreaPoses(Vector2Int areaPos)
        => from x in Range(1, AreaWidth)
            from y in Range(1, AreaHeight)
            select new Vector2Int(
                1 + (areaPos.X - 1) * (AreaWidth + CorridorWidth) + x - 1,
                1 + (areaPos.Y - 1) * (AreaHeight + CorridorWidth) + y - 1);

    bool InAreaMaxRange(Vector2Int areaPos) =>
        areaPos is 
        { 
            X : >= AreaMinX and <= AreaMaxX, 
            Y : >= AreaMinY and <= AreaMaxY
        };
    public bool IsRevealed(Vector2Int pos) => RevealedAreaSet.Contains(GetAreaPos(pos));
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
    public CurTechInfo? GetCurTechInfo()
    {
        var curNodes = TechTreeData.GetCurNodesGroupByDis();
        if (!curNodes.Any())
            return null;
        var curNode = curNodes.First().Nodes.First();
        var nodeConfig = curNode.Config;
        if (nodeConfig.RequireDic == null)
            return null;
        return new CurTechInfo
        {
            Node = curNode,
            TarDic = nodeConfig.RequireDic.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CurDic = curNode.CarValueDic
        };
    }
    protected override UniTask OnLaunchCom(bool isThisFromLoad)
    {
        TechTreeData.OnLoad();
        curtask = StartTodo();
        curtask.Forget();
        return UniTask.CompletedTask;
    }
    async UniTask StartTodo()
    {
        while (!CurCt.IsCancellationRequested)
        {
            if (!toDoList.Any())
            {
                await UniTask.Yield(CurCt);
                continue;
            }
            var first = toDoList[0];
            try
            {
                await first;
            }
            catch (OperationCanceledException e)
            {
                if (!first.IsCancelledSelfly)
                    throw;
            }
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

public struct CurTechInfo
{
    public TechNodeData Node;
    public Dictionary<EPropType, long> TarDic;
    public Dictionary<EPropType, long> CurDic;
}