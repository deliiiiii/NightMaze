using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneralPreview;

[DebuggerStepThrough]
public record MyEnv<TEnv, T1>(Func<TEnv, T1> RunEnv)
{
    public MyEnv<TEnv, T1B> Map<T1B>(Func<T1, T1B> f) =>
        new (env => f(RunEnv(env)));
    public MyEnv<TEnv, T1> Pure(T1 value) => 
        new (_ => value);
    public MyEnv<TEnv, T1B> Apply<T1B>(MyEnv<TEnv, Func<T1, T1B>> f) =>
        new(env => f.RunEnv(env)(RunEnv(env)));
    public MyEnv<TEnv, T1B> Bind<T1B>(Func<T1, MyEnv<TEnv, T1B>> f) =>
        new(env => f(RunEnv(env)).RunEnv(env));
    
    public MyEnv<TEnv, T1B> Select<T1B>(Func<T1, T1B> f) => Map(f);
    public MyEnv<TEnv, T1C> SelectMany<T1B, T1C>(Func<T1, MyEnv<TEnv, T1B>> f, Func<T1, T1B, T1C> s) =>
        Bind(a => f(a).Map(b => s(a, b)));
    public bool Where(bool v) => v;
    public MyEnv<TEnv, IEnumerable<T1>> Replicate(int count) 
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
[DebuggerStepThrough]
public static class EnvExt
{
    extension<TEnv, T>(MyEnv<TEnv, MyOption<T>> k)
    {
        public MyEnv<TEnv, MyOption<T>> ToNone() => new(_ => None);
    }
}