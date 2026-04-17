using System.Collections.Generic;
using General;
using GeneralPreview;
using NM.Data;
using UnityEngine;

namespace NM.View;

public class ItemEvtView : ViewBase
{
    [SerializeField] Btn btnConfirm;
    [SerializeField] Txt txtName;
    public ItemView BelongView { get; private set; }

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnConfirm.onClick.EvtBindTo(() =>
        {
            if (!BelongView.Data.Config.IsEvent || !BelongView.Data.IsBuildingOrEventKanSei)
                return;
            GamePlayData.MatchA(some =>
            {
                new GamePlaying.ActObtainEvt(some)
                {
                    Item = BelongView.Data
                }.Forget();
            });
        });
    }
    public void OnCreateView(ItemView fBelongView)
    {
        BelongView = fBelongView;
        txtName.text = $"{BelongView.Data.Config.Name} 已完成\n[点击领取]";
        name += $" {BelongView.Data.Config.Name}";
    }
    void Update()
    {
        if(BelongView != null)
            transform.position = BelongView.transform.position + Vector3.up * (Const.World.GridSize * 0.6f);
    }
}