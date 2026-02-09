using System;
using System.Collections.Generic;
using System.Linq;

namespace MyFP;
//
// // public record MyList<T>(List<T> Items) : K1<MyList, T>;
// // public abstract class MyList : IMonad1<MyList>
// // {
// //     public static K1<MyList, T1B> Map<T1, T1B>(K1<MyList, T1> k, Func<T1, T1B> f) => 
// //         new MyList<T1B>(k.As().Items.Select(f).ToList());
// //     public static K1<MyList, T1> Pure<T1>(T1 value) => 
// //         new MyList<T1>([value]);
// //     // List<int>, List<int -> string>
// //     // List<string>
// //     public static K1<MyList, T1B> Apply<T1, T1B>(K1<MyList, T1> k, K1<MyList, Func<T1, T1B>> funcList)
// //     {
// //         var ret = new List<T1B>();
// //         foreach (var f in funcList.As().Items)
// //         {
// //             ret.AddRange(Map(k, f).As().Items);
// //         }
// //         return new MyList<T1B>(ret);
// //     }
// //
// //     public static K1<MyList, T1B> Bind<T1, T1B>(K1<MyList, T1> k, Func<T1, K1<MyList, T1B>> f)
// //     {
// //         var ret = new List<T1B>();
// //         foreach (var item in k.As().Items)
// //         {
// //             ret.AddRange(f(item).As().Items);
// //         }
// //         return new MyList<T1B>(ret);
// //     }
// // }
public static class ListExt
{
    // extension<T>(K1<MyList, T> k)
    // {
    //     [DebuggerStepThrough]public MyList<T> As() => (MyList<T>)k;
    //     [DebuggerStepThrough]public MyList<T1B> Select<T1B>(Func<T, T1B> f) => k.Map(f).As();
    //     [DebuggerStepThrough]public MyList<T1C> SelectMany<T1B, T1C>(Func<T, MyList<T1B>> f, Func<T, T1B, T1C> s) =>
    //         k.Bind(a => f(a).Map(b => s(a, b))).As();
    // }

    extension<T1>(IEnumerable<T1> self)
    {
        public MyOption<IEnumerable<T1>> WhereM(Func<T1, MyOption<bool>> predicateM) 
            => self.Aggregate(
                (MyOption<IEnumerable<T1>>)new MySome<IEnumerable<T1>>([]),
                (acc, item) =>
                    from xs in acc
                    from keep in predicateM(item)
                    select keep ? xs.Append(item) : xs
            );
        public MyIO<IEnumerable<T1>> WhereM(Func<T1, MyIO<bool>> predicateM)
            => self.Aggregate(
                new MyIO<IEnumerable<T1>>(() => []),
                (acc, item) =>
                    from xs in acc
                    from keep in predicateM(item)
                    select keep ? xs.Append(item) : xs
            );
    }
}