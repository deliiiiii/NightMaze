using System;
using System.Collections.Generic;
using System.Linq;

public static class TypeExt
{
    public static IEnumerable<Type> SubTypeList(this Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => type.IsAssignableFrom(t) && t != type && !t.IsAbstract);
    }
}