using Cysharp.Threading.Tasks;
using General;
using NM.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class ItemSprTrigger : MonoBehaviour, IMultiBeginDragHandler, IMultiDragHandler, IMultiEndDragHandler
{
    [field:SerializeReference] public ItemView BelongView { get; set; }
    Vector2 initThisScreenPos;
    Vector2 initThisWorldPos;

    bool CheckDrag(PointerEventData eventData, bool isBeginDrag, out string? failInfo)
    {
        failInfo = null;
        if (eventData.button != PointerEventData.InputButton.Left)
            return false;
        if (!BelongView.Data.Config.CanDrag)
        {
            failInfo = isBeginDrag ? $"物体{BelongView.Data.Config.Name}不可拖动" : null;
            return false;
        }
        var inSpin = PlaySpinData.HasValue;
        if (inSpin)
        {
            failInfo = isBeginDrag ? $"正在结算/未收获, 不能拖动物体." : null;
            return false;
        }
        return true;
    }
    
    public void OnMultiBeginDrag(PointerEventData eventData)
    {
        if (!CheckDrag(eventData, true, out var failInfo))
        {
            if(failInfo != null)
                PlayViewIns.InstantInfoView.ShowAsync(failInfo).Forget();
            return;
        }
        initThisScreenPos = MyCamera.Main.WorldToScreenPoint(transform.position);
        initThisWorldPos = transform.position;
    }

    public void OnMultiDrag(PointerEventData eventData)
    {
        if (!CheckDrag(eventData, true, out _))
        {
            return;
        }
        // MyDebug.Log(mouseGridPos);
        var tarPos = MyCamera.Main.ScreenToWorldPoint(initThisScreenPos + eventData.position - eventData.pressPosition);
        transform.SetPositionXY(tarPos); 
    }

    public void OnMultiEndDrag(PointerEventData eventData)
    {
        if (!CheckDrag(eventData, true, out _))
        {
            return;
        }
        var tarPos = PlayView.WorldToGrid(transform.position);
        // var mouseGridPos = PlayView.ScreenToGrid(eventData.position);
        GamePlayData.MatchA(some =>
        {
            new GamePlaying.ActMoveItemToPos(some)
            {
                OldPos = BelongView.Data.PivotPos,
                Pos = tarPos,
                Item = BelongView.Data,
                ResultWrap = null!
            }.Forget();
        });
       
        transform.localPosition = Vector3.zero;
    }
}