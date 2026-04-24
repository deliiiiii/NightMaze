using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class SelectItemView : ViewBase
{
    [SerializeField] GO mainPnl;
    
    [SerializeField] Btn btnHide;
    [SerializeField] Btn btnConfirm;
    [SerializeField] Btn btnSkip;
    [SerializeField] Trs trsItemSelect;
    [SerializeField] ItemInSelectView pfbItemInSelectView;
    [ShowInInspector, ReadOnly] ItemInSelectView? CurSelect
    {
        get;
        set
        {
            field?.SetSelected(false);
            btnConfirm.interactable = value != null;
            field = value;
            field?.SetSelected(true);
        }
    }
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnHide.onClick.EvtBindTo(() =>
        {
            mainPnl.SetActiveReverse();
        });
        yield return btnConfirm.onClick.EvtBindTo(() =>
        {
            gameObject.SetActiveFalse();
            new GamePlaying.EvtClickSelectSymbol(CurSelect?.ConfigID).Forget();
        });
        yield return btnSkip.onClick.EvtBindTo(() =>
        {
            gameObject.SetActiveFalse();
            new GamePlaying.EvtClickSelectSymbol(null).Forget();
        });
    }

    UniEvt<GamePlaying.EvtStartSelectSymbol> OnStartSelectSymbol => new()
    {
        Invoke = (evt, ct) =>
        {
            trsItemSelect.ClearActiveChildren();
            evt.ToSelectIDs.ForEach(id =>
            {
                var ins = Instantiate(pfbItemInSelectView, trsItemSelect);
                ins.OnCreateView(id);
                ins.OnClick += () =>
                {
                    CurSelect = CurSelect == ins ? null : ins;
                };
            });
            CurSelect = null;
            
            gameObject.SetActiveTrue();
            mainPnl.SetActiveTrue();
            return UniTask.CompletedTask;
        },
        Des = "开始选择棋子",
    };
}