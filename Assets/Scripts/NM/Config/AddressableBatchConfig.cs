using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace NM.Config;

public class AddressableBatchConfig : SerializedScriptableObject
{ 
    // [Button("Open Batch Window", ButtonSizes.Large), PropertyOrder(-1)]
    // void OpenWindow()
    // {
    //     AddressableBatchProcessor.ShowWindowWithArg(this);
    // }

    [ReadOnly] public List<BatchRule> RuleList = [];
}