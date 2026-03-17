using System;
using System.Collections.Generic;
using System.Linq;
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
                assemblySet = null;
                insDic = null;
            }
        };
#endif
    }
    
    // ReSharper disable StaticMemberInGenericType
    static HashSet<Assembly>? assemblySet;
    static Dictionary<Type, Type>? insDic;
    static HashSet<Assembly> AssemblySet
    {
        get
        {
            if (assemblySet != null)
                return assemblySet;
            assemblySet = [];
            if (typeof(TInsBase).IsGenericType)
            {
                foreach (var arg in typeof(TInsBase).GetGenericArguments())
                {
                    assemblySet.Add(arg.Assembly);
                }
            }
            assemblySet.Add(typeof(TRelyBase).Assembly);
            return assemblySet;
        }
    }
    static Dictionary<Type, Type> InsDic
    {
        get
        {
            if (insDic != null)
                return insDic;
            insDic = [];
            typeof(TInsBase).SubTypes()
                .ForEach(type =>
                {
                    var attr = type.GetCustomAttribute<FacInsAttribute>();
                    if(attr != null)
                        insDic[attr.RelyType] = type;
                });
            return insDic;
        }
    }

    static readonly Type fallbackType = 
        (
            from subType in typeof(TInsBase).SubTypes() 
            where subType.GetCustomAttribute<FacFallbackAttribute>()?.RelyTypeBase == typeof(TRelyBase) 
            select subType)
        .FirstOrDefault() ?? throw new Exception($"未找到{typeof(TInsBase).GetNiceName()}的回退类型");
    public static TInsBase Create<TRely>(TRely rely) where TRely : TRelyBase
    {
        var relyType = rely.GetType();
        if (InsDic.TryGetValue(relyType, out var ins))
            return (TInsBase)Activator.CreateInstance(ins);
        return (TInsBase)Activator.CreateInstance(fallbackType);
    }
}