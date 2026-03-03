using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using General;
using Sirenix.OdinInspector;

namespace GeneralPreview;

public abstract class DataBase<TThis>
    where TThis : DataBase<TThis>
{
    [ShowInInspector] readonly Dictionary<Type, ComBase> comDic = [];
    [DebuggerStepThrough]
    protected T AddCom<T>(T? com = null) where T : ComBase
    {
        if (comDic.TryGetValue(typeof(T), out var existCom))
        {
            MyDebug.LogError($"Entity {GetType().Name} AddComponent {typeof(T).Name} But Already Exists");
            return (T)existCom;
        }

        com ??= Activator.CreateInstance<T>();
        com.BelongData = (TThis)this;
        comDic.Add(typeof(T), com);
        return com;
    }
    [DebuggerStepThrough]
    protected void RemoveCom<T>() where T : ComBase
    {
        if (!comDic.TryGetValue(typeof(T), out _))
        {
            MyDebug.LogError($"Entity {ToString()} RemoveComponent {typeof(T).Name} But NOT Exists");
            return;
        }
        comDic.Remove(typeof(T));
    }
    [DebuggerStepThrough]
    protected void RemoveAllCom() => comDic.Clear();

    [DebuggerStepThrough]
    public MyOption<T> GetCom<T>() where T : ComBase
        => comDic.TryGetValue(typeof(T), out var com) ? (T)com : None;
    [DebuggerStepThrough]
    public bool HasCom<T>() where T : ComBase 
        => comDic.ContainsKey(typeof(T));

    public abstract class ComBase
    {
        public required TThis BelongData { get; set; }
    }
}