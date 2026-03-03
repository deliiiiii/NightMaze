using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;

namespace NM.Data;

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
        UnityEditor.EditorApplication.playModeStateChanged += state =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                assemblySet = null;
                insDic = null;
            }
        };
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
            HashSet<Assembly> ret = [];
            if (typeof(TInsBase).IsGenericType)
            {
                foreach (var arg in typeof(TInsBase).GetGenericArguments())
                {
                    ret.Add(arg.Assembly);
                }
            }
            ret.Add(typeof(TRelyBase).Assembly);
            return ret;
        }
    }
    static Dictionary<Type, Type> InsDic
    {
        get
        {
            if (insDic != null)
                return insDic;
            Dictionary<Type, Type> ret = [];
            AssemblySet.SelectMany(a => a.GetTypes())
                .Where(type => 
                    type.IsSubclassOf(typeof(TInsBase)) 
                    && !type.IsAbstract)
                .ForEach(type =>
                {
                    var attr = type.GetCustomAttribute<FacInsAttribute>();
                    if(attr != null)
                        ret[attr.RelyType] = type;
                });
            return ret;
        }
    }

    static readonly Type fallbackType = 
        AssemblySet
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(type => type.IsSubclassOf(typeof(TInsBase)) 
                                    && !type.IsAbstract
                                    && type.GetCustomAttribute<FacFallbackAttribute>()
                                        ?.RelyTypeBase == typeof(TRelyBase)) 
                                        ?? throw new Exception($"未找到{typeof(TInsBase)}的回退类型");
    public static TInsBase Create<TRely>(TRely rely) where TRely : TRelyBase
    {
        var relyType = rely.GetType();
        if (InsDic.TryGetValue(relyType, out var ins))
            return (TInsBase)Activator.CreateInstance(ins);
        return (TInsBase)Activator.CreateInstance(fallbackType);
    }
    
    
}

