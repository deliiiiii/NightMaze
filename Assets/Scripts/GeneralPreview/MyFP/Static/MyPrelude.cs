global using static GeneralPreview.MyPrelude;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using General;

namespace GeneralPreview;
[DebuggerStepThrough]
public static class MyPrelude 
{
    public static readonly Unit None = new ();
    public static UniTask RTask() => UniTask.CompletedTask;
    public static UniTask RTask<T>(T _) => UniTask.CompletedTask;
    public static int RZero() => 0;
    public static bool RTrue() => true;
    public static bool RFalse() => false;
    public static T Rid<T>(T x) => x;
    public static string RStr() => string.Empty;
    public static List<T> RList<T>() => [];
    
    
    public static Func<T0, T2> Compose<T0, T1, T2>(Func<T1, T2> f1, Func<T0, T1> f0) =>
        x => f1(f0(x));
    public static Action<T0> ComposeA<T0, T1>(Action<T1> f1, Func<T0, T1> f0) =>
        x => f1(f0(x));
    
    // public static readonly Func<MyIO<string>, string> RunStrIO = io => io.Run();
    // public static readonly Func<MyIO<int>, int> RunIntIO = io => io.Run();
}