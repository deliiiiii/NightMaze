using UnityEngine.EventSystems;

namespace NM.View;

public interface IMultiPointerEnterHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiPointerEnter(PointerEventData eventData);
}

public interface IMultiPointerExitHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiPointerExit(PointerEventData eventData);
}

public interface IMultiPointerHoverHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiPointerHover(PointerEventData eventData);
}

public interface IMultiBeginDragHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiBeginDrag(PointerEventData eventData);
}

public interface IMultiDragHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiDrag(PointerEventData eventData);
}

public interface IMultiEndDragHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiEndDrag(PointerEventData eventData);
}

public interface IMultiPointerClickHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiPointerClick(PointerEventData eventData);
}

public interface IMultiScrollHandler : IEventSystemHandler, IPointerPena
{
    void OnMultiScroll(PointerEventData eventData);
}

public interface IPointerPena
{
    public virtual bool EnablePena => true;
}
