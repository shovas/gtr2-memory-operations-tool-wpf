using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    /// <summary>
    /// Gtr2Grid represents the region of GTR2 memory containing Gtr2GridVehicleSlot regions
    /// This is the GridData class from TShirt's memops.py
    /// </summary>
    public class Gtr2TestGrid
    {
        public int NumVeh { get; set; } // Number of vehicles currently on the grid. Can this be different than Slots.Count?
        public List<Gtr2TestGridVehicleSlot> Slots { get; set; }

        public Gtr2TestGrid()
        {
            NumVeh = 0;
            Slots = [];
        }

    }
}
