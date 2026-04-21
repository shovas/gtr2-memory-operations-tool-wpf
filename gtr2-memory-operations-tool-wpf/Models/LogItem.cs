using Gtr2MemOpsTool.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class LogItem(DateTime timestamp, string message, Logger.LogLevel logLevel)
    {
        public DateTime Timestamp { get; set;  } = timestamp;
        public string Message { get; set; } = message;
        public Logger.LogLevel LogLevel { get; set; } = logLevel;
    }
}
