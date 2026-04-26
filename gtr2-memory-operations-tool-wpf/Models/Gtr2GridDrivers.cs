using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    /// <summary>
    /// Gtr2Grid represents the region of GTR2 memory containing Gtr2GridVehicleSlot regions
    /// This is based on the GridData class from TShirt's memops.py
    /// </summary>
    public class Gtr2GridDrivers(uint address)
    {
        public uint Address { get; set; } = address; // Memory address of the grid region from the base address of the GTR2 process
        public List<Gtr2GridDriver> Drivers { get; set; } = [];
    }
}
