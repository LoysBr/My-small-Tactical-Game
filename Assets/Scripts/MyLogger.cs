using System;
using UnityEngine;

public static class MyLogger
{
    private const string LOGLEVEL_STRING_TRACE = "[TRCE]";
    private const string LOGLEVEL_STRING_DEBUG = "[DBUG]";
    private const string LOGLEVEL_STRING_INFO = "[INFO]";
    private const string LOGLEVEL_STRING_WARNING = "[WARN]";
    private const string LOGLEVEL_STRING_ERROR = "[ERRO]";
    private const string LOGLEVEL_STRING_CRITICAL = "[CRIT]";

    public enum LogLevel
    {
        Trace,
        Debug, 
        Info, 
        Warning, 
        Error,
        Critical
    }

    public static Logger EnemiesLogger = new Logger(Debug.unityLogger.logHandler);
    public static Logger OtherCategoryLogger = new Logger(Debug.unityLogger.logHandler);

    private static bool m_logTimestamp = true;

    public static void Init()
    {
        //if a build is not a "development build" (so is a "production build"), disable all logs (still log in Editor)
        Debug.unityLogger.logEnabled = Debug.isDebugBuild;

        EnemiesLogger.logEnabled = true;
        OtherCategoryLogger.logEnabled = false;
        m_logTimestamp = true;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(string message, LogLevel logLevel)
    {
        string fullStr = message;

        if (m_logTimestamp) 
        {
            fullStr = $"{GetTimestampString()} - {message}";
        }

        switch (logLevel)
        {
            case LogLevel.Error:
            case LogLevel.Critical:
                {
                    Debug.LogError($"{GetLogLevelPrefix(logLevel)} - {fullStr}");
                    break;
                }
            case LogLevel.Warning:
                {
                    Debug.LogWarning($"{GetLogLevelPrefix(logLevel)} - {fullStr}");
                    break;
                }
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Info:
                {
                    Debug.Log($"{GetLogLevelPrefix(logLevel)} - {fullStr}");
                    break;
                }
            default:
                {
                    Debug.LogError($"Invalid {nameof(LogLevel)}: '{logLevel}'.");
                    break;
                }
        }
    }

    //Also writes in the build's Log file
    public static void LogEverywhere(string message, LogLevel logLevel)
    {
        string fullStr = message;

        if (m_logTimestamp)
        {
            fullStr = $"{GetTimestampString()} - {message}";
        }

        switch (logLevel)
        {
            case LogLevel.Error:
            case LogLevel.Critical:
                {
                    Debug.LogError($"{GetLogLevelPrefix(logLevel)} - {fullStr}");
                    break;
                }
            case LogLevel.Warning:
                {
                    Debug.LogWarning($"{GetLogLevelPrefix(logLevel)} - {fullStr}");
                    break;
                }
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Info:
                {
                    Debug.Log($"{GetLogLevelPrefix(logLevel)} - {fullStr}");
                    break;
                }
            default:
                {
                    Debug.LogError($"Invalid {nameof(LogLevel)}: '{logLevel}'.");
                    break;
                }
        }
    }

    private static string GetTimestampString()
    {
         return DateTime.UtcNow.ToString("hh:mm:ss:ff");
    }

    private static string GetLogLevelPrefix(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                {
                    return LOGLEVEL_STRING_TRACE;
                }
            case LogLevel.Debug:
                {
                    return LOGLEVEL_STRING_DEBUG;
                }
            case LogLevel.Info:
                {
                    return LOGLEVEL_STRING_INFO;
                }
            case LogLevel.Warning:
                {
                    return LOGLEVEL_STRING_WARNING;
                }
            case LogLevel.Error:
                {
                    return LOGLEVEL_STRING_ERROR;
                }
            case LogLevel.Critical:
                {
                    return LOGLEVEL_STRING_CRITICAL;
                }
            default:
                {
                    return String.Empty;
                }
        }
    }
}
