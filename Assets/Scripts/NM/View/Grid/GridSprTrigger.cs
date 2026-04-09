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
    [SerializeField] GridView belongView;
    [SerializeField] DOTweenSequence onEnterTween;
    [SerializeField] DOTweenSequence onExitTween;
    readonly DoTweenSeqMutex enterExitTween = new();
    bool isHovering = false;
    public void OnMultiPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        // MyDebug.Log($"{name} Pointer Entered! EnterGO {eventData.pointerEnter.name}"); 
        enterExitTween.PlayMutexAsync(onEnterTween, destroyCancellationToken).Forget();
    }
    
    public void OnMultiPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        // MyDebug.Log($"{name} Pointer exit!");
        enterExitTween.PlayMutexAsync(onExitTween, destroyCancellationToken).Forget();
        PlayViewIns.HideGridDetail();
    }

    public void OnMultiPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle)
            return;
        // MyDebug.Log($"{name} middle click!");
        if (isHovering && PlayViewIns.LockedPosDetail == null)
        {
            PlayViewIns.LockedPosDetail = belongView.Data.PivotPos;
            return;
        }
        PlayViewIns.LockedPosDetail = null;
        PlayViewIns.ShowGridDetailAtPos(belongView.Data.PivotPos);
        PlayViewIns.GridDetail.SwitchToFirst();
        PlayViewIns.LockedPosDetail = belongView.Data.PivotPos;
    }

    public void OnMultiPointerHover(PointerEventData eventData)
    {
        if (PlayViewIns.LockedPosDetail == null)
        {
            PlayViewIns.ShowGridDetailAtPos(belongView.Data.PivotPos);
            PlayViewIns.GridDetail.SwitchToFirst();
        }
    }
}