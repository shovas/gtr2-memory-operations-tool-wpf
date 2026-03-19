using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool
{
    public class LogItem
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string Message { get; set; }
        public AsyncBatchLogger.LogLevel LogLevel { get; set; }
        public LogItem(DateTime timestamp, string message, AsyncBatchLogger.LogLevel logLevel)
        {
            Timestamp = timestamp;
            Message = message;
            LogLevel = logLevel;
        }
    }
}
