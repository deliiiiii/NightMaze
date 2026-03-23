using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View.ZZZTest;

public class MultiEventRaycaster : MonoBehaviour
{
    HashSet<GO> hoveredObjects = [];
    PointerEventData hoverEventData = null!;
    readonly List<RaycastResult> results = new(64);

    PointerEventData[] dragEventDatas = null!;
    HashSet<GO>[] draggedObjects = null!;
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
            
            if (result.gameObject.GetComponent<IPointerPena>() == null)
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

        // 独立轮询并派发3个按键的拖拽状态流
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
                
                // 捕获当前按键按下瞬间的命中集合
                draggedObjects[i] = [..currentHits];

                foreach (GO obj in draggedObjects[i])
                {
                    eventData.pointerDrag = obj;
                    ExecuteEvents.Execute<IMultiBeginDragHandler>(obj, eventData, 
                        (handler, data) => handler.OnMultiBeginDrag((PointerEventData)data));
                }
            }
            else if (Input.GetMouseButton(i) && draggedObjects[i].Count > 0)
            {
                eventData.dragging = true;
                foreach (GO obj in draggedObjects[i])
                {
                    eventData.pointerDrag = obj;
                    ExecuteEvents.Execute<IMultiDragHandler>(obj, eventData, 
                        (handler, data) => handler.OnMultiDrag((PointerEventData)data));
                }
            }
            else if (Input.GetMouseButtonUp(i) && draggedObjects[i].Count > 0)
            {
                eventData.dragging = false;
                foreach (GO obj in draggedObjects[i])
                {
                    ExecuteEvents.Execute<IMultiEndDragHandler>(obj, eventData, 
                        (handler, data) => handler.OnMultiEndDrag((PointerEventData)data));
                }

                draggedObjects[i].Clear();
            }
        }
    }
}