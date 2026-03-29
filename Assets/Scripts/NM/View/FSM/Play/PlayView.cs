using System.Collections.Generic;
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
    public Trs GridTrs;
    public Btn BtnExit;

    [SerializeField] GridView gridPfb;
    [SerializeField] SymbolView symbolPfb;

    readonly Dictionary<Vector2Int, GridView> gridDic = [];
    readonly Dictionary<Vector2Int, SymbolView> symbolDic = [];
    
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnExit.onClick.EvtBindTo(() => new EvtPlayViewClickExit().Forget());
    }
    
    #region OnEvt
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            Data = evt.WhoHasCt;
            // ClearAllGrid();
            Data.Grids.ForEach(grid => SetGridAtPos(grid, grid.Pos));
            Data.Symbols.ForEach(symbol => SetSymbolAtPos(symbol, symbol.Pos));
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
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };

    UniEvt<GamePlaying.EvtSetGridAtPos> OnSetGridAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetGridAtPos(evt.Grid, evt.Pos);
            return UniTask.CompletedTask;
        },
        Des = "显示地块",
    };
    UniEvt<GamePlaying.EvtSetSymbolAtPosWithOld> OnSetSymbolAtPos => new()
    {
        Invoke = (evt, ct) =>
        {
            SetSymbolAtPos(evt.Symbol, evt.NewPos, evt.OldPos);
            return UniTask.CompletedTask;
        },
        Des = "显示符号",
    };
    #endregion


    void ClearAllGrid()
    {
        gridDic.Values.ForEach(grid => Destroy(grid.gameObject));
        gridDic.Clear();
        symbolDic.Clear();
    }

    void SetGridAtPos(GamePlaying.Grid grid, Vector2Int pos)
    {
        gridDic.Remove(pos);
        var go = Instantiate(gridPfb, GridTrs);
        go.Data = grid;
        go.transform.position = GridToWorld(pos);
        go.SetActiveTrue();
        gridDic.Add(pos, go);
    }

    void SetSymbolAtPos(GamePlaying.Symbol symbol, Vector2Int pos, Vector2Int? oldPos = null)
    {
        if (oldPos == null || !symbolDic.Remove(oldPos.Value, out var go))
        {
            go = Instantiate(symbolPfb);
            go.Data = symbol;
            go.SetActiveTrue();
            go.Sr.SetActiveTrue();
        }
        symbolDic.Add(pos, go);

        go.transform.parent = gridDic[pos].TrsSymbol;
        go.transform.localPosition = Vector3.zero;
    }
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
}
