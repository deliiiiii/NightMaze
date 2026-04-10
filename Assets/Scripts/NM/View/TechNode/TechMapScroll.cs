using System;
using GeneralPreview;
using NM.View.ZZZTest;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class TechMapScroll : MonoBehaviour, IMultiScrollHandler
{
    [SerializeField] bool enablePena = true;
    public float MinOr = 2;
    public float MaxOr = 24;
    public float TarOr = 12;
    public float Duration = 0.4f;
    public float Speed = 1f;
    void Awake()
    {
        IUniEvt.BindAll(this, destroyCancellationToken);
        Tween.Bind(() => MyCamera.UIV.m_Lens.OrthographicSize, cur => MyCamera.UIV.m_Lens.OrthographicSize = cur,
            () => TarOr, Duration,
            Tween.CubicOut,
            destroyCancellationToken);
        // this.BindEvtTrg(EventTriggerType.Scroll, _ =>
        // {
        //     int y = (int)Math.Clamp(Input.mouseScrollDelta.y, -1, 1);
        //     // MyDebug.Log($"scroll {y}");
        //     TarSeenGrid = Math.Clamp(TarSeenGrid - y, MinSeenGrid, MaxSeenGrid);
        // });
    }
    
    public void OnMultiScroll(PointerEventData eventData)
    {
        float y = Math.Clamp(eventData.scrollDelta.y, -1, 1) * Speed;
        // MyDebug.Log($"scroll {y}");
        TarOr = Math.Clamp(TarOr - y, MinOr, MaxOr);
    }

    bool IPointerPena.EnablePena => enablePena;
}
