using System;
using System.Collections.Generic;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 'required' 修饰符或声明为可以为 null。

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
    [SerializeField] Txt txtCoin;
    [SerializeField] Txt txtSymbolCount;
    [SerializeField] Btn btn;
    [SerializeField] GO goSelected;
    public event Action? OnClick;
    
    public void Init(GamePlaying fData)
    {
        Data = fData;
        txtPlayerName.text = Data.PlayerName;
        var hours = (int)(Data.PlayTime / 3600);
        txtPlayTime.text = $@"{hours}:{TimeSpan.FromSeconds(Data.PlayTime):mm\:ss\.ff}";
        // txtCoin.text = Data.Coin.ToString();
        // txtSymbolCount.text = Data.SymbolDeck.Count().ToString();
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