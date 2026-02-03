using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public abstract class BindDataBase
{
    public void Bind()
    {
        if (guardSet.Any(guard => !guard.Invoke()))
        {
            return;
        }

        BindInternal();
    }
    protected abstract void BindInternal();
    public abstract void UnBind(); 
    readonly HashSet<Func<bool>> guardSet = new ();
    public T Where<T>(Func<bool> guard)
        where T : BindDataBase
    {
        guardSet.Add(guard);
        return this as T;
    }
}


