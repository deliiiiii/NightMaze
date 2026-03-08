using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace General
{
    [UnityEngine.CreateAssetMenu(fileName = nameof(MyDebugConfig), menuName = "General/" + nameof(MyDebugConfig))]
    [ReadOnly]
    public class MyDebugConfig : SerializedScriptableObject
    {
        [InfoBox("请到 Tools/General/MyDebugWindow 里修改日志设置")]
        public bool CanLogAll = true;
        public bool CanLog = true;
        public bool CanLogWarning = true;
        public bool CanLogError = true;
        
        public HashSet<LogType> ActiveLogTypes = new() { LogType.Default };

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