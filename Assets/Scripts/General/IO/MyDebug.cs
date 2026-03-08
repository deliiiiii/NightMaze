using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using Debug = UnityEngine.Debug;

namespace General
{
    public enum LogType
    {
        [LabelText("默认")]
        Default,
    }
    public static class MyDebug
    {
        static bool canLogAll = true;
        static bool canLog = true;
        static bool canLogWarning = true;
        static bool canLogError = true;
        static HashSet<LogType> logTypes = ((LogType[])Enum.GetValues(typeof(LogType))).ToHashSet();

        [UnityEngine.HideInCallstack][DebuggerStepThrough]
        public static void Log(object message, LogType logType = LogType.Default)
        {
            if (!canLog || !CheckLog(logType))
            {
                return;
            }
            Debug.Log(message);
        
        }
        [UnityEngine.HideInCallstack][DebuggerStepThrough]
        public static void LogWarning(object message, LogType logType = LogType.Default, int threshold = 0)
        {
            if (!canLogWarning || !CheckLog(logType))
            {
                return;
            }
            Debug.LogWarning(message);

        }
        [UnityEngine.HideInCallstack][DebuggerStepThrough]
        public static void LogError(object message, LogType logType = LogType.Default, int threshold = 0)
        {
            if (!canLogError || !CheckLog(logType))
            {
                return;
            }
            Debug.LogError(message);

        }
        [UnityEngine.HideInCallstack][DebuggerStepThrough]
        static bool CheckLog(LogType logType)
        {
            return canLogAll && logTypes.Contains(logType);
        }
        
        public static void ApplySettings(bool all, bool log, bool warning, bool error, HashSet<LogType> activeTypes)
        {
            canLogAll = all;
            canLog = log;
            canLogWarning = warning;
            canLogError = error;
            logTypes = activeTypes;
        }
    }
}