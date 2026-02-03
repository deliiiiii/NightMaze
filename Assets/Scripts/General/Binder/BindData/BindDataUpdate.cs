using System;
using System.Collections.Generic;
using System.Linq;

public class BindDataUpdate : BindDataBase
{
    public readonly Action<float> Act;
    
    readonly int priority;

    public BindDataUpdate(Action<float> act, EUpdatePri e)
    {
        Act = act;
        priority = (int)e;
    }

    protected override void BindInternal()
    {
        Updater.UpdateDic.TryAdd(priority, new HashSet<BindDataUpdate>());
        Updater.UpdateDic[priority].Add(this);
    }
    
    public override void UnBind()
    {
        if (Updater.UpdateDic.TryGetValue(priority, out var value))
        {
            var found = value.FirstOrDefault(v => v.Act == Act);
            if(found != null)
                value.Remove(found);
        }
    }
}

public enum EUpdatePri
{
    Default = -1,
    Input,
    
    Fsm = 10,
    
    Sprite = 11,
}