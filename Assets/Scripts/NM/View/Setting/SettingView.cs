using GeneralPreview;
using NM.Data;
using Sirenix.OdinInspector;

namespace NM.View;

public class SettingView : ViewBase
{
    [OnValueChanged(nameof(OnSpeedChanged)), PropertyRange(1, 5)] public int Speed;

    void OnSpeedChanged()
    {
        GameRoot.Setting.SpinTweenSpeed = Speed switch
        {
            >= 1 and <= 4 => new SettingData.SpinTweenSpeedNormal() { Value = Speed },
            _ => new SettingData.SpinTweenSpeedImmediate()
        };
    }
    
}