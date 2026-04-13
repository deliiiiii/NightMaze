using System.Linq;
using General;
using NM.Data;
using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class ItemSprTrigger : MonoBehaviour, IMultiBeginDragHandler, IMultiDragHandler, IMultiEndDragHandler,
    IMultiPointerEnterHandler, IMultiPointerExitHandler
{
    [field:SerializeReference] public ItemView BelongView { get; set; }
    Vector2 initThisScreenPos;
    Vector2 initThisWorldPos;
    
    public void OnMultiBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!BelongView.Data.Config.CanDrag)
            return;
        initThisScreenPos = MyCamera.Main.WorldToScreenPoint(transform.position);
        initThisWorldPos = transform.position;
    }

    public void OnMultiDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!BelongView.Data.Config.CanDrag)
            return;
        // MyDebug.Log(mouseGridPos);
        var tarPos = MyCamera.Main.ScreenToWorldPoint(initThisScreenPos + eventData.position - eventData.pressPosition);
        transform.SetPositionXY(tarPos); 
    }

    public void OnMultiEndDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left)
            return;
        if (!BelongView.Data.Config.CanDrag)
            return;
        var mouseGridPos = PlayView.ScreenToGrid(eventData.position);
        GamePlayData.MatchA(some =>
        {
            if (some.EmptyGrids.Any(grid => grid.CoverPos(mouseGridPos)))
            {
                new GamePlaying.ActMoveItemToPos(some)
                {
                    OldPos = BelongView.Data.PivotPos,
                    Pos = mouseGridPos,
                    Item = BelongView.Data,
                    ResultWrap = null!
                }.Forget();
            }
        });
       
        transform.localPosition = Vector3.zero;
    }

    public void OnMultiPointerEnter(PointerEventData eventData)
    {
    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {
        
    }
    
}