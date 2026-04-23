using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NM.Config;

namespace NM.Data;

public partial class GamePlaying
{
    [Obsolete("清空属性")]
    [MuteActEvt]
    UniTask ClearPropAsync(EPropType propType, CancellationToken ct)
    {
        new ActChangeProp(this)
        {
            Delta = -GetProp(propType),
            PropType = propType
        }.Forget();
        return UniTask.CompletedTask;
    }
    [Obsolete("写入属性")]
    UniTask ChangePropAsync(EPropType propType, long delta, CancellationToken ct)
    {
        switch (propType)
        {
            case EPropType.Prop1: PropBody += delta; break;
            case EPropType.Prop2: PropSans += delta; break;
            case EPropType.Prop3: PropLore += delta; break;
            case EPropType.PropA1: PropLoyalty += delta; break;
            case EPropType.PropA2: PropHostility += delta; break;
            default: throw new ArgumentOutOfRangeException(nameof(propType), propType, null);
        }

        return UniTask.CompletedTask;
    }
}