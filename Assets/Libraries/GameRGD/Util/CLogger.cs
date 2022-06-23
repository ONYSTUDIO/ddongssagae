#pragma warning disable 0162

using System;
using System.Diagnostics;
using System.IO;
using static DoubleuGames.GameRGD.CPreferences;

namespace DoubleuGames.GameRGD
{
    public class CLogger
    {
        private static string systemPath;
        private static string fileName;

        public static void SetFilePath(string path)
        {
            systemPath = path;
        }

        public static void LogWarning(string tag, string message)
        {
            if (!EnableLogByTargetServer()) return;

            System.DateTime time = System.DateTime.Now;

            string logMessage = CreateLogMessage(tag, message, time);
            UnityEngine.Debug.LogWarning(logMessage);

            CreateLogFile(logMessage, time);
        }

        public static void LogError(string tag, Exception e)
        {
            if (!EnableLogByTargetServer()) return;

            System.DateTime time = System.DateTime.Now;

            string logMessage = CreateLogMessage(tag, e.Message, time);
            UnityEngine.Debug.LogError(logMessage);

            CreateLogFile(logMessage, time);
        }

        public static void LogError(string tag, string message, UnityEngine.Object context)
        {
            if (!EnableLogByTargetServer()) return;

            System.DateTime time = System.DateTime.Now;

            string logMessage = CreateLogMessage(tag, message, time);
            UnityEngine.Debug.LogError(logMessage, context);

            CreateLogFile(logMessage, time);
        }

        public static void LogError(string tag, string message)
        {
            if (!EnableLogByTargetServer()) return;

            System.DateTime time = System.DateTime.Now;

            string logMessage = CreateLogMessage(tag, message, time);
            UnityEngine.Debug.LogError(logMessage);

            CreateLogFile(logMessage, time);
        }

        public static void Log(string tag, string message)
        {
            if (!EnableLogByTargetServer()) return;

            System.DateTime time = System.DateTime.Now;
            string logMessage = CreateLogMessage(tag, message, time);

            UnityEngine.Debug.Log(logMessage);

            CreateLogFile(logMessage, time);
        }

        private static string CreateLogMessage(string tag, string message, System.DateTime time)
        {
            // string logMessage = string.Format("[{0,-21}]\t[{1}] : {2}", (tag.Length > 21 ? tag.Substring(0, 21) : tag), time, message);
            string logMessage = $"[{tag}] {message}";
            return logMessage;
        }

        public static void CreateLogFile(string logMessage, System.DateTime time)
        {
            if (CGameConstants.USE_FILE_LOG)
            {
                if (string.IsNullOrEmpty(systemPath))
                {
                    return;
                }

                string savePath = string.Format("{0}/{1}", systemPath, CGameConstants.FILE_LOG_PATH);
                if (string.IsNullOrEmpty(savePath) == false)
                {
                    if (Directory.Exists(savePath) == false)
                    {
                        try
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        catch (System.ArgumentException e)
                        {
                            UnityEngine.Debug.Log("CLogger/ Occur Exception : " + e.Message);
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = string.Format("log_{0}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}.txt", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                    }

                    string fullPath = string.Format("{0}/{1}", savePath, fileName);
                    File.AppendAllText(fullPath, string.Format("{0}\n", logMessage));
                }
            }
        }

        public static void Log(string format, params object[] param)
        {
            string logMessage = string.Format(format, param);
            Log(logMessage);
        }

        public static void LogWarning(string format, params object[] param)
        {
            string logMessage = string.Format(format, param);
            LogWarning(logMessage);
        }

        public static void LogError(string format, params object[] param)
        {
            string logMessage = string.Format(format, param);
            LogError(logMessage);
        }

        public static void Log(string message = "")
        {
            string className = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
            string methodName = new StackFrame(1, true).GetMethod().Name;
            Log($"{className}-{methodName}", message);
        }

        public static void LogWarning(string message = "")
        {
            string className = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
            string methodName = new StackFrame(1, true).GetMethod().Name;
            LogWarning($"{className}-{methodName}", message);
        }

        public static void LogError(string message = "")
        {
            string className = new StackTrace().GetFrame(1).GetMethod().ReflectedType.Name;
            string methodName = new StackFrame(1, true).GetMethod().Name;
            LogError($"{className}-{methodName}", message);
        }

        private static bool EnableLogByTargetServer()
        {
            // 의존성으로 인한 주석처리 / 추후 다른 방법으로 처리 요망
            // if (CApplicationManager.Instance == null)
            // {
            //     return true;
            // }

            if (CPreferences.TargetServer == SERVER.REAL)
            {
                return false;
            }

            return true;
        }
    }
}
