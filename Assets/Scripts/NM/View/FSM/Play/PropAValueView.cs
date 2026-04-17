using NM.Data;
using UnityEngine;

namespace NM.View;

public class PropAValueView : PropValueView
{
    // [SerializeField] Txt txtMaxValue;
    // [SerializeField] Img imgBack;
    [SerializeField] Img imgFill;
    public override void Refresh(GamePlaying play)
    {
        base.Refresh(play);
        long curValue = play.GetProp(PropType);
        long maxValue = play.GetMaxProp(PropType);
        imgFill.fillAmount = curValue * 1f / maxValue;
    }
}