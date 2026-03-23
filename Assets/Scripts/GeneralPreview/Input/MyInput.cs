using System;
using System.Threading;
using General;
using UnityEngine;

namespace GeneralPreview;

public static class MyInput
{
    static readonly KeyCode[] allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
    // 鼠标按键索引：0-左键, 1-右键, 2-中键
    static readonly int[] mouseButtons = [0, 1, 2];
    public static void Init(CancellationToken ct)
    {
        var f = Action;
        
        f.ToBinder().Bind(ct);
        return;

        void Action(float dt)
        {
            foreach (var key in allKeys)
            {
                if (Input.GetKeyDown(key)) new EvtKeyDown(key).Forget(debug: false);
                if (Input.GetKey(key))     new EvtKeyHold(key).Forget(debug: false);
                if (Input.GetKeyUp(key))   new EvtKeyUp(key).Forget(debug: false);
            }
        }
    }
    public record EvtKeyDown(KeyCode Key) : EvtForgetBase;
    public record EvtKeyHold(KeyCode Key) : EvtForgetBase;
    public record EvtKeyUp(KeyCode Key) : EvtForgetBase;
}

