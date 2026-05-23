using System;
using System.Collections.Generic;
using System.Text;
using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;

namespace Gtr2MemOpsTool.Models
{
    public class AaiDriver : ObservableObject
    {
        public static readonly int PlayerVehicleSlotId = 0; // Driver 0 is always the player driver as that's the first grid slot and the player is always in the first grid slot
        public int VehicleSlotId { get; set; } = -1;
        public bool IsPlayer { get; set; } = false;
        public string Name { get; set; } = "";
        //public string Value { get; set; } = "";
        public int Place { get; set; } = 0;
        public int TotalLaps { get; set; } = 0; // Laps completed
        public float BestLaptime { get; set; } = 0;
        public string BestLaptimeFormatted { get { return FormatLaptime(BestLaptime); } }
        public float LastLaptime { get; set; } = 0; // Watch out for -1 (no lap time yet). This is why right now they show up as 00:01:00 in the UI instead of 00:00:000.
        public string LastLaptimeFormatted { get { return FormatLaptime(LastLaptime); } }
        public List<Gtr2Lap> Laptimes { get; set; } = [];
        public float WeightPenalty { get; set; } = 0;
        public int BopLap { get; set; } = 0; // The lap bop was last done
        public float BopBestLaptime { get; set; } = 0; // Best laptime after the last BOP
        public string BopBestLaptimeFormatted {  get { return FormatLaptime(BopBestLaptime);  } }
        public float BopProjectedLaptime { get; set; } = 0;
        public static string FormatLaptime(float laptime)
        {
            laptime = (float)Math.Round(laptime, 3, MidpointRounding.ToEven); // Kind of surprised it's not ToPositiveInfinity (ie. half up) but ToEven seems to match GTR2's Timing screen.
            var tsLaptime = TimeSpan.FromSeconds(laptime);
            var formattedLaptime = tsLaptime.ToString(@"mm\:ss\.fff");
            return formattedLaptime;
        }
        public void RefreshBindings()
        {
            OnPropertyChanged(string.Empty); // empty string = "all properties changed"
        }
    }
}
