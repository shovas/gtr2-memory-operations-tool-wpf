using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtr2_memory_operations_tool_wpf.Classes
{
    /// <summary>
    /// Grid Data represents a region of GTR2 memory containing various memory regions of interest
    /// </summary>
    internal class GridData
    {
        public  int NumVeh { get; set; }
        public  List<SlotData> Slots { get; set; }

        public GridData()
        {
            NumVeh = 0;
            Slots = [];
            //Slots = new List<SlotData> { };
        }

    }
}
