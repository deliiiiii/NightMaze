using System;
using General;
using GeneralPreview;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NM.View;

public class GridMapScroll : MonoBehaviour
{
    public int MinSeenGrid = 2;
    public int MaxSeenGrid = 24;
    public int TarSeenGrid = 12;
    public float Duration = 0.4f;
    [ShowInInspector]float TarOrtho => TarSeenGrid * GridSize / 2f;
    public const int GridSize = 1;
    void Awake()
    {
        IUniEvt.BindAll(this, destroyCancellationToken);
        // var tickMouse = TickMouse;
        // tickMouse.ToBinder().Bind(destroyCancellationToken);
        Tween.Bind(() => MyCamera.MainV.m_Lens.OrthographicSize, cur => MyCamera.MainV.m_Lens.OrthographicSize = cur,
            () => TarOrtho, Duration,
            Tween.CubicOut,
            destroyCancellationToken);
        
        this.BindEvtTrg(EventTriggerType.Scroll, _ =>
        {
            int y = (int)Math.Clamp(Input.mouseScrollDelta.y, -1, 1);
            MyDebug.Log($"scroll {y}");
            TarSeenGrid = Math.Clamp(TarSeenGrid - y, MinSeenGrid, MaxSeenGrid);
        });
    }

    void TickMouse(float dt)
    {
        int y = (int)Math.Clamp(Input.mouseScrollDelta.y, -1, 1);
        TarSeenGrid = Math.Clamp(TarSeenGrid - y, MinSeenGrid, MaxSeenGrid);
    }
    
    
    
    
    // UniEvt<MyInput.EvtKeyDown> EvtMouseDown => new()
    // {
    //     Des = "按下鼠标键",
    //     Invoke = (evt, ct) =>
    //     {
    //         if (evt.Key == KeyCode.Mouse0)
    //         {
    //             MyDebug.Log("0");
    //         }
    //         else if(evt.Key == KeyCode.Mouse1)
    //         {
    //             MyDebug.Log("1");
    //         }
    //         return UniTask.CompletedTask;
    //     },
    // };
}