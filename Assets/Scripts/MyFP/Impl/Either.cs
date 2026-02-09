using System;
using System.Diagnostics;

namespace MyFP;

public abstract record MyEither<T1, T2>
{
    public static implicit operator MyEither<T1, T2>(T1 left) => new Left<T1, T2>(left);
    public static implicit operator MyEither<T1, T2>(T2 right) => new Right<T1, T2>(right);
    
    [DebuggerStepThrough]public MyEither<T1B, T2> Map1<T1B>(Func<T1, T1B> f)
        => this switch
        {
            Left<T1, T2> left => new Left<T1B, T2>(f(left.Value)),
            Right<T1, T2> right => new Right<T1B, T2>(right.Value),
            _ => throw new InvalidOperationException(),
        };
    [DebuggerStepThrough]public MyEither<T1, T2> Pure1(T1 value) => new Left<T1, T2>(value);
    [DebuggerStepThrough]public MyEither<T1B, T2> Apply1<T1B>(MyEither<Func<T1, T1B>, T2> f)
        => this switch
        {
            Left<T1, T2> leftK when f is Left<Func<T1, T1B>, T2> leftF =>
                new Left<T1B, T2>(leftF.Value(leftK.Value)),
            Right<T1, T2> rightK => new Right<T1B, T2>(rightK.Value),
            _ => throw new InvalidOperationException(),
        };
    [DebuggerStepThrough]public MyEither<T1B, T2> Bind1<T1B>(Func<T1, MyEither<T1B, T2>> f)
        => this switch
        {
            Left<T1, T2> left => f(left.Value),
            Right<T1, T2> right => new Right<T1B, T2>(right.Value),
            _ => throw new InvalidOperationException(),
        };
    [DebuggerStepThrough]public void MatchA(Action<T1> left, Action<T2>? right = null)
    {
        switch (this)
        {
            case Left<T1, T2> l:
                left.Invoke(l.Value);
                break;
            case Right<T1, T2> r:
                right?.Invoke(r.Value);
                break;
        }
    }
    [DebuggerStepThrough]public TR Match<TR>(Func<T1, TR> left, Func<T2, TR> right)
        => this switch
        {
            Left<T1, T2> l => left.Invoke(l.Value),
            Right<T1, T2> r => right.Invoke(r.Value),
            _ => throw new InvalidOperationException()
        };
    
    [DebuggerStepThrough]public MyEither<T1B, T2> Select<T1B>(Func<T1, T1B> f) => Map1(f);
    [DebuggerStepThrough]public MyEither<T1C, T2> SelectMany<T1B, T1C>(Func<T1, MyEither<T1B, T2>> f, Func<T1, T1B, T1C> s) =>
        Bind1(a => f(a).Map1(b => s(a, b)));
}
public record Left<T1, T2>(T1 Value) : MyEither<T1, T2>;
public record Right<T1, T2>(T2 Value) : MyEither<T1, T2>;