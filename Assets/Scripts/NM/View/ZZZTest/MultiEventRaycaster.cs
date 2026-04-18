using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class MultiEventRaycaster : MonoBehaviour
{
    HashSet<GO> hoveredObjects = [];
    PointerEventData hoverEventData;
    readonly List<RaycastResult> results = new(64);

    PointerEventData[] dragEventDatas;
    HashSet<GO>[] draggedObjects;
    Vector2 lastMousePosition;

    void Start()
    {
        hoverEventData = new PointerEventData(EventSystem.current);
        
        dragEventDatas = new PointerEventData[3];
        draggedObjects = new HashSet<GO>[3];
        for (int i = 0; i < 3; i++)
        {
            dragEventDatas[i] = new PointerEventData(EventSystem.current)
            {
                button = (PointerEventData.InputButton)i
            };
            draggedObjects[i] = [];
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        Vector2 currentMousePosition = Input.mousePosition;
        Vector2 delta = currentMousePosition - lastMousePosition;
        lastMousePosition = currentMousePosition;

        hoverEventData.position = currentMousePosition;
        EventSystem.current.RaycastAll(hoverEventData, results);
        
        hoveredObjects.RemoveWhere(obj => obj == null);

        HashSet<GO> currentHits = [];
        foreach (RaycastResult result in results)
        {
            currentHits.Add(result.gameObject);
            var iPointerPena = result.gameObject.GetComponent<IPointerPena>();
            if (iPointerPena is not { EnablePena: true })
            {
                break;
            }
        }

        var exits = hoveredObjects.Except(currentHits);
        foreach (GO obj in exits)
        {
            hoverEventData.pointerEnter = obj;
            ExecuteEvents.Execute<IMultiPointerExitHandler>(obj, hoverEventData, 
                (handler, data) => handler.OnMultiPointerExit((PointerEventData)data));
        }

        var enters = currentHits.Except(hoveredObjects);
        foreach (GO obj in enters)
        {
            hoverEventData.pointerEnter = obj;
            ExecuteEvents.Execute<IMultiPointerEnterHandler>(obj, hoverEventData, 
                (handler, data) => handler.OnMultiPointerEnter((PointerEventData)data));
        }

        hoveredObjects = currentHits;
        foreach (GO obj in hoveredObjects)
        {
            ExecuteEvents.Execute<IMultiPointerHoverHandler>(obj, hoverEventData, 
                (handler, data) => handler.OnMultiPointerHover((PointerEventData)data));
        }

        for (int i = 0; i < 3; i++)
        {
            draggedObjects[i].RemoveWhere(obj => obj == null);
            
            PointerEventData eventData = dragEventDatas[i];
            eventData.position = currentMousePosition;
            eventData.delta = delta;

            if (Input.GetMouseButtonDown(i))
            {
                eventData.pressPosition = currentMousePosition;
                eventData.dragging = false;
                draggedObjects[i] = new HashSet<GameObject>(currentHits);
            }
            else if (Input.GetMouseButton(i) && draggedObjects[i].Count > 0)
            {
                float dragDistance = (currentMousePosition - eventData.pressPosition).magnitude;
                if (!eventData.dragging && dragDistance >= EventSystem.current.pixelDragThreshold)
                {
                    eventData.dragging = true;
                    foreach (var obj in draggedObjects[i])
                    {
                        eventData.pointerDrag = obj;
                        ExecuteEvents.Execute<IMultiBeginDragHandler>(obj, eventData, 
                            (handler, data) => handler.OnMultiBeginDrag((PointerEventData)data));
                    }
                }
                if (eventData.dragging)
                {
                    foreach (var obj in draggedObjects[i])
                    {
                        eventData.pointerDrag = obj;
                        ExecuteEvents.Execute<IMultiDragHandler>(obj, eventData, 
                            (handler, data) => handler.OnMultiDrag((PointerEventData)data));
                    }
                }
            }
            else if (Input.GetMouseButtonUp(i) && draggedObjects[i].Count > 0)
            {
                if (eventData.dragging)
                {
                    foreach (var obj in draggedObjects[i])
                    {
                        ExecuteEvents.Execute<IMultiEndDragHandler>(obj, eventData, 
                            (handler, data) => handler.OnMultiEndDrag((PointerEventData)data));
                    }
                }
                eventData.dragging = false;
                foreach (var obj in draggedObjects[i])
                {
                    if (currentHits.Contains(obj))
                    {
                        ExecuteEvents.Execute<IMultiPointerClickHandler>(obj, eventData, 
                            (handler, data) => handler.OnMultiPointerClick((PointerEventData)data));
                    }
                }
                draggedObjects[i].Clear();
            }
            
            Vector2 scrollDelta = Input.mouseScrollDelta;
            if (scrollDelta != Vector2.zero)
            {
                hoverEventData.scrollDelta = scrollDelta;
                foreach (GO obj in currentHits)
                {
                    ExecuteEvents.Execute<IMultiScrollHandler>(obj, hoverEventData,
                        (handler, data) => handler.OnMultiScroll((PointerEventData)data));
                }
            }
        }
    }
}