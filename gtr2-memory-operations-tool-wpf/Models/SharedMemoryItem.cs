using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class SharedMemoryItem(string structName, string key, string value, string type)
    {
        public int Index { get; set; } = -1;
        public string StructName { get; set; } = structName;
        public string Key { get; set; } = key;
        public string Value { get; set; } = value;
        public string Type { get; set; } = type;
    }
}
