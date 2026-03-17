using System;
using System.Collections.Generic;
using System.Linq;

namespace General
{
    public static class TypeExt
    {
        public static IEnumerable<Type> SubTypes(this Type type) =>
            from ass in AppDomain.CurrentDomain.GetAssemblies()
            from t in ass.GetTypes()
            where type.IsAssignableFrom(t) && t != type && !t.IsAbstract 
            select t;
    }
}