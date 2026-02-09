using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyFP;

[DebuggerStepThrough]
public record MyEnv<TEnv, T1>(Func<TEnv, T1> RunEnv)
{
    [DebuggerStepThrough] public MyEnv<TEnv, T1B> Map<T1B>(Func<T1, T1B> f) =>
        new (env => f(RunEnv(env)));
    [DebuggerStepThrough] public MyEnv<TEnv, T1> Pure(T1 value) => 
        new (_ => value);
    [DebuggerStepThrough] public MyEnv<TEnv, T1B> Apply<T1B>(MyEnv<TEnv, Func<T1, T1B>> f) =>
        new(env => f.RunEnv(env)(RunEnv(env)));
    [DebuggerStepThrough] public MyEnv<TEnv, T1B> Bind<T1B>(Func<T1, MyEnv<TEnv, T1B>> f) =>
        new(env => f(RunEnv(env)).RunEnv(env));
    
    [DebuggerStepThrough]public MyEnv<TEnv, T1B> Select<T1B>(Func<T1, T1B> f) => Map(f);
    [DebuggerStepThrough]public MyEnv<TEnv, T1C> SelectMany<T1B, T1C>(Func<T1, MyEnv<TEnv, T1B>> f, Func<T1, T1B, T1C> s) =>
        Bind(a => f(a).Map(b => s(a, b)));
    [DebuggerStepThrough]public bool Where(bool v) => v;
    [DebuggerStepThrough]public MyEnv<TEnv, IEnumerable<T1>> Replicate(int count) 
        => new(env =>
        {
            var results = new List<T1>(count);
            for (int i = 0; i < count; i++)
            {
                results.Add(RunEnv(env));
            }
            return results;
        });
}

public static class EnvExt
{
    extension<TEnv, T>(MyEnv<TEnv, MyOption<T>> k)
    {
        [DebuggerStepThrough]public MyEnv<TEnv, MyOption<T>> ToNone() => new(_ => None);
    }
}