using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralPreview;

public static class IEnumerableExt
{
    extension<T>(IEnumerable<T> self)
    {
        public MyOption<T> MyFirst(Func<T, bool> predicate)
        {
            var first = self.FirstOrDefault(predicate);
            if (first is not null)
                return first;
            return None;
        }
    }
}