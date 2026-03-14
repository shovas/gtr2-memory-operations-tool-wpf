using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace Gtr2MemOpsTool
{
    public class AsyncBatchLogger
    {
        // Using a Channel to implement an async logging system that batches log entries and updates the UI at regular intervals without blocking the main thread.
        private readonly Channel<LogItem> _channel = Channel.CreateUnbounded<LogItem>();
        public ChannelReader<LogItem> Reader => _channel.Reader; // Expose the reader to allow the MainWindow to consume log items without exposing the writer, ensuring encapsulation and thread safety.

        public Log.LogLevel LoggingLevel { get; set; } = Gtr2MemOpsTool.Log.LogLevel.Debug;

        public AsyncBatchLogger(Log.LogLevel loggingLevel)
        {
            LoggingLevel = loggingLevel;
        }

        public void Log(LogItem logItem)
        {

            _channel.Writer.TryWrite(logItem);
        }

        public void Add(string message, Gtr2MemOpsTool.Log.LogLevel loggingLevel)
        {
            // Only for console apps I guess: Console.WriteLine(message);
            //Debug.WriteLine(message); // Argh. Debug.WriteLine is a synchronous blocking call and ridiculously slow. It freezes the UI. Don't use it.
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