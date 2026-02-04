using System;
using System.Collections.Generic;
using System.Linq;

public static class TypeExt
{
    public static IEnumerable<Type> GetSubTypes(this Type type)
    {
        return type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && t != type && !t.IsAbstract);
    }
}