using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using General;
using Sirenix.Utilities;

namespace GeneralPreview;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FacInsAttribute(Type relyType) : Attribute
{
    public readonly Type RelyType = relyType;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FacFallbackAttribute(Type relyTypeBase) : Attribute
{
    public readonly Type RelyTypeBase = relyTypeBase;
}

public static class Factory<TRelyBase, TInsBase>
    where TRelyBase : class
    where TInsBase : class
{
    static Factory()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                insDic.Clear();
            }
        };
#endif
        
        var subTypes = typeof(TInsBase).SubTypes().ToList();
        foreach (Type subType in subTypes)
        {
            if (subType.GetCustomAttribute<FacFallbackAttribute>()?.RelyTypeBase == typeof(TRelyBase))
            {
                fallbackFunc = NewByType(subType);
                break;
            }
        }
        if (fallbackFunc == null)
            MyDebug.LogError($"未找到{typeof(TInsBase).GetNiceName()}的回退类型");
        foreach (var subType in subTypes)
        {
            FacInsAttribute attr = subType.GetCustomAttribute<FacInsAttribute>();
            if(attr != null)
                insDic[attr.RelyType] = NewByType(subType);
        }

    }
    // // ReSharper disable once StaticMemberInGenericType
    // [AllowNull] [field: MaybeNull]
    // static HashSet<Assembly> AssemblySet
    // {
    //     get
    //     {
    //         if (field != null)
    //             return field;
    //         field = [];
    //         if (typeof(TInsBase).IsGenericType)
    //         {
    //             foreach (var arg in typeof(TInsBase).GetGenericArguments())
    //             {
    //                 field.Add(arg.Assembly);
    //             }
    //         }
    //         field.Add(typeof(TRelyBase).Assembly);
    //         return field;
    //     }
    //     set;
    // }
    // ReSharper disable once StaticMemberInGenericType
    [AllowNull] static readonly Dictionary<Type, Func<TInsBase>> insDic = [];
    static readonly Func<TInsBase>? fallbackFunc;
    public static TInsBase Create<TRely>(TRely rely)
        where TRely : TRelyBase =>
        insDic.GetValueOrDefault(rely.GetType())?.Invoke() 
        ?? fallbackFunc?.Invoke()
        ?? throw new Exception($"{typeof(TInsBase).GetNiceName()} 创建失败. 未找到对应的类型, 且没有回退类型");

    static Func<TInsBase> NewByType(Type type)
    {
        // () => new Type()
        var newExp = Expression.New(type);
        var lambda = Expression.Lambda<Func<TInsBase>>(newExp);
        return lambda.Compile();
    }
}