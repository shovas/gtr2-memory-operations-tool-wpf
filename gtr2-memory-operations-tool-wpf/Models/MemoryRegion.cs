using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryRegion(MemoryField[] fields)
    {
        public MemoryField[] Fields { get; set; } = fields;

        public void AddField(MemoryField field)
        {
            Type heldType = field.HeldType;
            int fieldSize = Marshal.SizeOf(heldType) * field.Length;
            int lastFieldOffset = ( Fields.Length > 0 ) ? Fields[^1].OffsetStatic : 0;
            int fieldOffset = lastFieldOffset + fieldSize;
            field.OffsetStatic = fieldOffset;
            _ = Fields.Append(field);
        }
    }
}
