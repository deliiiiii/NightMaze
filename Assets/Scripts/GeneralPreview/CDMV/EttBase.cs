using System;
using System.Collections.Generic;
using System.Diagnostics;
using General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeneralPreview;

public abstract class EttBase<TThis>
    where TThis : EttBase<TThis>
{
    protected EttBase() => entityID = nextEntityID++;
    // ReSharper disable once StaticMemberInGenericType
    static int nextEntityID;
    [HideInInspector]int entityID;
    [ShowInInspector] readonly Dictionary<Type, ICom> comDic = [];
    [DebuggerStepThrough]
    public T AddCom<T>(T com)
        where T : ICom
    {
        if (comDic.TryGetValue(typeof(T), out var existCom))
        {
            MyDebug.LogError($"Entity {GetType().Name} AddComponent {typeof(T).Name} But Already Exists");
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
            MyDebug.LogError($"Entity {ToString()} RemoveComponent {typeof(T).Name} But NOT Exists");
            return;
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
    }

    public interface ICom;
    public interface ICom<TCtx> : ICom;
    [DebuggerStepThrough]
    public CtxScope<TCtx> Ctx<TCtx>(TCtx ctx) => new((TThis)this);
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