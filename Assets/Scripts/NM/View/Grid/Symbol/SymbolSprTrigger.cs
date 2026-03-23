using System;
using System.Threading;
using General;
using GeneralPreview;
using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class SymbolSprTrigger : MonoBehaviour, ILeaf<SymbolView>, IMultiBeginDragHandler, IMultiDragHandler, IMultiEndDragHandler
{
    public CancellationToken CurCt => destroyCancellationToken;
    [field:SerializeReference] public SymbolView BelongData { get; set; } = null!;
    Vector3? initPos;

    void Awake()
    {
        // this.ForwardAllEvt();
        // this.DisableRayWhenDrag();
        // this.BindEvtPntTrg(EventTriggerType.BeginDrag, evt =>
        // {
        //     
        // });
        // this.BindEvtPntTrg(EventTriggerType.Drag, evt =>
        // {
        //    
        // });
        // this.BindEvtPntTrg(EventTriggerType.EndDrag, evt =>
        // {
        //     
        // });
    }

    public void OnMultiBeginDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        initPos = transform.position;
    }

    public void OnMultiDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        var tarPos = MyCamera.Main.ScreenToWorldPoint(eventData.position);
        transform.SetPositionXY(tarPos); 
    }

    public void OnMultiEndDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        transform.SetPositionXY(initPos!.Value);
    }
}