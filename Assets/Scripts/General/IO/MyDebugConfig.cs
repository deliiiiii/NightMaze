using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace General
{
    [UnityEngine.CreateAssetMenu(fileName = nameof(MyDebugConfig), menuName = "General/" + nameof(MyDebugConfig))]
    [ReadOnly]
    public class MyDebugConfig : SerializedScriptableObject
    {
        [InfoBox("请到 Tools/General/MyDebugWindow 里修改日志设置")]
        [ReadOnly] public bool CanLogAll = true;
        [ReadOnly] public bool CanLog = true;
        [ReadOnly] public bool CanLogWarning = true;
        [ReadOnly] public bool CanLogError = true;
        
        public HashSet<ELogType> ActiveLogTypes = new() { ELogType.Default };

        void OnValidate()
        {
            ApplyToMyDebug();
        }

        void OnEnable()
        {
            ApplyToMyDebug();
        }

        public void ApplyToMyDebug()
        {
            MyDebug.ApplySettings(CanLogAll, CanLog, CanLogWarning, CanLogError, ActiveLogTypes);
        }
    }
}