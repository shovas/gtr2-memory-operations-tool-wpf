using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using static Gtr2MemOpsTool.Log;

namespace Gtr2MemOpsTool
{
    public class AsyncBatchLogger
    {
        // Using a Channel to implement an async logging system that batches log entries and updates the UI at regular intervals without blocking the main thread.
        private readonly Channel<LogItem> _channel = Channel.CreateUnbounded<LogItem>();
        public ChannelReader<LogItem> Reader => _channel.Reader; // Expose the reader to allow the MainWindow to consume log items without exposing the writer, ensuring encapsulation and thread safety.

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Exception
        }
        /// <summary>
        ///  The effective logging level. Log entries with a level below this will be ignored. For example, if set to Warning, Debug and Info entries will be ignored, but Warning, Error and Exception entries will be logged.
        /// </summary>
        public LogLevel LoggingLevel { get; set; } = LogLevel.Debug;
        //public event Action<string, LogLevel>? EntryAdded;
        public AsyncBatchLogger(LogLevel loggingLevel)
        {
            LoggingLevel = loggingLevel;
        }

        public void Log(LogItem logItem)
        {

            _channel.Writer.TryWrite(logItem);
        }

        public void Add(string message, LogLevel loggingLevel)
        {
            // Only for console apps I guess: Console.WriteLine(message);
            //Debug.WriteLine(message); // Argh. Debug.WriteLine is a synchronous blocking call and ridiculously slow. It freezes the UI. Don't use it.
            DateTime logItemTs = DateTime.Now;
            //string logItemTsStr = logItemTs.ToString("yyyy-MM-dd HH:mm:ss");
            LogItem logItem = new LogItem(logItemTs, message, loggingLevel);
            _channel.Writer.TryWrite(logItem);
            //EntryAdded?.Invoke(message, loggingLevel);
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

        //public void AddException(Exception ex)
        //{
        //    Add($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}", LogLevel.Exception);
        //}

        public LogLevel GetLogLevel( string logLevelLabel ) {
            switch (logLevelLabel)
            {
                case "Debug":
                    return LogLevel.Debug;
                case "Info":
                    return LogLevel.Info;
                case "Warning":
                    return LogLevel.Warning;
                case "Error":
                    return LogLevel.Error;
                //case "Exception":
                //    return LogLevel.Exception;
                default:
                    App.Log.AddError($"Unknown log level label: {logLevelLabel}");
                    return LogLevel.Debug; // Safe default
            }
        }

        public string GetLogLevelLabel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Info:
                    return "Info";
                case LogLevel.Warning:
                    return "Warning";
                case LogLevel.Error:
                    return "Error";
                //case LogLevel.Exception:
                //    return "Exception";
                default:
                    App.Log.AddError($"Unknown log level: {logLevel}");
                    return "Unspecified";
            }
        }

    }
}