using System;
using System.Collections.Generic;
using General.BindData;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected virtual IEnumerable<IFuncWrap> OnEvt() => [];
    protected virtual IEnumerable<BindDataBase> BindList() => [];

    void Awake()
    {
        OnEvt().ForEach(wrap => wrap.Register());
        BindList().ForEach(b => b.Bind());
    }

    void OnDestroy()
    {
        BindList().ForEach(b => b.UnBind());
        OnEvt().ForEach(wrap => wrap.UnRegister());
    }
}