global using static GeneralPreview.MyPrelude;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;

namespace GeneralPreview;
public record Unit;
public static class MyPrelude 
{
    public static readonly Unit None = new ();
    [DebuggerStepThrough] public static UniTask RTask() => UniTask.CompletedTask;
    [DebuggerStepThrough] public static UniTask RTask<T>(T _) => UniTask.CompletedTask;
    [DebuggerStepThrough] public static int RZero() => 0;
    [DebuggerStepThrough] public static bool RTrue() => true;
    [DebuggerStepThrough] public static bool RFalse() => false;
    [DebuggerStepThrough] public static T Rid<T>(T x) => x;
    [DebuggerStepThrough] public static string RStr() => string.Empty;
    [DebuggerStepThrough] public static List<T> RList<T>() => [];
    
    
    public static Func<T0, T2> Compose<T0, T1, T2>(Func<T1, T2> f1, Func<T0, T1> f0) =>
        x => f1(f0(x));
    public static Action<T0> ComposeA<T0, T1>(Action<T1> f1, Func<T0, T1> f0) =>
        x => f1(f0(x));
    
    // public static readonly Func<MyIO<string>, string> RunStrIO = io => io.Run();
    // public static readonly Func<MyIO<int>, int> RunIntIO = io => io.Run();
}