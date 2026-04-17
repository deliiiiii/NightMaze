using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using General;
using GeneralPreview;
using JetBrains.Annotations;
using NM.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NM.View;

public class SettingView : ViewBase
{
    [SerializeField] Btn btnClose;
    [SerializeField] Slider sliderSpeed;
    readonly List<float> speedTar = [0, 0.25f, 0.5f, 0.75f, 1f];

    protected override IEnumerable<BindDataBase> BindList()
    {
        yield return btnClose.onClick.EvtBindTo(() => 
            Saver.SaveAsync(Const.Name.Save.SettingFolder, Const.Name.Save.SettingName, GameRoot.Setting).Forget());
    }

    void Start()
    {
        sliderSpeed.value = GameRoot.Setting.SpinTweenSpeed switch
        {
            SettingData.SpinTweenSpeedImmediate => 1f,
            SettingData.SpinTweenSpeedNormal normal => (normal.Value - 1) / 4f,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [PublicAPI]public void OnSliderValueChanged(float v)
    {
        // v和speedTar里的哪个数字最接近
        var nearest = (from near in speedTar
            select (near, Mathf.Abs(v - near))
            into pair
            orderby pair.Item2 ascending
            select pair.near).First();
        sliderSpeed.value = nearest;
        var tarSpeed = (int)(nearest * 4.1f) + 1;
        GameRoot.Setting.SpinTweenSpeed = tarSpeed switch
        {
            >= 1 and <= 4 => new SettingData.SpinTweenSpeedNormal { Value = tarSpeed },
            _ => new SettingData.SpinTweenSpeedImmediate()
        };
    }
}