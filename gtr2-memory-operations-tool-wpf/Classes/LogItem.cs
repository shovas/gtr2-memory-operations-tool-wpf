using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool
{
    public class LogItem
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public string Message = "";
        public Log.LogLevel LogLevel;
        public LogItem(DateTime timestamp, string message, Log.LogLevel logLevel)
        {
            Timestamp = timestamp;
            Message = message;
            LogLevel = logLevel;
        }
    }
}
