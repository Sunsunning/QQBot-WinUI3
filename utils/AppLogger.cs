using System;
using System.Diagnostics;
using System.IO;

namespace QQBotCodePlugin.utils
{
    public class AppLogger
    {
        private string LogDirectory { get; }
        private string LogFileName { get; set; }

        public AppLogger(string logDirectory)
        {
            LogDirectory = logDirectory;
            CreateNewLogFile();
        }

        private void CreateNewLogFile()
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            LogFileName = $"log-{timeStamp}.txt";
            string fullPath = Path.Combine(LogDirectory, LogFileName);
            Directory.CreateDirectory(LogDirectory);

            File.Create(fullPath).Dispose();
        }

        public void Log(string message, bool includeStackTrace = false, StackTrace stackTrace = null)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

            if (includeStackTrace && stackTrace == null)
            {
                throw new ArgumentNullException(nameof(stackTrace), "StackTrace cannot be null when includeStackTrace is true.");
            }

            if (includeStackTrace)
            {
                logMessage += Environment.NewLine + GetStackTrace(stackTrace);
            }

            File.AppendAllText(Path.Combine(LogDirectory, LogFileName), logMessage + Environment.NewLine);
        }

        private string GetStackTrace(StackTrace stackTrace)
        {
            if (stackTrace.FrameCount > 0)
            {
                StackFrame frame = stackTrace.GetFrame(1); // GetFrame(0) is the Log method itself
                string file = frame.GetFileName();
                string methodName = frame.GetMethod().Name;
                int lineNumber = frame.GetFileLineNumber();
                return $"{file} - {methodName}:{lineNumber}";
            }
            return "No stack trace available";
        }
    }
}
