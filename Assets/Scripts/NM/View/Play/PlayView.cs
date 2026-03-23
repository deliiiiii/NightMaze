using System.Collections.Generic;
using System.Linq;
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

public class PlayView : ViewBase
{
    [SerializeField] List<SymbolColumnView> symbolColumnList = [];
    public Txt TxtCoin;
    public Txt TxtPayCoin;
    public Btn BtnSpin;
    public Btn BtnExit;
    [SerializeField, Required] DOTweenSequence payTween;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return BtnSpin.onClick.EvtBindTo(() => new GamePlaying.EvtClickSpin().Forget());
        yield return BtnExit.onClick.EvtBindTo(() => new GamePlaying.EvtClickExit().Forget());
    }
    
    #region OnEvt
    UniEvt<GamePlaying.EvtOnEnter> OnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            gameObject.SetActive(true);
            RefreshAll(evt.WhoHasCt);
            return UniTask.CompletedTask;
        },
        Des = "(进入Root - Playing状态时) 恢复游戏"
    };

    UniEvt<GamePlaying.EvtOnExit> OnExit => new()
    {
        Invoke = (evt, ct) =>
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        },
        Des = "(退出Root - Playing状态时) 隐藏界面"
    };
    
    
    UniEvt<GamePlaying.EvtShowSymbolAt> OnPlayEvtShowSymbolAt => new()
    {
        Invoke = (evt, ct) =>
        {
            SetSymbolAt(evt.Symbol, evt.Pos);
            return UniTask.CompletedTask;
        },
        Des = "(某符号旋转到某位置时) 在格子上显示符号"
    };
    UniEvt<GamePlaying.EvtCoinChanged> OnCoinChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            TxtCoin.text = evt.NewValue.ToString();
            return UniTask.CompletedTask;
        },
        Des = "(金币增加时) 刷新金币显示"
    };
    
    UniEvt<PlayingSpin.EvtSymbolPay> OnSpinEvtSpinPay => new()
    {
        Invoke = async (evt, ct) =>
        {
            var pay = evt.Pay;
            foreach (var symbolColumn in symbolColumnList)
            {
                foreach (var symbolView in symbolColumn.SymbolList)
                {
                    if (symbolView.Data == evt.WhoHasCt && pay > 0)
                    {
                        payTween[0].FromValue = symbolView.transform.position;
                        TxtPayCoin.text = pay.ToString();
                        await payTween.PlayAsync(ct);
                    }
                }
            }
        },
        Des = "(某符号结算时) 播放结算动画"
    };
    
    UniEvt<SymbolData.EvtPosChanged> OnSymbolEvtPosChanged => new()
    {
        Invoke = (evt, ct) =>
        {
            evt.NewValue.MatchA(none: () => evt.OldValue.MatchA(SetEmptyAt));
            return UniTask.CompletedTask;
        },
        Des = "(某符号删除Pos时) 在位置上显示空符号"
    };
    #endregion


    void RefreshAll(GamePlaying ctx)
    {
        SetAllEmpty();
        ctx.SymbolDeck.ForEach(s =>
        {
            s.Pos.MatchA(some => SetSymbolAt(s, some));
        });
        TxtCoin.text = ctx.Coin.ToString();
    }

    void SetAllEmpty()
    {
        foreach (var x in Enumerable.Range(Const.SpinFirstID, Const.SpinW))
        {
            foreach (var y in Enumerable.Range(Const.SpinFirstID, Const.SpinH))
            {
                SetEmptyAt(new Vector2Int(x, y));
            }
        }
    }
    void SetSymbolAt(SymbolData symbolData, Vector2Int pos)
    {
        var symbolView = symbolColumnList[pos.X - 1].SymbolList[pos.Y - 1];
        symbolView.Data = symbolData;
    }
    void SetEmptyAt(Vector2Int pos)
    {
        SetSymbolAt(SymbolData.CreateEmpty(), pos);
    }
}