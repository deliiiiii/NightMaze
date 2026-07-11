using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using General;
using UnityEngine;

namespace GeneralPreview;

public static class MyInput
{
    static readonly KeyCode[] allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
    // 鼠标按键索引：0-左键, 1-右键, 2-中键
    static readonly Dictionary<int, Vector3?> mouseDragDic = new()
    {
        [0] = null,
        [1] = null,
        [2] = null,
    };
    
    
    public static void Init(CancellationToken ct)
    {
        var f = Action;
        
        f.ActBindTo().Bind(ct);
        return;
        
        void Action(float dt)
        {
            foreach (var key in allKeys)
            {
                if (Input.GetKeyDown(key)) new EvtKeyDown(key).Forget(debug: false);
                if (Input.GetKey(key))     new EvtKeyHold(key).Forget(debug: false);
                if (Input.GetKeyUp(key))   new EvtKeyUp(key).Forget(debug: false);
            }

            var mousePos = Input.mousePosition;
            mousePos.z = 0;
            foreach (var mouseKey in Enumerable.Range(0, 3))
            {
                if (Input.GetMouseButtonDown(mouseKey))
                {
                    mouseDragDic[mouseKey] = mousePos;
                }
                else if (Input.GetMouseButtonUp(mouseKey))
                {
                    mouseDragDic[mouseKey] = null;
                }
                else if(mouseDragDic[mouseKey].HasValue)
                {
                    var delta = mousePos - mouseDragDic[mouseKey]!.Value;
                    // MyDebug.Log($"Mouse Drag {mouseKey} {delta}");
                    if(delta.magnitude > 0)
                        new EvtMouseDrag(mouseKey, delta).Forget(debug: false);
                    mouseDragDic[mouseKey] = mousePos;
                }
            }
        }
    }
    
    
    public record EvtKeyDown(KeyCode Key) : EvtForgetBase;
    public record EvtKeyHold(KeyCode Key) : EvtForgetBase;
    public record EvtKeyUp(KeyCode Key) : EvtForgetBase;
    
    public record EvtMouseDrag(int MouseKey, Vector3 Delta) : EvtForgetBase;
}

