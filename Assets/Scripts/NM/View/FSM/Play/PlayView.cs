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
    public Trs GridTrs;
    public Btn BtnExit;

    [SerializeField] GridView gridPfb;
    [SerializeField] SymbolView symbolPfb;

    readonly List<GridView> gridList = [];
    readonly List<SymbolView> symbolList = [];
    
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
            Data.Grids.ForEach(SetGridAtPos);
            Data.Symbols.ForEach(SetSymbolAtPos);
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
        symbolList.Add(go);

        go.transform.parent = gridList.FirstOrDefault(g => g.Data.Pos == symbol.Pos)?.TrsSymbol;
        go.transform.localPosition = Vector3.zero;
    }
    public static Vector2Int ScreenToGrid(Vector2 screenPos) => WorldToGrid(MyCamera.Main.ScreenToWorldPoint(screenPos));
    static Vector2Int WorldToGrid(Vector2 worldPos) => new((int)worldPos.x, (int)worldPos.y);
    static Vector2 GridToWorld(Vector2Int gridPos) => gridPos;
}
