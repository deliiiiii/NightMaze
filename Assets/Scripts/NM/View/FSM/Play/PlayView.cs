using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Vector2Int = GeneralPreview.Vector2Int;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;
public class PlayView : ViewBase<GamePlaying>
{
    public Trs GridTrs;
    public Txt TxtCoin;
    public Txt TxtPayCoin;
    public Btn BtnSpin;
    public Btn BtnExit;

    [SerializeField] GridView gridPfb;
    [SerializeField] SymbolView symbolPfb;

    readonly Dictionary<Vector2Int, GridView> gridDic = [];
    
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
            ClearAllGrid();
            Data.Grids.ForEach(grid =>
            {
                var go = Instantiate(gridPfb, GridTrs);
                go.transform.localPosition = grid.Pos;
                gridDic.Add(grid.Pos, go);
            });
            
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
            gameObject.SetActiveFalse();
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };
    #endregion


    void ClearAllGrid()
    {
        gridDic.Values.ForEach(grid => Destroy(grid.gameObject));
        gridDic.Clear();
    }
}
