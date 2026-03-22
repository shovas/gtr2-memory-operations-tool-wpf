using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool
{
    public class SharedMemoryItem
    {
        public int Index { get; set; } = -1;
        public string StructName { get; set; } = "";
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string Type { get; set; } = "";

        public SharedMemoryItem(string structName, string key, string value, string type)
        {
            StructName = structName;
            Key = key;
            Value = value;
            Type = type;
        }

    }
}
