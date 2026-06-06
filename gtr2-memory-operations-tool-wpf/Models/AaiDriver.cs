using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using Gtr2MemOpsTool.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class AaiDriver : ObservableObject
    {
        public static readonly int PlayerVehicleSlotId = 0; // Driver 0 is always the player driver as that's the first grid slot and the player is always in the first grid slot
        public int VehicleSlotId { get => _vehicleSlotId; set { _vehicleSlotId = value; OnPropertyChanged(); }  }
        private int _vehicleSlotId = -1;
        public bool IsPlayer { get => _isPlayer; set { _isPlayer = value; OnPropertyChanged(); } }
        private bool _isPlayer = false;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private string _name = "";
        //public string Value { get; set; } = "";
        public int Place { get => _place; set { _place = value; OnPropertyChanged(); } }
        private int _place = 0;
        /// <summary>
        /// Gets or sets the number of completed laps. When lap count changes, Total Laps is the lap number of the lap just completed.
        /// </summary>
        /// <remarks>Non-negative count of completed laps. Defaults to 0.</remarks>
        public int TotalLaps { get => _totalLaps; set { _totalLaps = value; OnPropertyChanged(); } }
        private int _totalLaps = 0;
        public float BestLaptime {
            get => _bestLapTime;
            set {
                _bestLapTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BestLaptimeFormatted));
            }
        }
        private float _bestLapTime = 0.0f;
        public string BestLaptimeFormatted { get { return FormatLaptime(BestLaptime); } }
        /// <summary>
        ///  Watch out for -1 (no lap time yet). This is why right now they show up as 00:01:00 in the UI instead of 00:00:000.
        /// </summary>
        public float LastLaptime {
            get => _lastLaptime;
            set {
                _lastLaptime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastLaptimeFormatted));
            }
        }
        private float _lastLaptime = 0.0f;
        public string LastLaptimeFormatted { get { return FormatLaptime(LastLaptime); } }
        public List<Gtr2Lap> Laptimes { get => _laptimes; set { _laptimes = value; OnPropertyChanged(); } }
        private List<Gtr2Lap> _laptimes = [];
        public float WeightPenalty { get => _weightPenalty; set { _weightPenalty = value; OnPropertyChanged(); } }
        private float _weightPenalty = 0.0f;
        /// <summary>
        /// The completed lap BOP was last done for. Should match Total Laps at time of BOP.
        /// </summary>
        /// <remarks>Defaults to 0 if no BOP done yet.</remarks>
        public int BopLap { get => _bopLap; set { _bopLap = value; OnPropertyChanged(); } }
        private int _bopLap = 0;
        /// <summary>
        /// Best lap time, in seconds, recorded after the most recent balance-of-performance (BOP) adjustment.
        /// </summary>
        /// <remarks>Defaults to 0 if no lap has been recorded since the last BOP.</remarks>
        public float BopBestLaptime {
            get => _bopBestLaptime;
            set {
                _bopBestLaptime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BopBestLaptimeFormatted));
            }
        }
        private float _bopBestLaptime = 0.0f;
        public string BopBestLaptimeFormatted {  get { return FormatLaptime(BopBestLaptime);  } }
        public float BopProjectedLaptime { get => _bopProjectedLaptime; set { _bopProjectedLaptime = value; OnPropertyChanged(); } }
        private float _bopProjectedLaptime = 0.0f;
        public static string FormatLaptime(float laptime)
        {
            laptime = (float)Math.Round(laptime, 3, MidpointRounding.ToEven); // Kind of surprised it's not ToPositiveInfinity (ie. half up) but ToEven seems to match GTR2's Timing screen.
            var tsLaptime = TimeSpan.FromSeconds(laptime);
            var formattedLaptime = tsLaptime.ToString(@"mm\:ss\.fff");
            return formattedLaptime;
        }
        /// <summary>
        /// Notifies that all observable properties have changed by raising PropertyChanged with an empty property name.
        /// </summary>
        /// <remarks>An empty property name indicates that all properties should be treated as changed;
        /// listeners should refresh all bindings.</remarks>
        public void RefreshAllBindings()
        {
            OnPropertyChanged(string.Empty); // empty string = "all properties changed"
        }
        /// <summary>
        /// Return the best laptime since the last BOP lap
        /// </summary>
        /// <returns></returns>
        public float BestBopLaptime()
        {
            float bestLaptime = BestLaptime;
            if (bestLaptime < 0)
            {
                return bestLaptime;
            }
            if (BopLap > 0)
            {
                BopBestLaptime = bestLaptime;
                var bopLaptimes = Laptimes.Where(lap => lap.Lap > BopLap);
                if (bopLaptimes.Any())
                {
                    var newBestLaptime = bopLaptimes.Min(lap => lap.Laptime);
                    if (bestLaptime != newBestLaptime)
                    {
                        //AddLogItem($"Found new best laptime for player driver {playerDriver.Name} (P{playerDriver.Place}): BopLap={playerDriver.BopLap}, old best={playerBestLaptime}, new best={newBestLaptime}", Logger.LogLevel.Debug);
                        bestLaptime = newBestLaptime;
                        BopBestLaptime = bestLaptime;
                    }
                }
            }
            return BopBestLaptime;
        }
    }
}
