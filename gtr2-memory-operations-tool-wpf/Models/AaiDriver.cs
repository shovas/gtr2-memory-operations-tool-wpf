using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class AaiDriver
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public float LastLaptime { get; set; } = 0;
        public string LastLaptimeFormatted {
            get
            {
                var ret = TimeSpan.FromSeconds(LastLaptime).ToString(@"mm\:ss\.fff");
                return ret;
            }
        }
    }
}
