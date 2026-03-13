using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool
{
    public class LogItem
    {
        public string Message = "";
        public Log.LogLevel LogLevel;
        public LogItem(string message, Log.LogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }
    }
}
