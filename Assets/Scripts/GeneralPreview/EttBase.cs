using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneralPreview;

public abstract class EttBase<TThis>
    where TThis : EttBase<TThis>
{
    protected EttBase() => entityID = nextEntityID++;
    // ReSharper disable once StaticMemberInGenericType
    static int nextEntityID;
    int entityID;
    protected readonly Dictionary<Type, ICom> ComDic = [];
    [DebuggerStepThrough]
    public T AddCom<T>(T com)
        where T : ICom
    {
        if (ComDic.TryGetValue(typeof(T), out var existCom))
        {
            return (T)existCom;
        }
        ComDic.Add(typeof(T), com);
        return com;
    }
    [DebuggerStepThrough]
    public void RemoveCom<T>() where T : ICom
    {
        if (!ComDic.TryGetValue(typeof(T), out _))
        {
            throw new Exception($"Entity {GetType().Name} RemoveComponent {typeof(T).Name} But NOT Exists");
        }
        ComDic.Remove(typeof(T));
    }
    [DebuggerStepThrough]
    public void RemoveAllCom()
    {
        ComDic.Clear();
    }
    [DebuggerStepThrough]
    public T GetOrAddCom<T>(T com) where T : ICom
    {
        if (ComDic.TryGetValue(typeof(T), out var existCom))
            return (T)existCom;
        return AddCom(com);
    }
    [DebuggerStepThrough]
    public MyOption<T> GetCom<T>() where T : ICom
    {
        if (ComDic.TryGetValue(typeof(T), out var com))
        {
            return (T)com;
        }
        return None;
        // throw new Exception($"Entity {GetType().Name} GetComponent {typeof(T).Name} But NOT Exists");
    }

    public interface ICom;
    public interface IRequireCom<T> where T : ICom;
}

public interface IEttTick
{
    void Tick(float dt);
}