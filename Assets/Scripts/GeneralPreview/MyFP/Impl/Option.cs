using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Newtonsoft.Json;

namespace GeneralPreview;
[Serializable, ShowInInspector, HideReferenceObjectPicker]
[JsonConverter(typeof(MyOptionJsonConverter))]
public abstract record MyOption<T1>
{
    [DebuggerStepThrough]
    public static implicit operator MyOption<T1>(T1 some)
    {
        if(some is null)
            return new MyNone<T1>();
        return new MySome<T1>(some);
    }
    public static readonly MyNone<T1> None = new();
    [DebuggerStepThrough]
    public static implicit operator MyOption<T1>(Unit _) => None;
    public bool IsSome => this is MySome<T1>;
    [DebuggerStepThrough] public void MatchA(Action<T1>? some = null, Action? none = null)
    {
        switch (this)
        {
            case MySome<T1> s:
                some?.Invoke(s.Value);
                break;
            case MyNone<T1>:
                none?.Invoke();
                break;
        }
    }
    
    
    [DebuggerStepThrough] public TR Match<TR>(Func<T1, TR> some, Func<TR> none)
        => this switch
        {
            MySome<T1> s => some.Invoke(s.Value),
            _ => none.Invoke()
        };
    [DebuggerStepThrough] public UniTask MatchAsync(Func<T1, UniTask> some, Func<UniTask> none)
        => this switch
        {
            MySome<T1> s => some.Invoke(s.Value),
            _ => none.Invoke()
        };
    [DebuggerStepThrough]public MyOption<T1B> Map<T1B>(Func<T1, T1B> f) 
        => this switch
        {
            MySome<T1> kSome => new MySome<T1B>(f(kSome.Value)),
            MyNone<T1> => new MyNone<T1B>(),
            _ => throw new InvalidOperationException()
        };
    [DebuggerStepThrough]public MyOption<T1> Pure(T1 value) => value;
    [DebuggerStepThrough]public MyOption<T1B> Apply<T1B>(MyOption<Func<T1, T1B>> f)
        => this switch
        {
            MySome<T1> kSome => f switch
            {
                MySome<Func<T1, T1B>> fSome => new MySome<T1B>(fSome.Value(kSome.Value)),
                MyNone<Func<T1, T1B>> => new MyNone<T1B>(),
                _ => throw new InvalidOperationException(),
            },
            MyNone<T1> => new MyNone<T1B>(),
            _ => throw new InvalidOperationException()
        };
    [DebuggerStepThrough]public MyOption<T1B> Bind<T1B>(Func<T1, MyOption<T1B>> f)
        => this switch
        {
            MySome<T1> kSome => f(kSome.Value),
            MyNone<T1> => new MyNone<T1B>(),
            _ => throw new InvalidOperationException()
        };
    
    
    [DebuggerStepThrough]public MyOption<T1B> Select<T1B>(Func<T1, T1B> f) => Map(f);
    [DebuggerStepThrough]public MyOption<T1C> SelectMany<T1B, T1C>(Func<T1, MyOption<T1B>> f, Func<T1, T1B, T1C> s) =>
        Bind(a => f(a).Map(b => s(a, b)));
}
[DebuggerStepThrough][Serializable]
public record MySome<T>([property: ShowInInspector] T Value) : MyOption<T>;

[DebuggerStepThrough][Serializable]
public record MyNone<T> : MyOption<T>;