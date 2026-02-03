using JetBrains.Annotations;
using UnityEngine.Events;

public class BindDataEvent : BindDataBase
{
    [CanBeNull] UnityAction act;
    readonly UnityEvent evt;

    public BindDataEvent(UnityEvent evt)
    {
        this.evt = evt;
    }

    public BindDataEvent To(UnityAction fAct)
    {
        if(act != null)
            evt.RemoveListener(act);
        act = fAct;
        return this;
    }

    protected override void BindInternal()
    {
        evt.AddListener(act);
    }

    public override void UnBind()
    {
        if(act != null)
            evt.RemoveListener(act);
    }
    
}

public class BindDataEvent<T> : BindDataBase
{
    UnityAction<T> act;
    [CanBeNull] readonly UnityEvent<T> evt;

    public BindDataEvent(UnityEvent<T> evt)
    {
        this.evt = evt;
    }

    public BindDataEvent<T> To(UnityAction<T> fAct)
    {
        act = fAct;
        return this;
    }

    protected override void BindInternal()
    {
        evt?.AddListener(act);
    }

    public override void UnBind()
    {
        evt?.RemoveListener(act);
    }
    
}