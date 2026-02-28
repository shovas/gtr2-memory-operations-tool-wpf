using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtr2_memops9
{
    internal class MemoryField
    {
        public string Name { get; set; }
        public Type HeldType { get; set; }
        public int Length { get; set; } // Length of byte run for this field in memory
        public int OffsetStatic { get; set; } // Offset from start of MemoryRegion address. Do not rely on this. Should be able to dynamically calculate this.
        public MemoryField(string name, Type heldType, int length, int offset)
        {
            Name = name;
            HeldType = heldType; // Can be determined with typeof(Type) eg. typeof(int) for int, typeof(List<string>) for List<string>, etc.
            Length = length;
            OffsetStatic = offset;
        }
    }
}
