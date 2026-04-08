using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;
[DebuggerStepThrough]
public static class IEnumerableExt
{
    extension<T>(IEnumerable<T> self)
    {
        public MyOption<T> FirstOptional(Func<T, bool>? predicate = null) 
        {
            predicate ??= RTrue1;
            var list = self.ToList();
            if (!list.Any())
                return None;
            foreach (var item in list)
            {
                if (predicate(item))
                    return item;
            }
            return None;
        }
        public async UniTask ForEachAsync(Func<T, UniTask> action)
        {
            foreach (var item in self)
            {
                await action(item);
            }
        }
    }
}