using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace NM.View;
public class PlayView : ViewBase<GamePlaying>
{
    [Header("上-左")]
    public List<PropValueView> PropValueViewList = [];
    [Header("上-中")]
    public Txt TxtLayerCount;
    public Txt TxtTurnCount;
    public Txt TxtNextSoftDdl;
    [Header("上-右")]
    public Txt TxtLoyalty;
    public Txt TxtHostility;
    [Header("上中-即时信息")]
    public InstantInfoView InstantInfoView;
    [Header("上右")]
    public Btn BtnSave;
    public Btn BtnExit;
    public Btn BtnSetting;
    [Header("中-地图")]
    [SerializeField] Trs gridTrs;
    [SerializeField] BoxCollider2D colliderConfiner;
    public Trs TrsToBuild;
    public ItemView PfbItemView;
    [SerializeField] LineRenderer lr;
    readonly List<ItemView> itemViewList = [];
    IEnumerable<ItemView> GridViews => itemViewList.Where(i => i.Data.Config.IsGrid);
    [Header("中-地图格详情")]
    public GridDetail GridDetail; 
    public Vector2Int? LockedPosDetail;
    [Header("中-物体事件")]
    [SerializeField] Trs trsItemEvtView;
    [SerializeField] ItemEvtView pfbItemEvtView;
    readonly List<ItemEvtView> itemEvtViewList = [];
    
    [Header("下")]
    public Btn BtnSpin;
    public Btn BtnNextTurn;
    public Btn BtnTechTree;
    public TechTreeView TechTreeView;
    
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSave.onClick.EvtBindTo(() => Saver.SaveAsync(Const.Name.Save.SlotFolder, Data.PlayerName, Data));
        yield return BtnExit.onClick.EvtBindTo(() => new GamePlaying.EvtClickExit().Forget());
        yield return BtnSetting.onClick.EvtBindTo(() => SettingViewIns.SetActiveTrue());
        
        yield return BtnSpin.onClick.EvtBindTo(() => new GamePlaying.EvtClickStartTurn().Forget());
        yield return BtnNextTurn.onClick.EvtBindTo(() => new GamePlaying.EvtClickNextTurn().Forget());
        
        yield return BtnTechTree.onClick.EvtBindTo(() => TechTreeView.LoadFromConfigRT());
    }
    void Update()
    {
        if (LockedPosDetail != null) 
            ShowGridDetailAtPos(LockedPosDetail.Value);
        BtnSpin.interactable = Data.ToDoList.FirstOrDefault() 
            is GamePlaying.ActWaitForClickStartTurn;
        BtnNextTurn.interactable = (
            from spin in Data.GetSpinOptional()
            select spin.IsWaitClickNextTurn) 
            | false;
    }

    #region OnEvt
    int spawnC = 0;
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = async (evt, ct) =>
        {
            Data = evt.WhoHasCt;
            
            int tarCount = Data.Items.Count();
            int curCount = 0;
            LoadingViewIns.Register(
                () => GameRoot.ChangeStateAsync(new GameTitle(), false).Forget(),
                getProgress: () => curCount * 1f / tarCount
                );
            await Data.Items.ForEachAsync(async item =>
            {
                await SpawnItemAsync(item, ct);
                curCount++;
                if(spawnC++ % Const.Await.OneFramePerSpawn == 0)
                    await UniTask.Yield(cancellationToken: ct);
            });
            RefreshTurnAndSoOn();
            RefreshItemEvt();
            PropValueViewList.ForEach(view => view.Refresh(Data));
            RefreshGridEdge();
            RefreshConfinerAndFog();
            lr.SetActiveTrue();
            gameObject.SetActiveTrue();
            LoadingViewIns.Release();
        },
        Des = "(进入Root - Playing状态时) 恢复游戏"
    };
    void RefreshTurnAndSoOn()
    {
        TxtTurnCount.text = Data.TurnCount.ToString();
        TxtLayerCount.text = Data.CurLayer.ToString();
    }
    UniEvt<GamePlaying.EvtOnExit> OnExit => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = null!;
            ClearAllGrid();
            ClearAllItemEvt();
            GridDetail.SetActiveFalse();
            LockedPosDetail = null;
            if(lr != null)
                lr.SetActiveFalse();
            gameObject.SetActiveFalse();
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };
    UniEvt<GamePlaying.EvtStartSpin> OnStartSpin => new()
    {
        Invoke = (evt, ct) =>
        {
            ClearAllItemEvt();
            return UniTask.CompletedTask;
        },
        Des = "清空已完成事件列表"
    };
    UniEvt<GamePlaying.EvtEndSpin> OnEndSpin => new()
    {
        Invoke = (evt, ct) =>
        {
            RefreshItemEvt();
            return UniTask.CompletedTask;
        },
        Des = "刷新已完成事件列表"
    };
    
    UniEvt<GamePlaying.EvtTurnCountChanged> OnTurnCountChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            TxtTurnCount.text = evt.NewValue.ToString();
            return UniTask.CompletedTask;
        },
        Des = "更新文本",
    };

    UniEvt<GamePlaying.EvtCurLayerChanged> OnCurLayerChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            TxtLayerCount.text = evt.NewValue.ToString();
            return UniTask.CompletedTask;
        },
        Des = "更新文本",
    };
    
    UniEvt<GamePlaying.EvtOnTick> OnTickSpin => new()
    {
        Invoke = (evt, ct) =>
        {
            PropValueViewList.ForEach(view => view.Refresh(Data));
            return UniTask.CompletedTask;
        },
        Des = "刷新属性显示"
    };
    UniEvt<GamePlaying.EvtSpawnItem> OnSpawnItemAtPos => new()
    {
        Invoke = async (evt, ct) =>
        {
            await SpawnItemAsync(evt.Item, ct);
            if(spawnC++ % Const.Await.OneFramePerSpawn == 0)
                await UniTask.Yield(cancellationToken: ct);
        },
        Des = "生成物体",
    };
    UniEvt<GamePlaying.EvtMoveItem> OnMoveItem => new()
    {
        Invoke = (evt, ct) =>
        {
            MoveItem(evt.Item);
            return UniTask.CompletedTask;
        },
        Des = "移动物体",
    };
    UniEvt<GamePlaying.EvtRemoveItem> OnRemoveItem => new()
    {
        Invoke = (evt, ct) =>
        {
            RemoveItem(evt.ToRemove);
            return UniTask.CompletedTask;
        },
        Des = "移除物体",
    };
    UniEvt<GamePlaying.EvtUnlockArea> OnUnlockArea => new()
    {
        Invoke = (evt, ct) =>
        { 
            RefreshConfinerAndFog();
            return UniTask.CompletedTask;
        },
        Des = "刷新迷雾",
    };
    #endregion
    
    #region GridEdge
    [Header("中-GridEdge")]
    static readonly Dictionary<Vector2Int, Func<ItemView, Trs>> p1Dic = new()
    {
        [Vector2Int.Up   ] = v => v.Lu,
        [Vector2Int.Right] = v => v.Ru,
        [Vector2Int.Down ] = v => v.Rd,
        [Vector2Int.Left ] = v => v.Ld,
    };
    static readonly Dictionary<Vector2Int, Func<ItemView, Trs>> p2Dic = new()
    {
        [Vector2Int.Up   ] = v => v.Ru,
        [Vector2Int.Right] = v => v.Rd,
        [Vector2Int.Down ] = v => v.Ld,
        [Vector2Int.Left ] = v => v.Lu,
    };
    void RefreshGridEdge()
    {
        var itemPosDic = itemViewList.Where(item => item.Data.Config.IsGrid).ToDictionary(item => item.Data.PivotPos);
        var edgeList = (
            from item in itemViewList
            where item.Data.Config.IsGrid
            from delta in (List<Vector2Int>)[Vector2Int.Up, Vector2Int.Down, Vector2Int.Left, Vector2Int.Right]
            where !itemPosDic.TryGetValue(item.Data.PivotPos + delta, out _)
            select new Edge
            {
                Point1 = p1Dic[delta](item).position,
                Point2 = p2Dic[delta](item).position,
            }).ToList();
        var lines = edgeList.ConnectToLines();
        if (lines.Any())
        {
            lr.positionCount = lines[0].Count;
            lr.SetPositions(lines[0].ToArray());
        }
    }
    #endregion
    
    #region GridDetail
    public void ShowGridDetailAtPos(Vector2Int gridPos)
    {
        List<DetailInfo> detailList =
        [
            ..
            from item in Data.Items
            where item.CoverPos(gridPos)
            orderby (int)item.ItemType
            select new DetailInfo
            {
                Type = item.Config.PrefixName,
                Name = item.Config.Name,
                TagInfoList = item.Config.DetailTagInfos,
                Detail = $"""
                          {item.PivotPos}
                          {ResolveBaseValue(item)}{ResolveBuildingOrEvt(item)}{(!Data.Items.Contains(item) ? "已不复存在" : string.Empty)}{ResolveItemDesList(item.AllConfigList)}
                          <color=grey>{item.Config.FlavorDes}</color>
                          """,
                InSpinLineList =
                [
                    ..
                    from spin in PlaySpinData.ToIEnumerable()
                    let itemInSpin = item[spin]
                    from modProp in itemInSpin.ModifyPropList
                    where modProp.HasValue
                    orderby modProp.PropType, modProp.AddValue descending, modProp.MultiValue descending
                    select $"{modProp.From.Config.Name} " +
                           modProp.PropType.GetLabelText() +
                           (modProp.AddValue != 0 ? modProp.AddValue.ToStringWithSymbol() : string.Empty) +
                           (Math.Abs(modProp.MultiValue - 1) > 1e-5 ? $"<color=green>x{modProp.MultiValue}</color>" : string.Empty),
                    ..
                    from spin in PlaySpinData.ToIEnumerable()
                    let itemInSpin = item[spin]
                    from distributeProp in itemInSpin.DistributePropList
                    orderby distributeProp.Value
                    select $"->{(distributeProp.ToTech
                        ? "TECH" : distributeProp.ToItem?.Config.Name ?? "YOU")}{distributeProp.PropType.GetLabelText()}{distributeProp.Value.ToStringWithSymbol()}"
                ]
            }
        ];
        GridDetail.SetActiveTrue();
        GridDetail.transform.position = GridToWorld(gridPos + new Vector2Int(1,1) * Const.World.GridSize);
        GridDetail.transform.SetLocalPositionZ(0);
        GridDetail.Refresh(detailList);
    }
    string ResolveBaseValue(GamePlaying.MyItem item)
    {
        if (!item.Config.IsSymbol)
            return string.Empty;
        var propValueList = item.Config.SymbolPropValueList;
        if (propValueList.Count == 0)
            return string.Empty;
        return "白值" + string.Join(',', propValueList
            .Select(pair => $"{pair.Key.GetLabelText()}{pair.Value.ToStringWithSymbol()}"))
               + "\n";
    }
    string ResolveBuildingOrEvt(GamePlaying.MyItem item)
    {
        if (!item.Config.IsBuildingOrEvent)
            return string.Empty;
        if (item.IsBuildingOrEventKanSei)
        {
            return item.Config.IsEvent
                ? "事件已完成."
                : "建筑运营消耗" + string.Join(',', item.Config.RunPropValueList.Select(pair =>
                    $"{pair.Key.GetLabelText()} {pair.Value}"));
        }
        return "建造/事件需要" + string.Join(',', item.BuildingOrEventProgress.Select(pair => 
            $"{pair.Key.GetLabelText()} {pair.Value}/{item.Config.BuildPropValueList.First(p => p.Key == pair.Key).Value}"));
    }
    string ResolveItemDesList(List<ItemDesConfig> desConfigList)
    {
        var ret = string.Join("\n", desConfigList.Select(ResolveItemDes));
        if (ret != string.Empty)
        {
            ret = $"\n{ret}<sprite name=\"GridBack\">";
        }
        return ret;
    }
    string ResolveItemDes(ItemDesConfig desConfig)
    {
        var sb = new StringBuilder();
        sb.Append(desConfig.DesToPlayer);
        return sb.ToString();
    }
    public void HideGridDetail()
    {
        // 如果删除了地块...
        if (LockedPosDetail != null && Data.GridPoses.Contains(LockedPosDetail.Value))
            return;
        GridDetail.SetActiveFalse();
    }
    #endregion

    #region Add/Remove Item
    void ClearAllGrid()
    {
        itemViewList.Where(item => item != null).ForEach(item => Destroy(item.gameObject));
        itemViewList.Clear();
        RefreshGridEdge();
    }
    UniTask SpawnItemAsync(GamePlaying.MyItem item, CancellationToken ct)
    {
        ItemView ins = Instantiate(PfbItemView);
        ins.OnCreateView(item);
        SetViewPosInternal(ins);
        ins.SetActiveTrue();
        
        itemViewList.Add(ins);
        if(item.Config.IsGrid)
            // TODO 如果同一帧创建与销毁大量物体, 疑似有性能问题.
            RefreshGridEdge();
        return UniTask.CompletedTask;
    }
    void MoveItem(GamePlaying.MyItem item)
    {
        ItemView? ins = itemViewList.FirstOrDefault(s => s.Data == item);
        if (ins == null)
        {
            MyDebug.LogError($"没有找到物体 {item} 对应的View.");
            return;
        }
        SetViewPosInternal(ins);
        
        if(item.Config.IsGrid)
            RefreshGridEdge();
    }
    void SetViewPosInternal(ItemView item)
    {
        if (item.Data.Config.IsGrid)
        {
            item.transform.parent = gridTrs;
            item.transform.position = GridToWorld(item.Data.PivotPos);
        }
        else
        {
            item.transform.parent = GridViews.FirstOrDefault(g => g.Data.PivotPos == item.Data.PivotPos)?.transform;
            item.transform.localPosition = new Vector3(0.5f, 0.5f) * Const.World.GridSize;
        }
    }
    void RemoveItem(GamePlaying.MyItem item)
    {
        ItemView? ins = itemViewList.FirstOrDefault(s => s.Data == item);
        if (ins == null)
        {
            MyDebug.LogError($"没有找到物体 {item} 对应的View.");
            return;
        }
        if(item.Config.IsEvent)
            RemoveItemEvt(item);
        
        Destroy(ins.gameObject);
        itemViewList.Remove(ins);
        
        if(item.Config.IsGrid)
            RefreshGridEdge();
    }
    #endregion
    
    #region Add/Remove ItemEvt

    void RefreshItemEvt()
    {
        ClearAllItemEvt();
        if (Data.GetSpinOptional() is MySome<PlaySpin> { Value.IsWaitClickNextTurn: true })
            return;
        itemViewList
            .Where(itemView => itemView.Data.Config.IsEvent && itemView.Data.IsBuildingOrEventKanSei)
            .ForEach(SpawnItemEvt);
    }
    void ClearAllItemEvt()
    {
        itemEvtViewList.ForEach(itemEvtView => Destroy(itemEvtView.gameObject));
        itemEvtViewList.Clear();
    }
    void SpawnItemEvt(ItemView itemView)
    {
        ItemEvtView itemEvtView = Instantiate(pfbItemEvtView, trsItemEvtView);
        itemEvtView.OnCreateView(itemView);
        itemEvtView.SetActiveTrue();
        itemEvtViewList.Add(itemEvtView);
    }

    void RemoveItemEvt(GamePlaying.MyItem item)
    {
        ItemEvtView? ins = itemEvtViewList.FirstOrDefault(s => s.BelongView.Data == item);
        if (ins == null)
        {
            // MyDebug.LogError($"没有找到物体事件 {item} 对应的View.");
            return;
        }
        Destroy(ins.gameObject);
        itemEvtViewList.Remove(ins);
    }
    #endregion
    
    #region Fog

    void RefreshConfinerAndFog()
    {
        // colliderConfiner.size
        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        Data.GridPoses.ForEach(pos =>
        {
            if (pos.X < minX)
                minX = pos.X;
            if (pos.X > maxX)
                maxX = pos.X;
            if (pos.Y < minY)
                minY = pos.Y;
            if (pos.Y > maxY)
                maxY = pos.Y;
        });
        var width = maxX - minX + 4.2f;
        var height = maxY - minY + 4.2f;
        colliderConfiner.size = new(width, height);
        colliderConfiner.offset = new Vector2(minX + maxX / 2f, minY + maxY / 2f);
        GridViews.ForEach(itemView =>
        {
            itemView.RefreshFog();
        });
    }
    #endregion
    
    #region Screen/World/Grid pos transfrom
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    public static Vector2 GridToScreen(Vector2Int gridPos) => MyCamera.Main.WorldToScreenPoint(GridToWorld(gridPos));
    public static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    public static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
    #endregion
}

internal static class IntExt
{
    extension(long self)
    {
        public string ToStringWithSymbol()
        {
            // if(ignoreZero && self == 0)
                // return string.Empty;
            string symbol = self switch
            {
                > 0 => "+",
                // < 0 => "-",
                _ => string.Empty
            };
            return $"{symbol}{self}";
        }
    }
}
internal struct Edge
{
    public required Vector3 Point1;
    public required Vector3 Point2;
}
internal static class EdgeExt
{
    extension(List<Edge> edges)
    {
        internal List<List<Vector3>> ConnectToLines() 
        {
            List<List<Vector3>> allPaths =[];
            List<Edge> remainingEdges = [..edges];
            while (remainingEdges.Count > 0)
            {
                LinkedList<Vector3> currentPath = new();
                
                Edge firstEdge = remainingEdges[0];
                remainingEdges.RemoveAt(0);
                
                currentPath.AddLast(firstEdge.Point1);
                currentPath.AddLast(firstEdge.Point2);

                bool isGrowing = true;
                while (isGrowing)
                {
                    isGrowing = false;
                    for (int i = remainingEdges.Count - 1; i >= 0; i--)
                    {
                        Edge edge = remainingEdges[i];
                        Vector3 head = currentPath.First.Value;
                        Vector3 tail = currentPath.Last.Value;

                        if (ArePointsEqual(edge.Point1, tail))
                        {
                            currentPath.AddLast(edge.Point2);
                            remainingEdges.RemoveAt(i);
                            isGrowing = true;
                        }
                        else if (ArePointsEqual(edge.Point2, tail))
                        {
                            currentPath.AddLast(edge.Point1);
                            remainingEdges.RemoveAt(i);
                            isGrowing = true;
                        }
                        else if (ArePointsEqual(edge.Point1, head))
                        {
                            currentPath.AddFirst(edge.Point2);
                            remainingEdges.RemoveAt(i);
                            isGrowing = true;
                        }
                        else if (ArePointsEqual(edge.Point2, head))
                        {
                            currentPath.AddFirst(edge.Point1);
                            remainingEdges.RemoveAt(i);
                            isGrowing = true;
                        }
                    }
                }
                allPaths.Add([..currentPath]);
            }
            return allPaths;
        }   
    }
    internal static bool ArePointsEqual(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude < 0.0001f;
    }
}