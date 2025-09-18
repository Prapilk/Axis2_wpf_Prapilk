using System;
using System.IO;

namespace Axis2.WPF.Services
{
    public static class Logger
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "axis2_wpf_debug.log");
        private static readonly object _lock = new object();
        public static event Action<string> OnLogMessage;

        public static void Init()
        {
            try
            {
                File.WriteAllText(logFilePath, string.Empty); // Clear log on start
            }
            catch { /* Ignore */ }
        }

        public static void Log(string message)
        {
            try
            {
                var formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                lock (_lock)
                {
                    File.AppendAllText(logFilePath, formattedMessage + "\n");
                }
                OnLogMessage?.Invoke(formattedMessage);
            }
            catch { /* Ignore */ }
        }
    }
}