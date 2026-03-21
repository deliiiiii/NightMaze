using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralPreview;

public static class TypeExt
{
    extension(Type type)
    {
        public IEnumerable<Type> SubTypes() =>
            from ass in AppDomain.CurrentDomain.GetAssemblies()
            from t in ass.GetTypes()
            where type.IsAssignableFrom(t) && t != type && !t.IsAbstract 
            select t;
    }
}