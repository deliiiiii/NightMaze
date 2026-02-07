using System;
using System.Collections.Generic;
using General.PriorityQueue;

namespace NM;

public class EvtBus
{
    static readonly Dictionary<Type, SimplePriorityQueue<ActionWrapper, int>> onReceiveDic = [];
    static readonly Dictionary<object, ActionWrapper> wrapperDic = new();
    public static void Register<T>(Action<T> act, int priority = 0) where T : EvtBase
    {
        onReceiveDic.TryAdd(typeof(T), []);
        // if
        var wrapper = new ActionWrapper
        {
            Action = eBase => act((T)eBase!),
            Priority = priority,
        };
        onReceiveDic[typeof(T)].Enqueue(wrapper, priority);
    }
    public static void UnRegister<T>(Action<T> e)
    {
        // onReceiveDic[typeof(T)].Remove(e);
    }
    public static void UnRegisterAll<T>()
    {
        onReceiveDic.Remove(typeof(T));
    }
    public static void Fire<T>(T e) where T : EvtBase
    {
        
    }
    
    class ActionWrapper
    {
        public required Action<EvtBase> Action;
        public required int Priority;
        public string Des = "None";
    }
    
    public abstract class EvtBase;
}


// Data: 
//      Fire: new XXXDataXXXChanged(value))
//      Register：(OnClickAdd evtArg => XXXData.xxx += evtArg.Value)
// View