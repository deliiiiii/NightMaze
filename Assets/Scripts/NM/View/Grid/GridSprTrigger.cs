using Cysharp.Threading.Tasks;
using General;
using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class GridSprTrigger : MonoBehaviour,
    IMultiPointerEnterHandler,
    IMultiPointerHoverHandler,
    IMultiPointerExitHandler,
    IMultiPointerClickHandler
{
    [SerializeField] GridView belongView = null!;
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
        PlayViewIns.HideGridDetail();
    }

    public void OnMultiPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle)
            return;
        // MyDebug.Log($"{name} middle click!");
        PlayViewIns.LockedPosDetail = null;
        PlayViewIns.ShowGridDetailAtPos(belongView.Data.Pos);
        PlayViewIns.GridDetail.SwitchToFirst();
        PlayViewIns.LockedPosDetail = belongView.Data.Pos;
    }

    public void OnMultiPointerHover(PointerEventData eventData)
    {
        if (PlayViewIns.LockedPosDetail == null)
        {
            PlayViewIns.ShowGridDetailAtPos(belongView.Data.Pos);
            PlayViewIns.GridDetail.SwitchToFirst();
        }
    }
}