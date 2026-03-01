using System;
using System.Collections.Generic;
using System.Diagnostics;
using General;
using Sirenix.OdinInspector;

namespace GeneralPreview;

public abstract class DataBase<TThis>
    where TThis : DataBase<TThis>
{
    [ShowInInspector] readonly Dictionary<Type, ICom> comDic = [];
    [DebuggerStepThrough]
    public T AddCom<T>(T? com = null)
        where T : class, ICom, new()
    {
        if (comDic.TryGetValue(typeof(T), out var existCom))
        {
            MyDebug.LogError($"Entity {GetType().Name} AddComponent {typeof(T).Name} But Already Exists");
            return (T)existCom;
        }

        com ??= new T();
        comDic.Add(typeof(T), com);
        return com;
    }
    [DebuggerStepThrough]
    public void RemoveCom<T>() where T : class, ICom, new()
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
    public MyOption<T> GetCom<T>() where T : class, ICom, new() 
        => comDic.TryGetValue(typeof(T), out var com) ? (T)com : None;
    // [DebuggerStepThrough]
    // public bool HasCom<T>() where T : class, ICom, new() => comDic.ContainsKey(typeof(T));

    public interface ICom;
}