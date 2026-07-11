using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using Debug = UnityEngine.Debug;

namespace General
{
    public enum ELogType
    {
        [LabelText("默认")]
        Default,
    }
    public enum ELogLevel
    {
        Info,
        Warning,
        Error,
    }
    [DebuggerStepThrough]
    public static class MyDebug
    {
        static bool canLogAll = true;
        static bool canLog = true;
        static bool canLogWarning = true;
        static bool canLogError = true;
        static HashSet<ELogType> logTypes = ((ELogType[])Enum.GetValues(typeof(ELogType))).ToHashSet();

        [UnityEngine.HideInCallstack]
        public static void Log(object message, ELogType eLogType = ELogType.Default)
        {
            if (!canLog || !CheckLog(eLogType))
            {
                return;
            }
            Debug.Log(message);
        }
        [UnityEngine.HideInCallstack]
        public static void LogWarning(object message, ELogType eLogType = ELogType.Default, int threshold = 0)
        {
            if (!canLogWarning || !CheckLog(eLogType))
            {
                return;
            }
            Debug.LogWarning(message);
        }
        [UnityEngine.HideInCallstack]
        public static void LogError(object message, ELogType eLogType = ELogType.Default, int threshold = 0)
        {
            if (!canLogError || !CheckLog(eLogType))
            {
                return;
            }
            Debug.LogError(message);
        }
        [UnityEngine.HideInCallstack]
        static bool CheckLog(ELogType eLogType) => canLogAll && logTypes.Contains(eLogType);

        public static void ApplySettings(bool all, bool log, bool warning, bool error, HashSet<ELogType> activeTypes)
        {
            canLogAll = all;
            canLog = log;
            canLogWarning = warning;
            canLogError = error;
            logTypes = activeTypes;
        }
    }
}