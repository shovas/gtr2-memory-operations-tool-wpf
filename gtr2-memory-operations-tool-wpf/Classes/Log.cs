using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace gtr2_memory_operations_tool_wpf
{
    public class Log
    {
        public event Action<string>? EntryAdded;
        public Log() {
            // This won't work because EntryAdded isn't set yet
            //Add("GTR2 Memory Operations Tool Log\n");
        }
        public void Add(string message)
        {
            // Only for console apps I guess: Console.WriteLine(message);
            Debug.WriteLine(message);
            message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            EntryAdded?.Invoke(message);
        }
    }
}
