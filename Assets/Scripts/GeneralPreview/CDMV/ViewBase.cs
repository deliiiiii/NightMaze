using System.Collections.Generic;
using General.BindData;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class ViewBase : MonoBehaviour
{
    protected virtual IEnumerable<IUniEvt> OnEvt() => [];
    protected virtual IEnumerable<BindDataBase> BindList() => [];

    void Awake()
    {
        OnEvt().RegAll();
        BindList().ForEach(b => b.Bind());
    }

    void OnDestroy()
    {
        BindList().ForEach(b => b.UnBind());
        OnEvt().UnRegAll();
    }
}