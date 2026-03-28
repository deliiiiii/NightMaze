using System.Threading;
using General;
using GeneralPreview;
using NM.Data;
using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class SymbolSprTrigger : MonoBehaviour, IMultiBeginDragHandler, IMultiDragHandler, IMultiEndDragHandler,
    IMultiPointerEnterHandler, IMultiPointerExitHandler
{
    [field:SerializeReference] public SymbolView BelongView { get; set; } = null!;
    Vector2 initThisScreenPos;
    Vector2 initThisWorldPos;
    
    public void OnMultiBeginDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        initThisScreenPos = MyCamera.Main.WorldToScreenPoint(transform.position);
        initThisWorldPos = transform.position;
    }

    public void OnMultiDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        var tarPos = MyCamera.Main.ScreenToWorldPoint(initThisScreenPos + eventData.position - eventData.pressPosition);
        transform.SetPositionXY(tarPos); 
    }

    public void OnMultiEndDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        transform.position = initThisWorldPos;
        // transform.SetPositionXY(initScreenPos);

    }

    public record EvtOnEndDrag(Data.EttSymbol EttSymbol, Vector3 WorldPos) : EvtForgetBase;

    public void OnMultiPointerEnter(PointerEventData eventData)
    {
    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {
        
    }
    
}