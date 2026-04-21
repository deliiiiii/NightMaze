using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class TechNodeBuildingPreView : ViewBase,
    IMultiPointerEnterHandler,
    IMultiPointerExitHandler
{
    [SerializeField, ReadOnly] ItemConfig belongConfig;
    [SerializeField] Img img;
    [SerializeField] GO pnlBriefDes;
    [SerializeField] Txt txtBriefDes;
    public void OnMultiPointerEnter(PointerEventData eventData)
    {
        txtBriefDes.text = belongConfig.BriefDes;
        pnlBriefDes.transform.position = transform.position;
        pnlBriefDes.SetActiveTrue();
    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {
        pnlBriefDes.SetActiveFalse();
    }

    public void OnCreateView(ItemConfig fBelongConfig)
    {
        belongConfig = fBelongConfig;
        img.sprite = ItemResLoader.Acquire(belongConfig.ID);
    }
}