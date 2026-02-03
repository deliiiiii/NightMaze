using System;
using JetBrains.Annotations;
using UnityEngine.Events;
using UnityEngine.UI;

public class BindDataObs<T> : BindDataBase
    where T : struct
{
    protected Observable<T> osv;
    protected UnityAction<T> act;
    // bool isImmediate = true;

    public BindDataObs<T> To(UnityAction<T> fAct)
    {
        osv.OnValueChangedAfter -= fAct;
        act = fAct;
        return this;
    }
    
    // public BindDataActTxt<T> ToTxt(Text txt)
    // {
    //     BeforeTo();
    //     var ret = new BindDataActTxt<T>(osv, txt);
    //     ret.AfterTo();
    //     return ret;
    // }
    //
    // public BindDataActImg<T> ToImg(Image img, [CanBeNull] Func<float, float> func = null)
    // {
    //     BeforeTo();
    //     func ??= v => v;
    //     var ret = new BindDataActImg<T>(osv, img, func);
    //     ret.AfterTo();
    //     return ret;
    // }

    
    public BindDataObs(Observable<T> osv)
    {
        this.osv = osv;
    }

    protected override void BindInternal()
    {
        osv.OnValueChangedAfter += act;
        // if (isImmediate)
        // {
            act(osv.Value);
        // }
    }

    public override void UnBind()
    {
        osv.OnValueChangedAfter -= act;
    }
}






