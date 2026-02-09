global using static GeneralPreview.MyPrelude;
using System;

namespace GeneralPreview;
public record NoneClass;
public static class MyPrelude 
{
    public static readonly NoneClass None = new ();
    
    public static Func<T0, T2> Compose<T0, T1, T2>(Func<T1, T2> f1, Func<T0, T1> f0) =>
        x => f1(f0(x));
    public static Action<T0> ComposeA<T0, T1>(Action<T1> f1, Func<T0, T1> f0) =>
        x => f1(f0(x));
    
    // public static readonly Func<MyIO<string>, string> RunStrIO = io => io.Run();
    // public static readonly Func<MyIO<int>, int> RunIntIO = io => io.Run();
}