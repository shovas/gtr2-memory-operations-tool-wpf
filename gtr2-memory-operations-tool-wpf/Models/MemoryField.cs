using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryField(string name, Type heldType, int length, int offset)
    {
        public string Name { get; set; } = name;
        public Type HeldType { get; set; } = heldType; // Can be determined with typeof(Type) eg. typeof(int) for int, typeof(List<string>) for List<string>, etc.
        public int Length { get; set; } = length;
        public int OffsetStatic { get; set; } = offset;
    }
}
