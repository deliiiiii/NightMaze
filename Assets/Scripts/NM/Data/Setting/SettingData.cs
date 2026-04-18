using System;
using DG.Tweening;
using General;
using Newtonsoft.Json;
using UnityEngine;

namespace NM.Data;
[Serializable]
public class SettingData
{
    public const float MouseScrollMapSpeedMin = 0.2f;
    public const float MouseScrollMapSpeedMax = 2f;

    public float MouseScrollMapSpeed
    {
        get;
        set => field = Math.Clamp(value, MouseScrollMapSpeedMin, MouseScrollMapSpeedMax);
    } = 1f;

    [field: SerializeReference][JsonProperty(IsReference = false)]
    public SpinTweenSpeedBase SpinTweenSpeed
    {
        get;
        set
        {
            field = value;
            DOTween.timeScale = value switch
            {
                SpinTweenSpeedImmediate => 1,
                SpinTweenSpeedNormal normal => normal.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
    } = new SpinTweenSpeedNormal() { Value = 1 };

    public abstract class SpinTweenSpeedBase;
    public class SpinTweenSpeedNormal : SpinTweenSpeedBase
    {
        public int Value
        {
            get;
            init
            {
                field = Math.Clamp(value, 1, 4);
                DOTween.timeScale = field;
            }
        } = 1;
    }
    public class SpinTweenSpeedImmediate : SpinTweenSpeedBase;
}
