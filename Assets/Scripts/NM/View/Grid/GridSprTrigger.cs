using System.Threading;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NM.View;

public class GridSprTrigger : MonoBehaviour
{
    // CancellationTokenSource cts = new();
    [SerializeField] DOTweenSequence onEnterTween = null!;
    [SerializeField] DOTweenSequence onExitTween = null!;
    readonly DoTweenSeqMutex enterExitTween = new();

    // CancellationTokenSource LinkedCts =>
        // CancellationTokenSource.CreateLinkedTokenSource(cts.Token, destroyCancellationToken);
    void Awake()
    {
        this.BindEvtTrg(EventTriggerType.PointerEnter, _ =>
        {
           // MyDebug.Log("Pointer Entered!"); 
           enterExitTween.PlayMutexAsync(onEnterTween, destroyCancellationToken).Forget();
        });
        this.BindEvtTrg(EventTriggerType.PointerExit, _ =>
        {
            // MyDebug.Log("Pointer exit!");
            enterExitTween.PlayMutexAsync(onExitTween, destroyCancellationToken).Forget();
        });
    }
}

public static class TriggerExt
{
    extension(MonoBehaviour self)
    {
        public void BindEvtTrg(EventTriggerType type, UnityAction<BaseEventData> callback, CancellationToken? ct = null)
        {
            ct ??= self.destroyCancellationToken;
            var trigger = self.gameObject.GetOrAddCom<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            ct.Value.Register(() => trigger.triggers.Remove(entry));
            trigger.triggers.Add(entry);
        }
    }
}