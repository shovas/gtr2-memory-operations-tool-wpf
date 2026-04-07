using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryRegion(uint address, List<MemoryItem> fields)
    {
        public uint Address { get; set; } = address;
        public List<MemoryItem> Fields { get; set; } = fields;

        public void AddMemoryItem(MemoryItem field)
        {
            Fields.Add(field);
        }

        public void AddMemoryItems(List<MemoryItem> fields)
        {
            foreach (MemoryItem field in fields)
            {
                uint lastFieldOffset = (Fields.Count > 0) ? Fields[^1].Offset : 0;
                int lastFieldLength = (Fields.Count > 0) ? Fields[^1].Length : 0;
                uint fieldOffset = (uint)(lastFieldOffset + lastFieldLength);
                field.Offset = fieldOffset;
            }
            Fields.AddRange(fields);
        }

    }
}
