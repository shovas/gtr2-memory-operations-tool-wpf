using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool
{
    public class LogItem
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string Message = "";
        public AsyncBatchLogger.LogLevel LogLevel;
        public LogItem(DateTime timestamp, string message, AsyncBatchLogger.LogLevel logLevel)
        {
            Timestamp = timestamp;
            Message = message;
            LogLevel = logLevel;
        }
    }
}
