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
            var first = self.FirstOrDefault(predicate);
            if (first is not null)
                return first;
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