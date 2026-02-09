using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace GeneralPreview;

public abstract class EttBase<TThis>
    where TThis : EttBase<TThis>
{
    protected EttBase() => entityID = nextEntityID++;
    // ReSharper disable once StaticMemberInGenericType
    static int nextEntityID;
    int entityID;
    [ShowInInspector] readonly Dictionary<Type, ICom> comDic = [];
    [DebuggerStepThrough]
    public T AddCom<T>(T com)
        where T : ICom
    {
        if (comDic.TryGetValue(typeof(T), out var existCom))
        {
            return (T)existCom;
        }
        comDic.Add(typeof(T), com);
        return com;
    }
    [DebuggerStepThrough]
    public void RemoveCom<T>() where T : ICom
    {
        if (!comDic.TryGetValue(typeof(T), out _))
        {
            throw new Exception($"Entity {GetType().Name} RemoveComponent {typeof(T).Name} But NOT Exists");
        }
        comDic.Remove(typeof(T));
    }
    [DebuggerStepThrough]
    public void RemoveAllCom()
    {
        comDic.Clear();
    }
    
    [DebuggerStepThrough]
    public MyOption<T> GetCom<T>() where T : ICom
    {
        if (comDic.TryGetValue(typeof(T), out var com))
        {
            return (T)com;
        }
        return None;
        // throw new Exception($"Entity {GetType().Name} GetComponent {typeof(T).Name} But NOT Exists");
    }

    public interface ICom;
    public interface ICom<TCtx> : ICom;
    [DebuggerStepThrough]
    public CtxScope<TCtx> In<TCtx>(TCtx ctx) where TCtx : IEvtCtx => new((TThis)this);
    [DebuggerStepThrough]
    T GetByCtx<TCtx, T>(TCtx _) where T : ICom<TCtx>
    {
        if (comDic.TryGetValue(typeof(T), out var existCom))
            return (T)existCom;
        throw new Exception($"Entity {GetType().Name} GetComponent {typeof(T).Name} With Ctx({typeof(TCtx)}) But NOT Exists");    
    }
    public readonly struct CtxScope<TCtx>(TThis self)
    {
        public TCom As<TCom>() where TCom : ICom<TCtx>
        {
            return self.GetByCtx<TCtx, TCom>(default!);
        }
    }
}

public interface IEttTick
{
    void Tick(float dt);
}