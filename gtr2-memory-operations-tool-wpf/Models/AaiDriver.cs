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
        private int _vehicleSlotId = -1;
        public int VehicleSlotId { get => _vehicleSlotId; set { _vehicleSlotId = value; OnPropertyChanged(); }  }
        private bool _isPlayer = false;
        public bool IsPlayer { get => _isPlayer; set { _isPlayer = value; OnPropertyChanged(); } }
        private string _name = "";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        //public string Value { get; set; } = "";
        private int _place = 0;
        public int Place { get => _place; set { _place = value; OnPropertyChanged(); } }
        private int _totalLaps = 0;
        /// <summary>
        /// Gets or sets the number of completed laps. When lap count changes, Total Laps is the lap number of the lap just completed.
        /// </summary>
        /// <remarks>Non-negative count of completed laps. Defaults to 0.</remarks>
        public int TotalLaps { get => _totalLaps; set { _totalLaps = value; OnPropertyChanged(); } } 
        private float _bestLapTime = 0.0f;
        public float BestLaptime {
            get => _bestLapTime;
            set {
                _bestLapTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BestLaptimeFormatted));
            }
        }
        public string BestLaptimeFormatted { get { return FormatLaptime(BestLaptime); } }
        private float _lastLaptime = 0.0f;
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
        public string LastLaptimeFormatted { get { return FormatLaptime(LastLaptime); } }
        private List<Gtr2Lap> _laptimes = [];
        public List<Gtr2Lap> Laptimes { get => _laptimes; set { _laptimes = value; OnPropertyChanged(); } }
        private float _weightPenalty = 0.0f;
        public float WeightPenalty { get => _weightPenalty; set { _weightPenalty = value; OnPropertyChanged(); } }
        /// <summary>
        /// The completed lap BOP was last done for. Should match Total Laps at time of BOP.
        /// </summary>
        /// <remarks>Defaults to 0 if no BOP done yet.</remarks>
        private int _bopLap = 0;
        public int BopLap { get => _bopLap; set { _bopLap = value; OnPropertyChanged(); } }
        private float _bopBestLaptime = 0.0f;
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
        public string BopBestLaptimeFormatted {  get { return FormatLaptime(BopBestLaptime);  } }
        private float _bopProjectedLaptime = 0.0f;
        public float BopProjectedLaptime { get => _bopProjectedLaptime; set { _bopProjectedLaptime = value; OnPropertyChanged(); } }
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
    }
}
