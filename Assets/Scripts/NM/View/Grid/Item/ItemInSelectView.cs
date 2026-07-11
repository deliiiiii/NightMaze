using System;
using System.Collections.Generic;
using General;
using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View;

public class ItemInSelectView : ViewBase
{
    public long ConfigID { get; private set; }
    // [ShowInInspector, ReadOnly]public ItemConfig Config { get; private set; }
    [SerializeField] Btn btn;
    [SerializeField] GO goSelected;

    [SerializeField] Txt txtID;
    public event Action? OnClick;

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btn.onClick.EvtBindTo(() => OnClick?.Invoke());
    }

    public void OnCreateView(long id)
    {
        ConfigID = id;
        txtID.text = id.ToString();
        
        gameObject.SetActiveTrue();
    }

    public void SetSelected(bool enable)
    {
        goSelected.SetActive(enable);
    }
}