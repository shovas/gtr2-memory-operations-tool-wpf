using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryRegion(MemoryItem[] fields)
    {
        public MemoryItem[] Fields { get; set; } = fields;

        public void AddField(MemoryItem field)
        {
            Type heldType = field.HeldType;
            int fieldSize = Marshal.SizeOf(heldType) * field.Length;
            int lastFieldOffset = ( Fields.Length > 0 ) ? Fields[^1].Offset : 0;
            int fieldOffset = lastFieldOffset + fieldSize;
            field.Offset = fieldOffset;
            _ = Fields.Append(field);
        }
    }
}
