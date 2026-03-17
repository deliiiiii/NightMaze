using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneralPreview;

[DebuggerStepThrough]
public record MyIO<T1>(Func<T1> Run)
{
    public MyIO<T1B> Map<T1B>(Func<T1, T1B> f)
        => new(() => f(Run()));
    public static MyIO<T1> Pure(T1 value) 
        => new(() => value);
    // MyIO<int>, MyIO<int -> string>
    // MyIO<string>
    public MyIO<T1B> Apply<T1B>(MyIO<Func<T1, T1B>> f)
        // => Map(k, f.Run());
        => new(() => f.Run()(Run()));
    
    // MyIO<int>, int -> MyIO<string>
    // MyIO<string>
    public MyIO<T1B> Bind<T1B>(Func<T1, MyIO<T1B>> f) =>
        // Map(k, f).Run();
        new(() => f(Run()).Run());
    
    public MyIO<T1B> Select<T1B>(Func<T1, T1B> f) => Map(f);
    public MyIO<T1C> SelectMany<T1B, T1C>(Func<T1, MyIO<T1B>> f, Func<T1, T1B, T1C> s) =>
        Bind(a => f(a).Map(b => s(a, b)));

    public MyIO<IEnumerable<T1>> Replicate(int count) 
        => new(() =>
        {
            var results = new List<T1>(count);
            for (int i = 0; i < count; i++)
            {
                results.Add(Run());
            }
            return results;
        });
}