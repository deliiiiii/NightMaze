using Cysharp.Threading.Tasks;
using General;
using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class GridSprTrigger : MonoBehaviour, IMultiPointerEnterHandler, IMultiPointerExitHandler
{
    [SerializeField] DOTweenSequence onEnterTween = null!;
    [SerializeField] DOTweenSequence onExitTween = null!;
    readonly DoTweenSeqMutex enterExitTween = new();
    public void OnMultiPointerEnter(PointerEventData eventData)
    {
        // MyDebug.Log($"{name} Pointer Entered! EnterGO {eventData.pointerEnter.name}"); 
        enterExitTween.PlayMutexAsync(onEnterTween, destroyCancellationToken).Forget();
    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {
        // MyDebug.Log($"{name} Pointer exit!");
        enterExitTween.PlayMutexAsync(onExitTween, destroyCancellationToken).Forget();
    }
}