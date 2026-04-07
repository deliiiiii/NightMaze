using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Config;
using NM.Data;
using NM.ViewEvt;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace NM.View;
public class PlayView : ViewBase<GamePlaying>
{
    public GridDetail GridDetail;
    
    public Trs GridTrs;
    public Btn BtnSpin;
    public Btn BtnHarvest;
    public Btn BtnExit;

    [SerializeField] GridView gridPfb;
    [SerializeField] SymbolView symbolPfb;
    [SerializeField] ResourceView resourcePfb;
    [SerializeField] BuildingView buildingPfb;

    readonly List<ItemViewBase> itemList = [];
    IEnumerable<GridView> Grids => itemList.OfType<GridView>();
    IEnumerable<SymbolView> Symbols => itemList.OfType<SymbolView>();
    IEnumerable<BuildingView> Buildings => itemList.OfType<BuildingView>();
    IEnumerable<ResourceView> Resources => itemList.OfType<ResourceView>();
    
    public Vector2Int? LockedPosDetail;
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSpin.onClick.EvtBindTo(() => new EvtPlayViewClickSpin().Forget());
        yield return BtnHarvest.onClick.EvtBindTo(() => new EvtPlayViewClickHarvest().Forget());
        yield return BtnExit.onClick.EvtBindTo(() => new EvtPlayViewClickExit().Forget());
    }

    void Update()
    {
        if (LockedPosDetail != null)
        {
            ShowGridDetailAtPos(LockedPosDetail.Value);
        }
    }

    #region OnEvt
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = evt.WhoHasCt;
            // ClearAllGrid();
            Data.Items.ForEach(SetItemAtPos);
            // Data.Grids.ForEach(SetGridAtPos);
            // Data.Symbols.ForEach(SetSymbolAtPos);
            // Data.Resources.ForEach(SetResourceAtPos);
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

    UniEvt<GamePlaying.EvtSetGridAtPos> OnSetGridAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetItemAtPos(evt.Grid);
            return UniTask.CompletedTask;
        },
        Des = "显示地块",
    };
    UniEvt<GamePlaying.EvtSetSymbolAtPos> OnSetSymbolAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetItemAtPos(evt.Symbol);
            return UniTask.CompletedTask;
        },
        Des = "显示符号",
    };
    #endregion
    
    public void ShowGridDetailAtPos(Vector2Int gridPos)
    {
        List<DetailInfo> detailList =
        [
            ..
            from item in Data.Items
            where item.CoverPos(gridPos)
            select new DetailInfo
            {
                Type = item.Config.PrefixName,
                Name = item.Config.Name,
                TagInfoList = item.Config.DetailTagInfos,
                // TODO 不仅仅是风味文本.
                Detail = item.Config.FlavorDes + $" {item.PivotPos}",
                InSpinLineList =
                [
                    ..
                    from spin in PlaySpinData.ToIEnumerable()
                    from itemInSpin in spin.GetItemByEtt(item.BelongEtt).ToIEnumerable()
                    from modProp1 in itemInSpin.ModifyProp1
                    select $"{item.Config.Name} {modProp1.Value.ToStringWithSymbol()}"
                ]
            }
        ];
        
        // var detailList = new List<DetailInfo>();
        // detailList.AddRange(
        // [
        //     .. 
        //     from grid in Data.Grids
        //     where grid.CoverPos(gridPos)
        //     select new DetailInfo
        //     {
        //         Type = "地块",
        //         Name = grid.Config.Name,
        //         TagInfoList = grid.Config.DetailTagInfos,
        //         Detail = $"DDD...Nothing but pos {grid.PivotPos.ToString()}",
        //         InSpinLineList = []
        //     }, 
        //     ..
        //     from symbol in Data.Symbols
        //     where symbol.CoverPos(gridPos)
        //     select new DetailInfo
        //     {
        //         Type = "符号",
        //         Name = symbol.Config.Name,
        //         TagInfoList = symbol.Config.DetailTagInfos,
        //         Detail = $"事符号. 白值{string.Join(", ",
        //             symbol.Config.Prop1.ToStringWithSymbol(),
        //             symbol.Config.Prop2.ToStringWithSymbol(),
        //             symbol.Config.Prop3.ToStringWithSymbol()
        //         )}",
        //         InSpinLineList = (
        //             from spin in PlaySpinData.ToIEnumerable()
        //             from symbolInSpin in spin[symbol.BelongEtt].ToIEnumerable()
        //             from modProp1 in symbolInSpin.ModifyProp1
        //             select $"{symbol.Config.name} {modProp1.Value.ToStringWithSymbol()}"
        //             ).ToList()
        //     },
        //     ..
        //     from resource in Data.Resources
        //     where resource.CoverPos(gridPos)
        //     select new DetailInfo
        //     {
        //         Type = "资源",
        //         Name = resource.Config.Name,
        //         TagInfoList = resource.Config.DetailTagInfos,
        //         Detail = $"RRResource...",
        //         InSpinLineList = []
        //     }
        // ]);
        GridDetail.SetActiveTrue();
        GridDetail.transform.position = GridToWorld(gridPos + new Vector2Int(1,1) * Const.GridSize);
        GridDetail.transform.SetLocalPositionZ(0);
        GridDetail.Refresh(detailList);
    }

    public void HideGridDetail()
    {
        if (LockedPosDetail != null)
            return;
        GridDetail.SetActiveFalse();
    }

    void ClearAllGrid()
    {
        itemList.ForEach(item => Destroy(item.gameObject));
        itemList.Clear();
    }

    void SetItemAtPos(GamePlaying.IItem item)
    {
        ItemViewBase pfb = item switch
        {
            GamePlaying.Grid => gridPfb,
            GamePlaying.Symbol => symbolPfb,
            GamePlaying.Resource => resourcePfb,
            GamePlaying.Building => buildingPfb,
            _ => throw new Exception($"未适配的Item类型 {item.GetType()}")
        };
        
        ItemViewBase? ins = itemList.FirstOrDefault(s => s.Data == item);
        if (ins == null)
        {
            ins = Instantiate(pfb);
            ins.Data = item;
            ins.SetActiveTrue();
            ins.OnCreateView();
        }

        if (ins is GridView)
        {
            ins.transform.parent = GridTrs;
            ins.transform.position = GridToWorld(item.PivotPos);
        }
        else
        {
            ins.transform.parent = Grids.FirstOrDefault(g => g.Data.PivotPos == item.PivotPos)?.TrsSymbol;
            ins.transform.localPosition = Vector3.zero;
        }
        itemList.Add(ins);
    }
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    static Vector2 GridToScreen(Vector2Int gridPos) => MyCamera.Main.WorldToScreenPoint(GridToWorld(gridPos));
    static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
}

internal static class IntExt
{
    extension(int self)
    {
        public string ToStringWithSymbol()
        {
            string symbol = self switch
            {
                > 0 => "+",
                < 0 => "-",
                _ => string.Empty
            };
            return $"{symbol}{self}";
        }
    }
}