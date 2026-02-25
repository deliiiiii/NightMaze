// using static NM.Data.GamePlaying;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using General.BindData;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

namespace NM.View;

public class PlayView : ViewBase
{
    [SerializeField] List<SymbolColumnView> symbolColumnList = [];
    public TextMeshProUGUI TxtCoin;
    public TextMeshProUGUI TxtPayCoin;
    public Button BtnSpin;
    [SerializeField, Required] DOTweenSequence payTween;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return Binder.FromEvt(BtnSpin.onClick).To(() => Bus.FireAndForget(new GamePlaying.EvtClickSpin()));
    }

    protected override void Awake()
    {
        base.Awake();
        OnPlayEvtOnEnter.AddTo(destroyCancellationToken);
        OnPlayEvtShowSymbolAt.AddTo(destroyCancellationToken);
        
        OnSpinEvtOnEnter.AddTo(destroyCancellationToken);
        OnSpinEvtSpinPay.AddTo(destroyCancellationToken);
    }

    UniEvt<GamePlaying.EvtOnEnter> OnPlayEvtOnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            SetAllEmpty(evt.Ctx);
            return UniTask.CompletedTask;
        },
        Des = "(进入Root - Playing状态时) 清空所有格子"
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
    UniEvt<PlayingSpin.EvtOnEnter> OnSpinEvtOnEnter => new()
    {
        Invoke = (evt, ct) =>
        {
            SetAllEmpty(evt.Ctx.BelongFSM);
            return UniTask.CompletedTask;
        },
        Des = "(进入Playing - Spin时) 清空所有格子"
    };
    UniEvt<PlayingSpin.EvtPay> OnSpinEvtSpinPay => new()
    {
        Invoke = async (evt, ct) =>
        {
            foreach (var symbolColumn in symbolColumnList)
            {
                foreach (var symbolView in symbolColumn.SymbolList)
                {
                    var ultimateGive = symbolView.SymbolEtt.GetUltimateGive();
                    if (symbolView.SymbolEtt == evt.Symbol && ultimateGive > 0)
                    {
                        payTween[0].FromValue = symbolView.transform.position;
                        TxtPayCoin.text = ultimateGive.ToString();
                        await payTween.PlayAsync(ct);
                    }
                }
            }
        },
        Des = "(某符号结算时) 播放结算动画"
    };


    void SetAllEmpty(GamePlaying ctx)
    {
        _ = from x in Enumerable.Range(Const.SpinFirstID, Const.SpinW)
            from y in Enumerable.Range(Const.SpinFirstID, Const.SpinH)
            select SetSymbolAt(SymbolEtt.CreateEmptySymbol(), new Vector2Int(x, y));
    }

    ValueTuple? SetSymbolAt(SymbolEtt symbolEtt, Vector2Int pos)
    {
        var symbolView = symbolColumnList[pos.X - 1].SymbolList[pos.Y - 1];
        symbolView.SymbolEtt = symbolEtt;
        return null;
    }
}