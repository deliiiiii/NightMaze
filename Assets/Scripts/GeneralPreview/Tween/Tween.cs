using System;
using System.Numerics;
using System.Threading;
using General;


namespace GeneralPreview;

public static class Tween
{
    public static void Bind<T>(
        Func<T> curGetter, Action<T> curSetter, 
        Func<T> tarGetter,
        float duration, EaseFunc<T> easeFunc, 
        CancellationToken ct)
    {
        var f = Action;
        f.ToBinder().Bind(ct);
        return;
        void Action(float dt)
        {            
            if (duration <= 0)
            {
                curSetter(tarGetter());
                return;
            }
            var t = dt / duration;
            if (t >= 1)
            {
                curSetter(tarGetter());
                return;
            }
            curSetter(easeFunc(curGetter(), tarGetter(), t));
        }
    }
    
}
public delegate T EaseFunc<T>(T start, T end, float t);

public static class EaseMethods
{
    public static float Linear(float s, float e, float t)
        => s + (e - s) * t;
    public static Vector3 QuadOut(Vector3 s, Vector3 e, float t) 
    {
        t = 1 - (1 - t) * (1 - t);
        return Vector3.Lerp(s, e, t);
    }
}