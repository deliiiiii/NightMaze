using Sirenix.OdinInspector;

namespace NM.Config;

public enum ERarity
{
    [LabelText("普通")] Common,
    [LabelText("罕见")]UnCommon,
    [LabelText("稀有")]Rare,
    [LabelText("非常稀有")]VeryRare,
}