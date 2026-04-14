using System;
using System.Collections.Generic;
using System.Linq;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;
// #pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。
namespace NM.View;

public class SlotView : ViewBase
{
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btn.onClick.EvtBindTo(() => OnClick?.Invoke());
    }

    public GamePlaying Data { get; private set; }
    
    [SerializeField] Txt txtPlayerName;
    [SerializeField] Txt txtPlayTime;
    [SerializeField] Txt txtProp1;
    [SerializeField] Txt txtProp2;
    [SerializeField] Txt txtProp3;
    [SerializeField] Txt txtPropA1;
    [SerializeField] Txt txtPropA2;
    [SerializeField] Txt txtItemCount;
    [SerializeField] Btn btn;
    [SerializeField] GO goSelected;
    public event Action? OnClick;
    
    public void Init(GamePlaying data)
    {
        Data = data;
        txtPlayerName.text = Data.PlayerName;
        var hours = (int)(Data.PlayTime / 3600);
        txtPlayTime.text = $@"{hours}:{TimeSpan.FromSeconds(Data.PlayTime):mm\:ss\.ff}";
        txtProp1.text = Data.PropBody.ToString();
        txtProp2.text = Data.PropSans.ToString();
        txtProp3.text = Data.PropLore.ToString();
        txtPropA1.text = Data.PropLoyalty.ToString();
        txtPropA2.text = Data.PropHostility.ToString();
        txtItemCount.text = Data.Items.Count().ToString();
    }
    public void OnSelect()
    {
        goSelected.SetActiveTrue();
    }
    public void OnUnSelect()
    {
        goSelected.SetActiveFalse();
    }
}