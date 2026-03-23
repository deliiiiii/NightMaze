using General;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View.ZZZTest;

public class PointerEnter : MonoBehaviour, IMultiPointerEnterHandler, IMultiPointerExitHandler, IPointerPena
{
    public void OnMultiPointerEnter(PointerEventData eventData)
    {
        MyDebug.Log($"{name}OnPointerEnter");
    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {
        MyDebug.Log($"{name}OnPointerExit");
    }
}