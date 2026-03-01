using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using General.BindData;
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