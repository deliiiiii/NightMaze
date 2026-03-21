using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GeneralPreview
{
    public static class IEnumerableExt
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    
        public static bool AnyType<T>(this IEnumerable self)
            => self.OfType<T>().Any();
    }
}