using System.Collections.Generic;
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
    
    void Awake()
    {
        // this.ForwardAllEvt();
        // this.BindEvtPntTrg(EventTriggerType.PointerEnter, evt =>
        // {
        //     
        // });
        // this.BindEvtPntTrg(EventTriggerType.PointerExit, evt =>
        // {
        //    
        // });
    }

    public void OnMultiPointerEnter(PointerEventData eventData)
    {
        MyDebug.Log($"{name} Pointer Entered! EnterGO {eventData.pointerEnter.name}"); 
        enterExitTween.PlayMutexAsync(onEnterTween, destroyCancellationToken).Forget();
    }

    public void OnMultiPointerExit(PointerEventData eventData)
    {
        MyDebug.Log($"{name} Pointer exit!");
        enterExitTween.PlayMutexAsync(onExitTween, destroyCancellationToken).Forget();
    }
}

public static class TriggerExt
{
    static readonly List<EventTriggerType> pointerTriggerType = [
        EventTriggerType.PointerEnter,
        EventTriggerType.PointerExit,
        EventTriggerType.PointerDown,
        EventTriggerType.Drag,
        EventTriggerType.BeginDrag,
        EventTriggerType.EndDrag
    ];
    extension(MonoBehaviour self)
    {
        // public void BindEvtPntTrg(EventTriggerType type, UnityAction<PointerEventData> callback, CancellationToken? ct = null)
        // {
        //     var col = self.GetComponent<Collider2D>();
        //     if (col == null)
        //     {
        //         MyDebug.LogError($"物体{self.name} 上没有碰撞体, 不能绑定EventTrigger.");
        //         return;
        //     }
        //
        //     if (!pointerTriggerType.Contains(type))
        //     {
        //         // 注意传参是UnityAction<PointerEventData>
        //         MyDebug.LogError($"物体{self.name} 上绑定了非Pointer类型的EventTrigger, 函数不支持.");
        //     }
        //     ct ??= self.destroyCancellationToken;
        //     var trigger = self.gameObject.GetOrAddCom<EventTrigger>();
        //     var entry = new EventTrigger.Entry { eventID = type };
        //     entry.callback.AddListener(x =>
        //     {
        //         callback((PointerEventData)x);
        //         x.Use();
        //     });
        //     ct.Value.Register(() => trigger.triggers.Remove(entry));
        //     trigger.triggers.Add(entry);
        //     
        // }
        //
        // public void ForwardAllEvt()
        // {
        //     self.gameObject.GetOrAddCom<UniversalEventForwarder>();
        // }
        //
        //
        // public void DisableRayWhenDrag()
        // {
        //     self.BindEvtPntTrg(EventTriggerType.BeginDrag, evt =>
        //     {
        //         if (evt.pointerDrag != null && evt.pointerDrag != self.gameObject)
        //             return;
        //         MyDebug.Log($"{self.gameObject.name} begin drag...");
        //         initLayerDic[self.gameObject] = self.gameObject.layer;
        //         self.gameObject.layer = 2;
        //     });
        //     self.BindEvtPntTrg(EventTriggerType.EndDrag, evt =>
        //     {
        //         if (evt.pointerDrag != null && evt.pointerDrag != self.gameObject)
        //             return;
        //         MyDebug.Log($"{self.gameObject.name} end drag...");
        //         self.gameObject.layer = initLayerDic[self.gameObject];
        //     });
        // }

    }
    // static readonly Dictionary<GO, int> initLayerDic = [];
}