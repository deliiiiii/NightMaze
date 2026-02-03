using System.Collections.Generic;
using UnityEngine;
public enum LogType
{
    Default,
    Tick,
}
public static class MyDebug
{
    //设置输出严重度，输出类型，是否输出
    static bool canLogAll = true;
    static bool canLog = true;
    static bool canLogWarning = true;
    static bool canLogError = true;
    static int logThreshold = 10;
    static HashSet<LogType> logTypes = new()
    {
        LogType.Default,
        
        LogType.Tick,
        
    };
    [HideInCallstack]
    public static void Log(object message, LogType logType = LogType.Default, int threshold = 0)
    {
        if (!canLog || !CheckLog(logType, threshold))
        {
            return;
        }
        Debug.Log(message);
        
    }
    [HideInCallstack]
    public static void LogWarning(object message, LogType logType = LogType.Default, int threshold = 0)
    {
        if (!canLogWarning || !CheckLog(logType, threshold))
        {
            return;
        }
        Debug.LogWarning(message);

    }
    [HideInCallstack]
    public static void LogError(object message, LogType logType = LogType.Default, int threshold = 0)
    {
        if (!canLogError || !CheckLog(logType, threshold))
        {
            return;
        }
        Debug.LogError(message);

    }

    static bool CheckLog( LogType logType, int threshold)
    {
        if (!canLogAll || logThreshold < threshold || !logTypes.Contains(logType))
        {
            return false;
        }
        return true;
    }
}