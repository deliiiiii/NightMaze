using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[Serializable]
public class BuffFloat
{
    [ShowInInspector] float value;

    public float Value
    {
        get
        {
            var ret = value;
            ret += baseAddSet.Sum(baseAdd => baseAdd.Value);
            ret = baseMultiSet.Aggregate(ret, (cur, baseMulti) => cur * (1f + baseMulti.Value));
            ret = finalMultiSet.Aggregate(ret, (cur, finalMulti) => cur * (1f + finalMulti.Value));
            ret += finalAddSet.Sum(finalAdd => finalAdd.Value);
            return ret;
        }
    }
    
    [ShowInInspector, ReadOnly]
    HashSet<Buff> baseAddSet = new HashSet<Buff>();
    [ShowInInspector, ReadOnly]
    HashSet<Buff> baseMultiSet = new HashSet<Buff>();
    [ShowInInspector, ReadOnly]
    HashSet<Buff> finalAddSet = new HashSet<Buff>();
    [ShowInInspector, ReadOnly]
    HashSet<Buff> finalMultiSet = new HashSet<Buff>();
    
    public void ImposeBaseAdd(Buff b)
    {
        baseAddSet.Add(b);
    }
    
    public void RemoveBaseAdd(Buff b)
    {
        baseAddSet.Remove(b);
    }
    
    public void ImposeBaseMulti(Buff b)
    {
        baseMultiSet.Add(b);
    }
    public void RemoveBaseMulti(Buff b)
    {
        baseMultiSet.Remove(b);
    }
    
    public void ImposeFinalAdd(Buff b)
    {
        finalAddSet.Add(b);
    }
    public void RemoveFinalAdd(Buff b)
    {
        finalAddSet.Remove(b);
    }
    public void ImposeFinalMulti(Buff b)
    {
        finalMultiSet.Add(b);
    }
    public void RemoveFinalMulti(Buff b)
    {
        finalMultiSet.Remove(b);
    }
    
    public static implicit operator float(BuffFloat config)
    {
        return config.Value;
    }
    
}