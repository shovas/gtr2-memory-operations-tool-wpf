using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace Gtr2MemOpsTool
{
    public class AsyncBatchLogger
    {
        private readonly Channel<LogItem> _channel;

        public Log.LogLevel LoggingLevel { get; set; }

        public AsyncBatchLogger() // Fake constructor for App.xaml.cs, will be replaced in OnStartup
        {
            _channel = Channel.CreateUnbounded<LogItem>();
            LoggingLevel = Gtr2MemOpsTool.Log.LogLevel.Debug;
        }   
        public AsyncBatchLogger(Channel<LogItem> channel, Log.LogLevel loggingLevel)
        {
            _channel = channel;
            LoggingLevel = loggingLevel;
        }

        public void Log(LogItem logItem)
        {

            _channel.Writer.TryWrite(logItem);
        }

        public void Add(string message, Gtr2MemOpsTool.Log.LogLevel loggingLevel)
        {
            // Only for console apps I guess: Console.WriteLine(message);
            Debug.WriteLine(message);
            LogItem logItem = new LogItem(message, loggingLevel);
            _channel.Writer.TryWrite(logItem);
        }

        public void AddDebug(string message)
        {
            Add(message, Gtr2MemOpsTool.Log.LogLevel.Debug);
        }

        public void AddInfo(string message)
        {
            Add(message, Gtr2MemOpsTool.Log.LogLevel.Info);
        }

        public void AddWarning(string message)
        {
            Add(message, Gtr2MemOpsTool.Log.LogLevel.Warning);
        }

        public void AddError(string message)
        {
            Add(message, Gtr2MemOpsTool.Log.LogLevel.Error);
        }

        public void AddException(Exception ex)
        {
            Add($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}", Gtr2MemOpsTool.Log.LogLevel.Exception);
        }

        public string GetLogLevelLabel(Log.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Gtr2MemOpsTool.Log.LogLevel.Debug:
                    return "Debug";
                case Gtr2MemOpsTool.Log.LogLevel.Info:
                    return "Info";
                case Gtr2MemOpsTool.Log.LogLevel.Warning:
                    return "Warning";
                case Gtr2MemOpsTool.Log.LogLevel.Error:
                    return "Error";
                case Gtr2MemOpsTool.Log.LogLevel.Exception:
                    return "Exception";
                default:
                    App.Log.AddError($"Unknown log level: {logLevel}");
                    return "Unspecified";
            }
        }

    }
}