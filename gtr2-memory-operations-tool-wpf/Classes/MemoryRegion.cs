using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gtr2_memops9
{
    internal class MemoryRegion
    {
        public MemoryField[] Fields { get; set; }
        public MemoryRegion(MemoryField[] fields)
        {
            Fields = fields;
        }
        public void AddField(MemoryField field)
        {
            Type heldType = field.HeldType;
            int fieldSize = Marshal.SizeOf(heldType) * field.Length;
            int lastFieldOffset = ( Fields.Length > 0 ) ? Fields[Fields.Length - 1].OffsetStatic : 0;
            int fieldOffset = lastFieldOffset + fieldSize;
            field.OffsetStatic = fieldOffset;
            Fields.Append(field);
        }
    }
}
