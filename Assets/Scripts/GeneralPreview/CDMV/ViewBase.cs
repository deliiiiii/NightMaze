using System.Collections.Generic;
using General;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected virtual IEnumerable<BindDataBase> BindList() => [];

    protected virtual void Awake()
    {
        BindList().ForEach(b => b.Bind());
        IUniEvt.BindAll(this, destroyCancellationToken);
    }

    protected virtual void OnDestroy()
    {
        BindList().ForEach(b => b.UnBind());
    }
}