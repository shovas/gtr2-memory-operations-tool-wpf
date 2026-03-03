using System;
using System.Collections.Generic;
using System.Text;

namespace gtr2_memory_operations_tool_wpf
{
    public class SharedMemoryItem
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string Type { get; set; } = "";

        public SharedMemoryItem(string key, string value, string type)
        {
            Key = key;
            Value = value;
            Type = type;
        }

    }
}
