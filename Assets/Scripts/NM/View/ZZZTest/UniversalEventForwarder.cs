// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
//
// namespace NM.View;
//
// public class UniversalEventForwarder : MonoBehaviour,
//     IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler,
//     IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
//     IBeginDragHandler, IDragHandler, IEndDragHandler,
//     IDropHandler, IScrollHandler
// {
//     public void OnPointerEnter(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.pointerEnterHandler);
//     public void OnPointerExit(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.pointerExitHandler);
//     public void OnPointerMove(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.pointerMoveHandler);
//     public void OnPointerDown(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.pointerDownHandler);
//     public void OnPointerUp(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.pointerUpHandler);
//     public void OnPointerClick(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.pointerClickHandler);
//     public void OnBeginDrag(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.beginDragHandler);
//     public void OnDrag(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.dragHandler);
//     public void OnEndDrag(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.endDragHandler);
//     public void OnDrop(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.dropHandler);
//     public void OnScroll(PointerEventData eventData) => PassEvent(eventData, ExecuteEvents.scrollHandler);
//     void PassEvent<T>(PointerEventData eventData, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
//     {
//         List<RaycastResult> resultList = [];
//         EventSystem.current.RaycastAll(eventData, resultList);
//         int selfIndex = resultList.FindIndex(r => r.gameObject == gameObject);
//         if (selfIndex < 0) return;
//         var count = resultList.Count;
//         for (int i = selfIndex + 1; i < count; i++)
//         {
//             ExecuteEvents.Execute(resultList[i].gameObject, eventData, function);
//             // if ()
//             // {
//             //     MyDebug.Log($"事件{eventData.GetType().GetNiceName()} → {resultList[i].gameObject.name}");
//             //     break;
//             // }
//         }
//     }
// }