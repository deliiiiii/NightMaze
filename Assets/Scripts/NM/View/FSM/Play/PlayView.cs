using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using NM.ViewEvt;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

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

    readonly List<GridView> gridList = [];
    readonly List<SymbolView> symbolList = [];
    readonly List<ResourceView> resourceList = [];
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSpin.onClick.EvtBindTo(() => new EvtPlayViewClickSpin().Forget());
        yield return BtnHarvest.onClick.EvtBindTo(() => new EvtPlayViewClickHarvest().Forget());
        yield return BtnExit.onClick.EvtBindTo(() => new EvtPlayViewClickExit().Forget());
    }
    
    #region OnEvt
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = evt.WhoHasCt;
            // ClearAllGrid();
            Data.Grids.ForEach(SetGridAtPos);
            Data.Symbols.ForEach(SetSymbolAtPos);
            Data.Resources.ForEach(SetResourceAtPos);
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
            lockGridDetail = false;
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };

    UniEvt<GamePlaying.EvtSetGridAtPos> OnSetGridAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetGridAtPos(evt.Grid);
            return UniTask.CompletedTask;
        },
        Des = "显示地块",
    };
    UniEvt<GamePlaying.EvtSetSymbolAtPos> OnSetSymbolAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetSymbolAtPos(evt.Symbol);
            return UniTask.CompletedTask;
        },
        Des = "显示符号",
    };
    #endregion


    public void ShowGridDetailAtPos(Vector2Int gridPos)
    {
        if (lockGridDetail)
            return;
        // var gridPos = ScreenToGrid(screenPos);
        var detailList = new List<DetailInfo>();
        detailList.AddRange(
        [
            .. 
            from grid in Data.Grids
            where grid.Pos == gridPos
            select new DetailInfo
            {
                Type = "地块",
                Name = grid.Config.Name,
                ItemTypeList = grid.Config.Type.ToValues(),
                Detail = $"DDD...Nothing but pos {grid.Pos.ToString()}",
                InSpinLineList = []
            }, 
            ..
            from symbol in Data.Symbols
            where symbol.CoverPos(gridPos)
            select new DetailInfo
            {
                Type = "符号",
                Name = symbol.Config.Name,
                ItemTypeList = symbol.Config.Type.ToValues(),
                Detail = $"SSS...",
                InSpinLineList = []
            },
            ..
            from resource in Data.Resources
            where resource.Pos == gridPos
            select new DetailInfo
            {
                Type = "资源",
                Name = resource.Config.Name,
                ItemTypeList = resource.Config.Type.ToValues(),
                Detail = $"RRResource...",
                InSpinLineList = []
            }
        ]);
        GridDetail.SetActiveTrue();
        GridDetail.transform.position = GridToWorld(gridPos + new Vector2Int(1,1) * Const.GridSize);
        GridDetail.transform.SetLocalPositionZ(0);
        GridDetail.Refresh(detailList);
    }

    public void HideGridDetail()
    {
        if (lockGridDetail)
            return;
        GridDetail.SetActiveFalse();
    }

    bool lockGridDetail;
    public void SwitchLockGridDetail(bool? tar = null)
    {
        if (tar == null)
            lockGridDetail = !lockGridDetail;
        else
            lockGridDetail = tar.Value;
    }

    void ClearAllGrid()
    {
        gridList.ForEach(grid => Destroy(grid.gameObject));
        gridList.Clear();
        symbolList.Clear();
    }

    void SetGridAtPos(GamePlaying.Grid grid)
    {
        // gridList.Remove(pos);
        // TODO
        var go = Instantiate(gridPfb, GridTrs);
        go.Data = grid;
        go.transform.position = GridToWorld(grid.Pos);
        go.SetActiveTrue();
        gridList.Add(go);
    }

    void SetSymbolAtPos(GamePlaying.Symbol symbol)
    {
        SymbolView? go = symbolList.FirstOrDefault(s => s.Data == symbol);
        if (go == null)
        {
            go = Instantiate(symbolPfb);
            go.Data = symbol;
            go.SetActiveTrue();
            go.Sr.SetActiveTrue();
        }
        go.transform.parent = gridList.FirstOrDefault(g => g.Data.Pos == symbol.PivotPos)?.TrsSymbol;
        go.transform.localPosition = Vector3.zero;
        
        symbolList.Add(go);
    }

    void SetResourceAtPos(GamePlaying.Resource resource)
    {
        var go = Instantiate(resourcePfb, 
            gridList.FirstOrDefault(g => g.Data.Pos == resource.Pos)?.TrsResource, true);
        go.transform.localPosition = Vector3.zero;
        go.Data = resource;
        go.SetActiveTrue();
        resourceList.Add(go);
    }
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    static Vector2 GridToScreen(Vector2Int gridPos) => MyCamera.Main.WorldToScreenPoint(GridToWorld(gridPos));
    static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
}
