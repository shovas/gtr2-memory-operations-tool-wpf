using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class AaiDriver
    {
        public int VehicleSlotId { get; set; } = -1;
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public float LastLaptime { get; set; } = 0; // Watch out for -1 (no lap time yet). This is why right now they show up as 00:01:00 in the UI instead of 00:00:000.
        public string LastLaptimeFormatted {
            get
            {
                float lastLaptime = (float)Math.Round(LastLaptime, 3, MidpointRounding.ToEven); // Kind of surprised it's not ToPositiveInfinity but ToEven seems to match GTR2's Timing screen.
                var ts = TimeSpan.FromSeconds(lastLaptime);
                var ret = ts.ToString(@"mm\:ss\.fff");
                return ret;
            }
        }
    }
}
