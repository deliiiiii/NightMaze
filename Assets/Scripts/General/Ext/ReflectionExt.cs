using System;
using System.Collections.Generic;
using System.Linq;

public static class ReflectionExt
{
    public static Type FirstSubType(this Type parentType)
    {
        return parentType.SubType().FirstOrDefault();
    }
    
    public static IEnumerable<Type> SubType(this Type parentType)
    {
        return parentType.Assembly.GetTypes().Where(x => x.IsSubclassOf(parentType));
    }
    
    public static bool HasAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttributes(typeof(T), true).Length > 0;
    }
}