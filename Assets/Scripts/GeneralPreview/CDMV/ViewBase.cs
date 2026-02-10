using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected virtual IEnumerable<IActionWrap> OnEvt() => [];

    void Awake()
    {
        OnEvt().ForEach(wrap => wrap.Register());
    }

    void OnDestroy()
    {
        OnEvt().ForEach(wrap => wrap.UnRegister());
    }
}