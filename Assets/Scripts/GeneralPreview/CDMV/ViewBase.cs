using System.Collections.Generic;
using General;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected virtual IEnumerable<BindDataBase> BindList() => [];
    bool bind;

    public virtual void Bind()
    {
        if (bind)
        {
            MyDebug.LogError($"{GetType().GetNiceName()} already bound");
            return;
        }
        BindList().ForEach(b => b.Bind());
        IUniEvt.BindAll(this, destroyCancellationToken);
        bind = true;
    }

    void Awake()
    {
        if(!bind)
            Bind();
    }

    protected virtual void OnDestroy()
    {
        BindList().ForEach(b => b.UnBind());
    }
}