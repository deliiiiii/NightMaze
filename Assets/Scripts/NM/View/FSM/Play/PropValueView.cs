using System.Linq;
using GeneralPreview;
using NM.Config;
using NM.Data;
using UnityEngine;

namespace NM.View;

public class PropValueView : ViewBase
{
    [SerializeField] Txt txtValue;
    [SerializeField] Txt txtLastValueDes;
    [SerializeField] EPropType propType;

    public void Refresh(GamePlaying play)
    {
        txtValue.text = propType switch
        {
            EPropType.Prop1 => play.PropBody.ToString(),
            EPropType.Prop2 => play.PropSans.ToString(),
            EPropType.Prop3 => play.PropLore.ToString(),
            _ => "NaN"
        };
        txtLastValueDes.text = (
            from spin in play.GetStateOptional<PlaySpin>()
            select $"({spin.Items.Sum(item => item.GetProp(propType)).ToStringWithSymbol()})"
        ) | string.Empty;
    }
}