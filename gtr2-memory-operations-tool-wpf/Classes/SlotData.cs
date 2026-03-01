using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtr2_memory_operations_tool_wpf.Classes
{
    internal class SlotData
    {
        public int SlotId { get; set; }
        public string DriverName { get; set; }
        public float WeightPenalty { get; set; }
        public string CarFilePath { get; set; }
        public List<string> CarParts { get; set; } = new List<string>();

        public SlotData()
        {
            SlotId = 0;
            DriverName = string.Empty;
            WeightPenalty = 0;
            CarFilePath = string.Empty;
        }

    }
}
