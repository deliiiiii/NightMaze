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
    public Txt TxtTurnCount;
    public Txt TxtNextSoftDdl;
    [Header("上右")]
    public Txt TxtLoyalty;
    public Txt TxtHostility;
    
    [Header("中")]
    public GridDetail GridDetail;
    
    [Header("Trs")]
    public Trs GridTrs;
    
    [Header("Btn")]
    public Btn BtnSpin;
    public Btn BtnHarvest;
    public Btn BtnSave;
    public Btn BtnExit;

    [Header("Pfb")]
    [SerializeField] ItemView itemPfb;

    readonly List<ItemView> itemViewList = [];
    IEnumerable<ItemView> Grids => itemViewList.Where(i => i.Data.Config.IsGrid);
    
    public Vector2Int? LockedPosDetail;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSpin.onClick.EvtBindTo(() => new EvtPlayViewClickSpin().Forget());
        yield return BtnHarvest.onClick.EvtBindTo(() => new EvtPlayViewClickHarvest().Forget());
        yield return BtnSave.onClick.EvtBindTo(() => Saver.SaveAsync(Const.SaveName.SlotFolder, Data.PlayerName, Data));
        yield return BtnExit.onClick.EvtBindTo(() => new EvtPlayViewClickExit().Forget());
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
        BtnHarvest.interactable = (
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
            PropValueViewList.ForEach(view => view.Refresh(Data));
            gameObject.SetActiveTrue();
            return UniTask.CompletedTask;
        },
        Des = "(进入Root - Playing状态时) 恢复游戏"
    };

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
            orderby (int)item.ItemType + (item.ItemType == EItemType.Grid ? int.MaxValue/2 : 0)
            // from config in (List<IItemConfig>)[item.Config, .. item.EatConfigs]
            select new DetailInfo
            {
                Type = item.Config.PrefixName,
                Name = item.Config.Name,
                TagInfoList = item.Config.DetailTagInfos,
                Detail = $"""
                          {item.PivotPos}{(!Data.Items.Contains(item) ? "已不复存在" : string.Empty)}{ResolveItemDesList(item.Config.DesList)}{ResolveItemDesList(item.EatConfigList)}
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
                           (Math.Abs(modProp.MultiValue - 1) > 1e-5 ? $"<color=green>x{modProp.MultiValue}</color>" : string.Empty)
                ]
            }
        ];
        GridDetail.SetActiveTrue();
        GridDetail.transform.position = GridToWorld(gridPos + new Vector2Int(1,1) * Const.GridSize);
        GridDetail.transform.SetLocalPositionZ(0);
        GridDetail.Refresh(detailList);
    }
    string ResolveItemDesList(List<ItemDesConfig> desConfigList)
    {
        var ret = string.Join("\n", desConfigList.Select(ResolveItemDes));
        if (ret != string.Empty)
            return $"\n{ret}<sprite name=\"GridBack\">";
        return string.Empty;
    }
    string ResolveItemDes(ItemDesConfig desConfig)
    {
        var sb = new StringBuilder();
        var result = desConfig.Result;
        bool isFirst = true;
        while (result != null)
        {
            if(!isFirst)
                sb.Append("<color=red> & </color>");
            isFirst = false;
            sb.Append(result.GetType().GetCustomAttribute<TypeRegistryItemAttribute>()?.Name ?? result.GetType().Name);
            result = result.Next;
        }
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
        itemViewList.ForEach(item => Destroy(item.gameObject));
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
            item.transform.parent = Grids.FirstOrDefault(g => g.Data.PivotPos == item.Data.PivotPos)?.TrsOnGrid;
            item.transform.localPosition = Vector3.zero;
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
    static Vector2 GridToScreen(Vector2Int gridPos) => MyCamera.Main.WorldToScreenPoint(GridToWorld(gridPos));
    static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
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