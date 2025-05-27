using System;
using System.IO;
using Game.Modules.Log.Domain;

namespace Game.Modules.Log.Infrastructure
{
    public class FileLogService : ILogService
    {
        private string logFilePath;

        public FileLogService(string path)
        {
            logFilePath = path;
        }

        public void Log(LogLevel level, string message)
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(logFilePath, logLine + Environment.NewLine);
#if UNITY_EDITOR
            UnityEngine.Debug.Log(logLine);
#endif
        }
    }
}