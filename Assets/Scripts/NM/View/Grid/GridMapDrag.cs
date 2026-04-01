using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class GridMapDrag : MonoBehaviour, IMultiDragHandler
{
    [SerializeField] Trs follow = null!;
    ClampInRect ClampInRect => field ??= follow.GetComponent<ClampInRect>();
    
    public void OnMultiDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Right)
            return;
        var worldDelta = MyCamera.ScreenDeltaToWorldDelta(eventData.delta);
        follow.transform.position = new Vector3(
            follow.transform.position.x - worldDelta.x,
            follow.transform.position.y - worldDelta.y,
            follow.transform.position.z);
        ClampInRect.Clamp();
    }
}