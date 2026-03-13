using System;
using System.Collections.Generic;
using System.Linq;

namespace General
{
    public static class TypeExt
    {
        public static IEnumerable<Type> SubTypes(this Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t != type && !t.IsAbstract);
        }
    }
}