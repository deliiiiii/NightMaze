using Cysharp.Threading.Tasks;
using GeneralPreview;
using UnityEngine;

namespace NM.View;

public class GridMapDrag : ViewBase
{
    [SerializeField] Trs follow = null!;
    ClampInRect ClampInRect => field ??= follow.GetComponent<ClampInRect>();
    
    UniEvt<MyInput.EvtMouseDrag> EvtMouseDrag => new()
    {
        Des = "鼠标拖动, 移动地图",
        Invoke = (evt, ct) =>
        {
            var worldPos = MyCamera.ScreenDeltaToWorldDelta(evt.Delta);
            // MyDebug.Log($"World Delta {worldPos}");
            follow.transform.position = new Vector3(
                follow.transform.position.x - worldPos.x,
                follow.transform.position.y - worldPos.y,
                follow.transform.position.z);
            ClampInRect.Clamp();
            return UniTask.CompletedTask;
        },
    };
}