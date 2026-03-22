using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class SettingItem(string key, string value, string description)
    {
        public string Key { get; set; } = key;
        public string Value { get; set; } = value;
        public string Description { get; set; } = description;
    }
}
