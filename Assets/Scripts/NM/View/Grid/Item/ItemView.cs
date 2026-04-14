using General;
using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class ItemView : ViewBase
{
    [ShowInInspector]public GamePlaying.MyItem Data { get; set; }

    
    public Trs TrsOnGrid;
    public Trs TrsInBuilding;
    
    [SerializeField]SpriteRenderer gridSr;
    [SerializeField]SpriteRenderer onGridSr;
    [SerializeField]DOTweenSequence onSpinTween;
    public void OnCreateView()
    {
        onGridSr.gameObject.SetActive(!Data.Config.IsGrid);
        gridSr.gameObject.SetActive(Data.Config.IsGrid);
    }
    
    UniEvt<PlaySpin.EvtBeforeCheckSymbolTween> OnBeforeCheckSymbol => new()
    {
        Invoke = async (evt, ct) =>
        {
            if (evt.Item != Data)
                return;
            if (GameRoot.Setting.SpinTweenSpeed is SettingData.SpinTweenSpeedImmediate)
                return;
            await onSpinTween.PlayAsync(ct);
        },
        Des = "符号结算前播放动画.",
    };
}