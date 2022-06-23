using System;
using System.Diagnostics;

[DebuggerStepThrough]
public static class Log
{
    public static void ForceTrace(object message)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Log, message);
    }

    public static void ForceTrace(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Log, message, context);
    }

    public static void ForceTraceFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Log, format, args);
    }

    public static void ForceTraceFormat(UnityEngine.Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Log, context, format, args);
    }

    [Conditional("UNITY_LOG_TRACE")]
    public static void Trace(object message)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Log, message);
    }

    [Conditional("UNITY_LOG_TRACE")]
    public static void Trace(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Log, message, context);
    }

    [Conditional("UNITY_LOG_TRACE")]
    public static void TraceFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Log, format, args);
    }

    [Conditional("UNITY_LOG_TRACE")]
    public static void TraceFormat(UnityEngine.Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Log, context, format, args);
    }

    [Conditional("UNITY_LOG_ERROR")]
    public static void Error(object message)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Error, message);
    }

    [Conditional("UNITY_LOG_ERROR")]
    public static void Error(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Error, message, context);
    }

    [Conditional("UNITY_LOG_ERROR")]
    public static void ErrorFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Error, format, args);
    }

    [Conditional("UNITY_LOG_ERROR")]
    public static void ErrorFormat(UnityEngine.Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Error, context, format, args);
    }

    [Conditional("UNITY_LOG_EXCEPTION")]
    public static void Exception(Exception exception)
    {
        UnityEngine.Debug.unityLogger.LogException(exception, null);
    }

    [Conditional("UNITY_LOG_EXCEPTION")]
    public static void Exception(Exception exception, UnityEngine.Object context)
    {
        UnityEngine.Debug.unityLogger.LogException(exception, context);
    }

    [Conditional("UNITY_LOG_WARNING")]
    public static void Warning(object message)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Warning, message);
    }

    [Conditional("UNITY_LOG_WARNING")]
    public static void Warning(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Warning, message, context);
    }

    [Conditional("UNITY_LOG_WARNING")]
    public static void WarningFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Warning, format, args);
    }

    [Conditional("UNITY_LOG_WARNING")]
    public static void WarningFormat(UnityEngine.Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Warning, context, format, args);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, (object)"Assertion failed");
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, UnityEngine.Object context)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, (object)"Assertion failed", context);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, object message)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, message);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, string message)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, (object)message);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, object message, UnityEngine.Object context)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, message, context);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool condition, string message, UnityEngine.Object context)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, (object)message, context);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Assert, format, args);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void AssertFormat(bool condition, UnityEngine.Object context, string format, params object[] args)
    {
        if (condition)
            return;
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Assert, context, format, args);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertion(object message)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, message);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertion(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Assert, message, context);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertionFormat(string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Assert, format, args);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public static void LogAssertionFormat(UnityEngine.Object context, string format, params object[] args)
    {
        UnityEngine.Debug.unityLogger.LogFormat(UnityEngine.LogType.Assert, context, format, args);
    }
}