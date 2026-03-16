using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneralPreview;

public static class FuncExt
{
    #region Repeat
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
    #endregion
    
    #region operator
    extension<T1, TRet>(Func<T1, TRet> self)
    {
        public Func<T1, TRet> Wrap(Action<T1>? before, Action<T1>? after)
            => t1 =>
            {
                before?.Invoke(t1);
                var ret = self(t1);
                after?.Invoke(t1);
                return ret;
            };
        // 函数调用参数T1
        public static TRet operator >>(Func<T1, TRet> @this, T1 t1) => @this(t1);
    }
    extension<T1, T2, TRet>(Func<T1, T2, TRet> self)
    {
        // 函数调用部分参数T1
        public static Func<T2, TRet> operator >>(Func<T1, T2, TRet> @this, T1 t1) => t2 => @this(t1, t2);
        // 函数调用参数T1, T2
        public static TRet operator >>(Func<T1, T2, TRet> @this, (T1, T2) args) => @this(args.Item1, args.Item2);
    }
    
    extension<T1, TMid, TRet>(Func<TMid, TRet>)
    {
        public static Func<T1, TRet> operator *(Func<TMid, TRet> @this, Func<T1, TMid> right) => x => @this(right(x));
    }
    extension<T1, T2, TMid, TRet>(Func<TMid, TRet>)
    {
        public static Func<T1, T2, TRet> operator *(Func<TMid, TRet> @this, Func<T1, T2, TMid> right) => (x, y) => @this(right(x, y));
    }
    #endregion
}