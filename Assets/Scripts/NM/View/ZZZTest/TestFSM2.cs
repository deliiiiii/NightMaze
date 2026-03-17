using GeneralPreview;
using NM.Config;
using NM.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View.ZZZTest;

public class TestFSM2 : MonoBehaviour
{
    [Button]
    public void Test()
    {
        var config = RefPoolMulti<SymbolConfig>.AcquireOne(_ => true);
        var com = Factory<SymbolConfig, SymbolData.ICom>.Create(config);
    }
}