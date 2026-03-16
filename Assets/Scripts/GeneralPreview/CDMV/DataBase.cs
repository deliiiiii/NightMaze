using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using General;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace GeneralPreview;

public abstract class DataBase<TThis> : IDisposable, IHasCt
    where TThis : DataBase<TThis>
{
    /// 状态初始化完成后调用，绑定组件的TThis及事件
    public void BindAllCom()
    {
        foreach (var com in comDic.Values)
        {
            com.BelongData = (TThis)this;
            com.BindToData();
        }
    }
    public void Dispose() 
    {
        RemoveAllCom();
        cts.Cancel();
    }
    public CancellationToken Ct => cts.Token;
    
    double savedVersion = Const.Version;
    [ShowInInspector] readonly Dictionary<Type, ComBase> comDic = [];
    [HideInInspector, JsonIgnore] readonly CancellationTokenSource cts = new();
    
    [DebuggerStepThrough] protected T AddCom<T>(T? com = null) where T : ComBase
    {
        if (comDic.TryGetValue(typeof(T), out var existCom))
        {
            MyDebug.LogError($"Entity {GetType().Name} AddComponent {typeof(T).Name} But Already Exists");
            return (T)existCom;
        }

        com ??= Activator.CreateInstance<T>();
        com.BelongData = (TThis)this;
        com.BindToData();
        comDic.Add(typeof(T), com);
        return com;
    }
    [DebuggerStepThrough] protected void RemoveCom<T>() where T : ComBase
    {
        if (!comDic.TryGetValue(typeof(T), out var com))
        {
            MyDebug.LogError($"Entity {ToString()} RemoveComponent {typeof(T).Name} But NOT Exists");
            return;
        }
        com.Dispose();
        comDic.Remove(typeof(T));
    }
    [DebuggerStepThrough] void RemoveAllCom()
    {
        comDic.Values.ForEach(com => com.Dispose());
        comDic.Clear();
    }

    [DebuggerStepThrough] public MyOption<T> GetCom<T>() where T : ComBase
        => comDic.TryGetValue(typeof(T), out var com) ? (T)com : None;
    [DebuggerStepThrough] public bool HasCom<T>() where T : ComBase 
        => comDic.ContainsKey(typeof(T));
    
    public abstract class ComBase : IDisposable
    {
        [HideInInspector, JsonIgnore] public required TThis BelongData { get; set; }
        [HideInInspector, JsonIgnore] readonly CancellationTokenSource cts = new();
        public void BindToData()
        {
            IUniEvt.BindAll(this, cts.Token);
        }
        public void Dispose()
        {
            cts.Cancel();
        }
    }
}