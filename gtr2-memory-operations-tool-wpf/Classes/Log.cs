using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace gtr2_memory_operations_tool_wpf
{
    public class Log
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Exception
        }
        public LogLevel LoggingLevel { get; set; }
        public event Action<string, LogLevel>? EntryAdded;
        public Log(LogLevel loggingLevel) {
            // Can't log from here because EntryAdded isn't set yet
            LoggingLevel = loggingLevel;
        }
        public void Add(string message, LogLevel level)
        {
            // Only for console apps I guess: Console.WriteLine(message);
            Debug.WriteLine(message);
            var logLevelLabel = GetLogLevelLabel(level);
            message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevelLabel}] {message}\n";
            EntryAdded?.Invoke(message, level);
        }

        public void AddDebug(string message)
        {
            Add(message, LogLevel.Debug);
        }

        public void AddInfo(string message)
        {
            Add(message, LogLevel.Info);
        }

        public void AddWarning(string message)
        {
            Add(message, LogLevel.Warning);
        }

        public void AddError(string message)
        {
            Add(message, LogLevel.Error);
        }

        public void AddException(Exception ex)
        {
            Add($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Exception);
        }

        public string GetLogLevelLabel(LogLevel logLevel) {
            switch (logLevel) {
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Info:
                    return "Info";
                case LogLevel.Warning:
                    return "Warning";
                case LogLevel.Error:
                    return "Error";
                default:
                    App.Log.AddError($"Unknown log level: {logLevel}");
                    return "Unspecified";
            }
        }
    }
}
