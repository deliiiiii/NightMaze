using System;
using GeneralPreview;
using NM.Data;
using NM.View.ZZZTest;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class GridMapScroll : MonoBehaviour, IMultiScrollHandler
{
    [SerializeField] bool enablePena = true;
    public int MinSeenGrid = 2;
    public int MaxSeenGrid = 24;
    public float TarSeenGrid = 12;
    public float Duration = 0.4f;
    [ShowInInspector]float TarOrtho => TarSeenGrid * Const.World.GridSize / 2f;
    void Awake()
    {
        IUniEvt.BindAll(this, destroyCancellationToken);
        Tween.Bind(() => MyCamera.MainV.m_Lens.OrthographicSize, cur => MyCamera.MainV.m_Lens.OrthographicSize = cur,
            () => TarOrtho, Duration,
            Tween.CubicOut,
            destroyCancellationToken);
        // this.BindEvtTrg(EventTriggerType.Scroll, _ =>
        // {
        // });
    }
    public void OnMultiScroll(PointerEventData eventData)
    {
        float y = Math.Clamp(eventData.scrollDelta.y, -1f, 1f) * GameRoot.Setting.MouseScrollMapSpeed;
        TarSeenGrid = Math.Clamp(TarSeenGrid - y, MinSeenGrid, MaxSeenGrid);
    }

    bool IPointerPena.EnablePena => enablePena;
}