using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class ResourceSpriteTrigger : MonoBehaviour,
        IMultiBeginDragHandler,
        IMultiDragHandler,
        IMultiEndDragHandler,
        IMultiPointerEnterHandler,
        IMultiPointerExitHandler
{
    [field:SerializeReference] public ResourceView BelongView { get; set; }
    public void OnMultiBeginDrag(PointerEventData eventData)
    {

    }

    public void OnMultiDrag(PointerEventData eventData)
    {

    }

    public void OnMultiEndDrag(PointerEventData eventData)
    {

    }

    public void OnMultiPointerEnter(PointerEventData eventData)
    {

    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {

    }
}