using System;
using System.Collections.Generic;
using System.Threading;
using General;
using GeneralPreview;
using UnityEngine;

namespace NM.View;

public class LoadingView : ViewBase
{
    [SerializeField] Btn btnCancel;
    [SerializeField] Img imgProgress;
    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnCancel.onClick.EvtBindTo(() =>
        {
            if (curOnCancel != null)
            {
                curOnCancel.Invoke();
                Release();
            }
        });
    }

    public void Register(Action onCancel, Func<float>? getProgress = null)
    {
        curOnCancel = onCancel;
        curGetProgress = getProgress;
        gameObject.SetActiveTrue();
    }
    public void Release()
    {
        curOnCancel = null;
        gameObject.SetActiveFalse();
    }
    Action? curOnCancel;
    Func<float>? curGetProgress;

    void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (curGetProgress == null)
            return;
        imgProgress.fillAmount = Mathf.Clamp01(curGetProgress());
    }
}