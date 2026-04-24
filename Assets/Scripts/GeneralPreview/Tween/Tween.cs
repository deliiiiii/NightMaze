using System;
using System.Threading;
using General;
using UnityEngine;


namespace GeneralPreview;

public static class Tween
{
    public static void Bind<T>(
        Func<T> curGetter, Action<T> curSetter, 
        Func<T> tarGetter,
        float duration, EaseFunc<T> easeFunc, 
        CancellationToken ct) where T : IEquatable<T>
    {
        float curDuration = 0;
        var curCur = curGetter();
        var curTar = tarGetter();
        var f = Action;
        f.ToBinder().Bind(ct);
        return;
        void Action(float dt)
        {
            if (duration <= 0)
            {
                curSetter(curTar);
                return;
            }
            var newTar = tarGetter();
            if (!newTar.Equals(curTar))
            {
                curDuration = 0;
                curCur = curGetter();
                curTar = newTar;
            }
            curDuration += dt;
            var percent = curDuration / duration;
            if (percent >= 1)
            {
                curSetter(curTar);
                return;
            }
            curSetter(easeFunc(curCur, curTar, percent));
        }
    }
    delegate float EaseCurve(float t);
    static readonly EaseCurve linearCurve = t => t;
    static readonly EaseCurve quadInCurve = t => t * t;
    static readonly EaseCurve quadOutCurve = t => 1 - (1 - t) * (1 - t);
    static readonly EaseCurve expoOutCurve = x => Mathf.Approximately(x, 1) ? 1 : 1 - Mathf.Pow(2, -10 * x);
    static readonly EaseCurve cubicOutCurve = x => 1 - Mathf.Pow(1 - x, 3);
    
    public delegate T EaseFunc<T>(T start, T end, float percent);
    public static EaseFunc<float> Linear = (s, e, t) => float.Lerp(s, e, linearCurve(t));
    public static EaseFunc<float> QuadIn = (s, e, t) => float.Lerp(s, e, quadInCurve(t));
    public static EaseFunc<float> QuadOut = (s, e, t) => float.Lerp(s, e, quadOutCurve(t));
    public static EaseFunc<float> ExpoOut = (s, e, t) => float.Lerp(s, e, expoOutCurve(t));
    public static EaseFunc<float> CubicOut = (s, e, t) => float.Lerp(s, e, cubicOutCurve(t));
}
