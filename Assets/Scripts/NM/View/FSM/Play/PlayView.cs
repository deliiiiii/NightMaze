using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using NM.ViewEvt;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace NM.View;
public class PlayView : ViewBase<GamePlaying>
{
    [Header("上左")]
    public List<PropValueView> PropValueViewList = [];
    
    [Header("上中")]
    public Txt TxtLayerCount;
    public Txt TxtTurnCount;
    public Txt TxtNextSoftDdl;
    [Header("上右")]
    public Txt TxtLoyalty;
    public Txt TxtHostility;
    
    [Header("中")]
    public GridDetail GridDetail;
    public InstantInfoView InstantInfoView;
    
    [Header("Trs")]
    public Trs GridTrs;
    
    [Header("Btn")]
    public Btn BtnSpin;
    public Btn BtnNextTurn;
    public Btn BtnSave;
    public Btn BtnExit;
    public Btn BtnSetting;

    [Header("Pfb")]
    [SerializeField] ItemView itemPfb;

    readonly List<ItemView> itemViewList = [];
    IEnumerable<ItemView> Grids => itemViewList.Where(i => i.Data.Config.IsGrid);
    
    public Vector2Int? LockedPosDetail;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSpin.onClick.EvtBindTo(() => new EvtPlayViewClickSpin().Forget());
        yield return BtnNextTurn.onClick.EvtBindTo(() => new EvtPlayViewClickNextTurn().Forget());
        yield return BtnSave.onClick.EvtBindTo(() => Saver.SaveAsync(Const.Name.Save.SlotFolder, Data.PlayerName, Data));
        yield return BtnExit.onClick.EvtBindTo(() => new EvtPlayViewClickExit().Forget());
        yield return BtnSetting.onClick.EvtBindTo(() => SettingViewIns.SetActiveTrue());
    }
    void Update()
    {
        if (LockedPosDetail != null)
        {
            ShowGridDetailAtPos(LockedPosDetail.Value);
        }
        BtnSpin.interactable = (
            from play in GamePlayData
            select play.IsState<PlayIdle>()) | false;
        BtnNextTurn.interactable = (
            from spin in PlaySpinData
            select spin.CanHarvest) | false;
    }

    #region OnEvt
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = evt.WhoHasCt;
            // ClearAllGrid();
            Data.Items.ForEach(SpawnItem);
            RefreshTurnAndSoOn(Data);
            PropValueViewList.ForEach(view => view.Refresh(Data));
            gameObject.SetActiveTrue();
            return UniTask.CompletedTask;
        },
        Des = "(进入Root - Playing状态时) 恢复游戏"
    };

    void RefreshTurnAndSoOn(GamePlaying play)
    {
        TxtTurnCount.text = play.TurnCount.ToString();
    }

    UniEvt<GamePlaying.EvtOnExit> OnExit => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = null!;
            ClearAllGrid();
            this.SetActiveFalse();
            GridDetail.SetActiveFalse();
            LockedPosDetail = null;
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };

    
    UniEvt<GamePlaying.EvtCurLayerChanged> OnCurLayerChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            TxtLayerCount.text = evt.GamePlaying.CurLayer.ToString();
            return UniTask.CompletedTask;
        },
        Des = "更新文本",
    };
    UniEvt<GamePlaying.EvtTurnCountChanged> OnTurnCountChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            TxtTurnCount.text = evt.GamePlaying.TurnCount.ToString();
            return UniTask.CompletedTask;
        },
        Des = "更新文本",
    };
    

    UniEvt<PlaySpin.EvtOnTick> OnTickSpin => new()
    {
        Invoke = (evt, ct) =>
        {
            PropValueViewList.ForEach(view => view.Refresh(Data));
            return UniTask.CompletedTask;
        },
        Des = "刷新属性显示"
    };

    UniEvt<PlayIdle.EvtOnEnter> OnEnterPlayIdle => new()
    {
        Invoke = (evt, ct) =>
        {
            PropValueViewList.ForEach(view => view.Refresh(Data));
            return UniTask.CompletedTask;
        },
        Des = "激活spin按钮"
    };
    UniEvt<PlayIdle.EvtOnExit> OnExitPlayIdle => new()
    {
        Invoke = (evt, ct) =>
        {
            return UniTask.CompletedTask;
        },
        Des = "取消激活spin按钮"
    };

    UniEvt<GamePlaying.EvtSpawnItem> OnSpawnItemAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SpawnItem(evt.Item);
            return UniTask.CompletedTask;
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
    #endregion
    
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
                    select $"->{distributeProp.ToItem?.Config.Name ?? "YOU"}{distributeProp.PropType.GetLabelText()}{distributeProp.Value.ToStringWithSymbol()}"
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
            return "建造/事件已完成.";
        return string.Join(',', item.BuildingOrEventProgress.Select(pair =>
        {
            return $"需要{pair.Key.GetLabelText()} {pair.Value}/{item.Config.BuildPropValueList.First(p => p.Key == pair.Key).Value}";
        }));
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

    void ClearAllGrid()
    {
        itemViewList.Where(item => item != null).ForEach(item => Destroy(item.gameObject));
        itemViewList.Clear();
    }

    void SpawnItem(GamePlaying.MyItem item)
    {
        ItemView ins = Instantiate(itemPfb);
        ins.Data = item;
        ins.name += $" {item.PivotPos.ToString()}";
        SetViewPosInternal(ins);
        ins.SetActiveTrue();
        ins.OnCreateView();
        itemViewList.Add(ins);
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
    }
    void SetViewPosInternal(ItemView item)
    {
        if (item.Data.Config.IsGrid)
        {
            item.transform.parent = GridTrs;
            item.transform.position = GridToWorld(item.Data.PivotPos);
        }
        else
        {
            item.transform.parent = Grids.FirstOrDefault(g => g.Data.PivotPos == item.Data.PivotPos)?.transform;
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
        Destroy(ins.gameObject);
        itemViewList.Remove(ins);
    }
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    public static Vector2 GridToScreen(Vector2Int gridPos) => MyCamera.Main.WorldToScreenPoint(GridToWorld(gridPos));
    public static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    public static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
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