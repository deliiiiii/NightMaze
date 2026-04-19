using GeneralPreview;
using NM.Config;
using NM.Data;
using UnityEngine;

namespace NM.View;

public class PropValueView : ViewBase
{
    [SerializeField] protected Txt TxtValue;
    [SerializeField] protected Txt TxtLastValueDes;
    [SerializeField] protected EPropType PropType;

    public virtual void Refresh(GamePlaying play)
    {
        TxtValue.text = play.GetProp(PropType).ToString();
        TxtLastValueDes.text = (
            from spin in play.GetSpinOptional()
            select $"({spin.GetDeltaPropValue(PropType).ToStringWithSymbol()})"
        ) | string.Empty;
    }
}