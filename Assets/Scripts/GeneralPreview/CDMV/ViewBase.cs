using System.Collections.Generic;
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
    }

    protected virtual void OnDestroy()
    {
        BindList().ForEach(b => b.UnBind());
    }
}