using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralPreview;

public static class FuncExt
{
    extension<TR>(Func<TR> self)
    {
        public List<TR> Repeat(int n) 
            => n < 0 
                ? throw new ArgumentException("n must be non-negative", nameof(n)) 
                : Enumerable.Range(0, n).Select(_ => self()).ToList();
    }

    extension<T1, TR>(Func<T1, TR> self)
    {
        public Func<T1, List<TR>> Repeat(int n) 
            => n < 0 
                ? throw new ArgumentException("n must be non-negative", nameof(n)) 
                : t1 => Enumerable.Range(0, n).Select(_ => self(t1)).ToList();
    }
}