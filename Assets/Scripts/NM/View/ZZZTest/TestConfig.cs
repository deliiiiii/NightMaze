using GeneralPreview;
using NM.Config;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NM.View.ZZZTest;

public class TestConfig : MonoBehaviour
{
    [Button]
    public void Test()
    {
        var grid = RefPoolMulti<GridConfig>.AcquireOne(c => c.ID == 1);
    }
}