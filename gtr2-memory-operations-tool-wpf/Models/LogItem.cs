using Gtr2MemOpsTool.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class LogItem(DateTime timestamp, string message, AsyncBatchLogger.LogLevel logLevel)
    {
        public DateTime Timestamp { get; } = timestamp;
        public string Message { get; set; } = message;
        public AsyncBatchLogger.LogLevel LogLevel { get; set; } = logLevel;
    }
}
