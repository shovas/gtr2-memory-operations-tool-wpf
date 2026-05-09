using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using Gtr2MemOpsTool.Services;
using Gtr2MemOpsTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for AutomaticAiView.xaml
    /// </summary>
    public partial class AutomaticAiView : UserControl
    {
        public BulkObservableCollection<AaiDriver> AaiDrivers { get; set; } = [];
        public BulkObservableCollection<LogItem> LogItems { get; set; } = [];

        private readonly List<AaiDriver> _aaiDrivers = [];

        private DispatcherTimer? _driversRefreshTimer;
        //private DispatcherTimer? _sharedMemoryRefreshTimer;
        //private readonly Gtr2SharMemOps _gtr2SharMemOps = new();
        private readonly Gtr2SharedMemoryWatcher _gtr2SharedMemoryWatcher = new();
        public AutomaticAiView()
        {
            InitializeComponent();
            DataContext = this;
            AddLogItem("Automatic AI tab starting...", Logger.LogLevel.Info);
            
            if ( Gtr2ProgMemOps.IsGtr2ProcessRunning())
            {
                AddLogItem("GTR2 process detected. Loading drivers...", Logger.LogLevel.Info);
                Activate();
            }
            else
            {
                AddLogItem("GTR2 process not detected. Please start GTR2 to load drivers.", Logger.LogLevel.Warning);
            }

            AddLogItem("Automatic AI tab started.", Logger.LogLevel.Info);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            RefreshDrivers();
            RefreshButton.IsEnabled = true;
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Reset());
        }

        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Activate());
        }

        private async void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Deactivate());
        }

        public async void OnGainFocus(object sender, RoutedEventArgs e)
        {
            //AddLogItem("Automatic AI tab gained focus. Activating...", Logger.LogLevel.Info);
            //Activate();
        }

        public async void OnLostFocus(object sender, RoutedEventArgs e)
        {
            AddLogItem("Automatic AI tab lost focus. Deactivating...", Logger.LogLevel.Info);
            Deactivate();
        }

        private void AddLogItem(string message, Logger.LogLevel logLevel)
        {
            LogItem logItem = new(DateTime.Now, message, logLevel);
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogItems.Add(logItem);
                LogListView.ScrollIntoView(LogItems.Last());
            });
        }

        private async void Reset()
        {
            AddLogItem("Reset()", Logger.LogLevel.Debug);
            Application.Current.Dispatcher.Invoke(() =>
            {
                AaiDrivers.Clear();
            });
        }

        private async void Activate()
        {
            AddLogItem("Activate()", Logger.LogLevel.Debug);
            Application.Current.Dispatcher.Invoke(() =>
            {
                //StartSharedMemoryRefreshTimer();
                _gtr2SharedMemoryWatcher.WatchGtr2SharedMemory();
                //_gtr2SharedMemoryWatcher.LaptimeChanged += (sender, e) => {
                //    Console.WriteLine(e.DriverName);
                //};
                _gtr2SharedMemoryWatcher.SessionChanged += OnSessionChanged;
                _gtr2SharedMemoryWatcher.GamePhaseChanged += OnGamePhaseChanged;
                _gtr2SharedMemoryWatcher.PlaceChanged += OnPlaceChanged;
                _gtr2SharedMemoryWatcher.LaptimeChanged += OnLaptimeChanged;

                StartDriversRefreshTimer();
            });
        }

        private async void Deactivate()
        {
            AddLogItem("Deactivate()", Logger.LogLevel.Debug);
            Application.Current.Dispatcher.Invoke(() =>
            {
                StopDriversRefreshTimer();
                _gtr2SharedMemoryWatcher.UnwatchGtr2SharedMemory();
                //StopSharedMemoryRefreshTimer();
            });
        }

        private void OnSessionChanged(object? sender, SessionChangedEventArgs e)
        {
            AddLogItem($"Session changed: {e.SessionName}", Logger.LogLevel.Info);
        }

        private void OnGamePhaseChanged(object? sender, GamePhaseChangedEventArgs e)
        {
            AddLogItem($"Game phase changed: {e.GamePhase}={e.GamePhaseName}", Logger.LogLevel.Info);
        }

        private void OnPlaceChanged(object? sender, PlaceChangedEventArgs e)
        {
            AddLogItem($"Place changed for driver {e.DriverName}: {e.Place}", Logger.LogLevel.Info);
            RecordDriverPlace(e.DriverName, e.Place);
        }

        private void OnLaptimeChanged(object? sender, LaptimeChangedEventArgs e)
        {
            AddLogItem($"Laptime changed for driver {e.DriverName}: {e.NewLapTime}", Logger.LogLevel.Info);
            RecordDriverLaptime(e.DriverName, e.NewLapTime);
            UpdateDriverWeightPenalty(e.DriverName);
        }

        private void RecordDriverPlace(string driverName, int newPlace)
        {
            var driver = _aaiDrivers.FirstOrDefault(d => d.Name == driverName);
            if (driver is not null)
            {
                driver.Place = newPlace;
            }
        }

        private void RecordDriverLaptime(string driverName, float newLapTime)
        {
            var driver = _aaiDrivers.FirstOrDefault(d => d.Name == driverName);
            driver?.Laptimes.Add(newLapTime);
        }

        private void UpdateDriverWeightPenalty(string driverName)
        {
            AaiDriver playerDriver = _aaiDrivers[0]; // Driver 0 is always the player driver as that's the first grid slot and the player is always in the first grid slot
            var targetDriver = _aaiDrivers.FirstOrDefault(d => d.Name == driverName);
            if (targetDriver is null)
            {
                AddLogItem($"Failed to find driver {driverName} to update weight penalty.", Logger.LogLevel.Warning);
                return;
            }

            // Calculate new weight penalties only compared to the player driver as we want to adjust AI based on the player's performance but we don't need to adjust the player based on AI performance
            if (targetDriver.Name == playerDriver.Name)
            {
                CalculateWeightPenalties(targetDriver);
            }
            
        }

        private void CalculateWeightPenalties(AaiDriver playerDriver)
        {
            // Continue only if we have enough laptimes recorded
            int minLaptimeCount = int.TryParse(App.Config.IniData.Sections["AutomaticAi"]["MinLaptimeCount"], out int minLaptimeCountResult) ? minLaptimeCountResult : 1;
            if (playerDriver.Laptimes.Count < minLaptimeCount)
            {
                return;
            }

            // Get all AI drivers excluding the player driver which is always the first
            List<AaiDriver> aiDrivers = _aaiDrivers.Slice(1, _aaiDrivers.Count-1);

            // We only need to calculate weight penalties against the player
            // - If player is in first place then we need AI to catch up and possibly we need to slow down the player if the AI can't catch up enough by just reducing their weight penalties
            float playerBestLaptime = playerDriver.Laptimes.Min();
            float weightPenaltyPerSecond = float.TryParse(App.Config.IniData.Sections["AutomaticAi"]["WeightPenaltyPerSecond"], out float weightPenaltyPerSecondResult) ? weightPenaltyPerSecondResult : 33.333333f;
            AddLogItem($"Adjusting weight penalties against player driver {playerDriver.Name} in place {playerDriver.Place} with best laptime {playerBestLaptime}.", Logger.LogLevel.Info);
            if ( playerDriver.Place == 1)
            {

                // Overview:
                // 1. Reduce P2 AI weight penaly
                // 2. Reduce weight penalties for the rest of the AI based on the percentage improvement of the first AI so they all maintain their relative gaps but also keep up to the driver ahead of them
                // 3. If P2 still isn't as fast as the player, add player weight penalty

                // Problem: We're applying weight penalties vs the player for all ai drivers which means they'll all end up competing against the player instead of in their relative positions
                // - Fix: Adjust the next placed AI driver by a calculated delta but the rest of the AI by a relative percentage so they maintain their relative performance gaps but all get closer to the player
                // - AI1 calculates weight penalty based on laptime delta to player and gets a weight penalty reduction based on that delta
                // - AI2+ get the percentage lift from AI1's laptime improvement so they all maintain their relative gaps but also keep up to the driver's ahead of them

                // First reduce any AI weight penalties
                // - Calculate AI laptime saved by the reduction to determine if we still need to add a weight penalty to the player after adjusting AI
                var aiDriversByPlace = aiDrivers.OrderBy(d => d.Place);
                AaiDriver p2AiDriver = aiDriversByPlace.First();

                // Determine best laptime and delta to player best laptime
                float p2AiBestLaptime = p2AiDriver.Laptimes.Min();
                float p2AiBestLaptimeDelta = p2AiBestLaptime - playerBestLaptime;

                // Calculate new AI weight penalty reduction
                float newP2AiWeightPenaltyReduction = p2AiBestLaptimeDelta * weightPenaltyPerSecond;
                // Bug?
                float newP2AiWeightPenaltyLaptimeSaved = (newP2AiWeightPenaltyReduction - p2AiDriver.WeightPenalty) / weightPenaltyPerSecond;

                // If the new weight penalty reduction is more than the current weight penalty then we need to zero the AI weight penalty and adjust the player's weight penalty instead
                float newP2AiWeightPenalty;
                if (newP2AiWeightPenaltyReduction >= p2AiDriver.WeightPenalty)
                {
                    newP2AiWeightPenalty = 0;
                }
                else
                {
                    newP2AiWeightPenalty = p2AiDriver.WeightPenalty - newP2AiWeightPenaltyReduction;
                }

                // Log and apply new AI weight penalty
                AddLogItem($"Decreasing weight penalty for driver {p2AiDriver.Name} in place {p2AiDriver.Place} with best laptime {p2AiBestLaptime} from {p2AiDriver.WeightPenalty} by {newP2AiWeightPenaltyReduction} to {newP2AiWeightPenalty} saving {newP2AiWeightPenaltyLaptimeSaved} seconds/lap.", Logger.LogLevel.Info);
                p2AiDriver.WeightPenalty = newP2AiWeightPenalty;

                // Calculate new AI laptime with weight penalty adjustment taken into account
                p2AiDriver.BopProjectedLaptime = p2AiBestLaptime - newP2AiWeightPenaltyLaptimeSaved;

                // Calculate relative performance improvement percentage of the first AI so we can apply that percentage improvement to the rest of the AI to maintain their relative gaps but also keep up to the driver ahead of them
                float p2AiLaptimeImprovementFactor = newP2AiWeightPenaltyLaptimeSaved / p2AiBestLaptime;

                // Apply a relative performance improvement to the rest of the AI based on the percentage improvement of the first AI so they all maintain their relative gaps but also keep up to the driver ahead of them
                List<AaiDriver> otherAiDrivers = aiDrivers.Slice(1, aiDrivers.Count - 1);
                foreach (var aiDriver in otherAiDrivers)
                {
                    float aiDriverBestLaptime = aiDriver.Laptimes.Min();
                    float newAiWeightPenaltyLaptimeSaved = aiDriverBestLaptime * p2AiLaptimeImprovementFactor; // Seconds saved per lap eg. 0.5 seconds/lap
                    float newAiWeightPenaltyReduction = newAiWeightPenaltyLaptimeSaved * weightPenaltyPerSecond; // Convert seconds saved to weight penalty reduction eg. 16.666667 weight penalty reduction
                    float newAiWeightPenalty;
                    if (newAiWeightPenaltyReduction >= aiDriver.WeightPenalty)
                    {
                        newAiWeightPenalty = 0;
                    }
                    else
                    {
                        newAiWeightPenalty = aiDriver.WeightPenalty - newAiWeightPenaltyReduction;
                    }

                    AddLogItem($"Decreasing weight penalty for driver {aiDriver.Name} in place {aiDriver.Place} with best laptime {aiDriverBestLaptime} from {aiDriver.WeightPenalty} by {newAiWeightPenaltyReduction} ({p2AiLaptimeImprovementFactor * 100}%) to {newAiWeightPenalty} saving {newAiWeightPenaltyLaptimeSaved} seconds/lap.", Logger.LogLevel.Info);
                    aiDriver.WeightPenalty = newAiWeightPenalty;
                    aiDriver.BopProjectedLaptime = aiDriverBestLaptime - newAiWeightPenaltyLaptimeSaved;
                }

                // If any AI weight penalties are zero it almost certainly means we couldn't adjust AI enough to catch up to the player so we need to adjust player weight penalty for the remaining laptime delta
                if ( aiDrivers.Any(d => d.WeightPenalty == 0))
                {
                    AaiDriver fastestAiDriver = aiDrivers.Where(d => d.WeightPenalty == 0).MinBy(d => d.Laptimes.Min()) ?? throw new Exception("Failed to find fastest AI driver with zero weight penalty.");

                    float aiLaptimeDelta = fastestAiDriver.BopProjectedLaptime - playerBestLaptime;
                    float playerWeightPenaltyIncrease = aiLaptimeDelta * weightPenaltyPerSecond;
                    float newPlayerWeightPenalty = playerDriver.WeightPenalty + playerWeightPenaltyIncrease;
                    AddLogItem($"Increasing weight penalty for player {playerDriver.Name} in place {playerDriver.Place} with best laptime {playerBestLaptime} from {playerDriver.WeightPenalty} by {playerWeightPenaltyIncrease} to {newPlayerWeightPenalty} adding {aiLaptimeDelta} seconds/lap.", Logger.LogLevel.Info);
                    playerDriver.WeightPenalty = newPlayerWeightPenalty;
                }

            }
            else // If player is not in first place then we need reduce the player's weight penalty and possibly also add weight penalties to the AI to slow them down enough to let the player catch up
            {
                // First, reduce the player's weight penalty
                // - If the player still isn't fast enough then we'll add weight to the AI

                // If the player's weight penalty is zero it means we can't adjust the player any faster so we need to add weight penalty to the AI


            }

        }

        //private void StartSharedMemoryRefreshTimer()
        //{
        //    AddLogItem("Starting shared memory refresh timer...", Logger.LogLevel.Debug);

        //    // Enable existing timer
        //    if (_sharedMemoryRefreshTimer is not null)
        //    {
        //        if (_sharedMemoryRefreshTimer.IsEnabled)
        //        {
        //            AddLogItem("Shared memory refresh timer already started", Logger.LogLevel.Debug);
        //        }
        //        else
        //        {
        //            _sharedMemoryRefreshTimer.IsEnabled = true;

        //        }
        //        return;
        //    }

        //    // Setup new timer
        //    int refreshTime = int.TryParse(App.Config.IniData.Sections["AutomaticAiView"]["RefreshSharedMemoryTime"], out int result) ? result : 1;
        //    _sharedMemoryRefreshTimer = new DispatcherTimer
        //    {
        //        Interval = TimeSpan.FromSeconds(refreshTime)
        //    };
        //    _sharedMemoryRefreshTimer.Tick += OnSharedMemoryRefreshTimerTick;
        //    RefreshSharedMemory(); // Immediate refresh on start
        //    _sharedMemoryRefreshTimer.Start();
        //}

        //private void StopSharedMemoryRefreshTimer()
        //{
        //    AddLogItem("Stopping shared memory refresh timer...", Logger.LogLevel.Debug);
        //    if (_sharedMemoryRefreshTimer is not null)
        //    {
        //        _sharedMemoryRefreshTimer.IsEnabled = false;
        //        //_sharedMemoryRefreshTimer.Stop();
        //        //_sharedMemoryRefreshTimer.Tick -= OnSharedMemoryRefreshTimerTick;
        //        //_sharedMemoryRefreshTimer = null;
        //    }
        //    ActivateButton.IsEnabled = true;
        //    DeactivateButton.IsEnabled = false;
        //}

        //private void OnSharedMemoryRefreshTimerTick(object? sender, EventArgs e)
        //{
        //    AddLogItem("Handling shared memory refresh timer tick...", Logger.LogLevel.Debug);
        //    RefreshSharedMemory();
        //}

        //private async void RefreshSharedMemory()
        //{
        //    AddLogItem("RefreshSharedMemory()", Logger.LogLevel.Debug);
        //    await Task.Run(() => LoadGtr2SharedMemory());
        //}

        //private void LoadGtr2SharedMemory()
        //{
        //    //public int mSession;                                         // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
        //    //_gtr2SharMemOps.FetchGtr2SharedMemoryStructs();
        //    AddLogItem("LoadGtr2SharedMemory(): Start connect and read", Logger.LogLevel.Debug);
        //    _gtr2SharMemOps.ConnectGtr2MemoryBuffers();
        //    _gtr2SharMemOps.ReadGtr2MemoryBuffers();
        //    AddLogItem("LoadGtr2SharedMemory(): End connect and read", Logger.LogLevel.Debug);
        //    //gtr2SharMemOps.Gtr2Scoring.mScoringInfo.mSession ;
        //}

        private void StartDriversRefreshTimer()
        {
            AddLogItem("Starting drivers refresh timer...", Logger.LogLevel.Debug);
            ActivateButton.IsEnabled = false;
            DeactivateButton.IsEnabled = true;

            // Enable existing timer
            if (_driversRefreshTimer is not null)
            {
                if (_driversRefreshTimer.IsEnabled)
                {
                    AddLogItem("Drivers refresh timer already started", Logger.LogLevel.Debug);
                }
                else
                {
                    _driversRefreshTimer.IsEnabled = true;

                }
                return;
            }

            // Start new timer
            int refreshTime = int.TryParse(App.Config.IniData.Sections["AutomaticAiView"]["RefreshDriversTime"], out int result) ? result : 1;
            _driversRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(refreshTime)
            };
            _driversRefreshTimer.Tick += OnDriversRefreshTimerTick;
            RefreshDrivers(); // Immediate refresh on start
            _driversRefreshTimer.Start();
        }

        private void StopDriversRefreshTimer()
        {
            AddLogItem("Stopping drivers refresh timer...", Logger.LogLevel.Debug);
            if (_driversRefreshTimer is not null)
            {
                _driversRefreshTimer.Stop();
                _driversRefreshTimer.Tick -= OnDriversRefreshTimerTick;
                _driversRefreshTimer = null;
            }
            ActivateButton.IsEnabled = true;
            DeactivateButton.IsEnabled = false;
        }

        private void OnDriversRefreshTimerTick(object? sender, EventArgs e)
        {
            AddLogItem("Handling drivers refresh timer tick...", Logger.LogLevel.Debug);
            RefreshDrivers();
        }

        private async void RefreshDrivers()
        {
            AddLogItem("RefreshDrivers()", Logger.LogLevel.Debug);
            await Task.Run(() => LoadDrivers());
        }

        

        private void LoadDrivers()
        {
            AddLogItem("LoadDrivers()", Logger.LogLevel.Debug);
            // Overview:
            // 1. Open the GT2 process with Gtr2MemOps functions.
            // 2. Read the Grid Slots in as AaiDriver objects.
            // 3. Add the AaiDriver objects to the AaiDrivers collection, which is bound to the UI.

            nint? gtr2ProcessPointer = null;
            try
            {
                // Read grid drivers
                AddLogItem("LoadDrivers(): Start Gtr2MemOps.ReadGtr2GridDrivers()", Logger.LogLevel.Debug);
                Gtr2GridDrivers gtr2GridDrivers = Gtr2ProgMemOps.ReadGtr2GridDrivers() ?? throw new Exception("Failed reading GTR2 grid.");
                AddLogItem("LoadDrivers(): End Gtr2MemOps.ReadGtr2GridDrivers()", Logger.LogLevel.Debug);

                // Get drivers from shared memory (SM) to match against drivers from program memory (PM) to determine active driver ie. driver one or two in each slot
                // - SM mDriver is currently active driver. The name we pick from PM should match SM mDriver.
                //Gtr2SharMemOps gtr2SharMemOps = new();
                //gtr2SharMemOps.FetchGtr2SharedMemoryStructs();
                //Gtr2Scoring scoring = gtr2SharMemOps.Gtr2Scoring;
                //var mDriverNameTmp = MemUtils.GetStringFromBytes(scoring.mVehicles[0].mDriverName, Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE));
                //AddLogItem($"mDriverNameTmp={mDriverNameTmp}", Logger.LogLevel.Debug);

                // Check for shared memory vehicles present
                //if ( _gtr2SharMemOps.Gtr2Scoring.mVehicles is null || _gtr2SharMemOps.Gtr2Scoring.mVehicles.Length == 0)
                //{
                //    throw new Exception("No vehicles found in shared memory.");
                //}
                //Gtr2VehicleScoring[] smVehicles = _gtr2SharMemOps.Gtr2Scoring.mVehicles;
                if ( _gtr2SharedMemoryWatcher.Gtr2SharMemOps.Gtr2Scoring.mVehicles is null || _gtr2SharedMemoryWatcher.Gtr2SharMemOps.Gtr2Scoring.mVehicles.Length == 0)
                {
                    throw new Exception("No vehicles found in shared memory.");
                }
                Gtr2VehicleScoring[] smVehicles = _gtr2SharedMemoryWatcher.Gtr2SharMemOps.Gtr2Scoring.mVehicles;

                // Convert Gtr2GridDrivers to AaiDriver list
                List<AaiDriver> newAaiDrivers = [];
                //foreach (var gridDriver in gtr2GridDrivers.Drivers)
                for (int i = 0; i < gtr2GridDrivers.Drivers.Count; i++)
                {
                    Gtr2VehicleScoring smVehicle = smVehicles[i];
                    var smDriverName = MemUtils.GetStringFromBytes(smVehicle.mDriverName, Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE));
                    //AddLogItem($"smDriverName={smDriverName}", Logger.LogLevel.Debug);
                    List<Gtr2GridDriver> pmGridDrivers = gtr2GridDrivers.Drivers;
                    Gtr2GridDriver pmGridDriver = pmGridDrivers[i];

                    // Vehicle Slot Id is our unique id for each data grid row for now
                    var vehicleSlotIdMemoryItem = pmGridDriver.GetMemoryItemByName("slot_id") ?? throw new Exception($"Failed reading vehicle slot id memory item for driver at grid slot {i}.");
                    var vehicleSlotId = vehicleSlotIdMemoryItem.ValueAsInt32;

                    // Determine active driver
                    // XXX: This is unnecessary as mDriverName already gives us the active driver name for each slot, but I'm doing it to learn.
                    MemoryItem pmDriverNameOneMemoryItem = pmGridDriver.GetMemoryItemByName("NameFull_One") ?? throw new Exception($"Failed reading driver name memory item for driver at grid slot {i}.");
                    MemoryItem pmDriverNameTwoMemoryItem = pmGridDriver.GetMemoryItemByName("NameFull_Two") ?? throw new Exception($"Failed reading driver name memory item for driver at grid slot {i}.");
                    string pmDriverNameOne = pmDriverNameOneMemoryItem.ValueAsString;
                    string pmDriverNameTwo = pmDriverNameTwoMemoryItem.ValueAsString;
                    string driverName = "";
                    if ( smDriverName == pmDriverNameOne )
                    {
                        //AddLogItem($"Chose driverNameOne={driverNameOne}", Logger.LogLevel.Info);
                        driverName = pmDriverNameOne;
                    } else
                    {
                        //AddLogItem($"Chose driverNameTwo={driverNameTwo}", Logger.LogLevel.Info);
                        driverName = pmDriverNameTwo;
                    }
                    //string driverName = gridDriver.GetFirstDriverName();

                    // Get last laptime
                    MemoryItem lastLaptimeMemoryItem = pmGridDriver.GetMemoryItemByName("Timing_Laptime_A") ?? throw new Exception($"Failed reading laptime memory item for driver {driverName}.");
                    float lastLaptime = lastLaptimeMemoryItem.ValueAsFloat;

                    // Add new AaiDriver to list
                    AaiDriver driver = new()
                    {
                        VehicleSlotId = vehicleSlotId,
                        Name = driverName,
                        LastLaptime = lastLaptime
                    };
                    newAaiDrivers.Add(driver);
                }
                
                // Update UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // This is a manual update of each row rather than a clear and re-add of a whole list as that seems too heavy for smooth UX
                    if (AaiDrivers.Count > 0)
                    {
                        for (int i = 0; i < AaiDrivers.Count; i++)
                        {
                            var aaiDriver = AaiDrivers[i];
                            var newAaiDriver = newAaiDrivers[i];
                            aaiDriver.VehicleSlotId = newAaiDriver.VehicleSlotId;
                            aaiDriver.Name = newAaiDriver.Name;
                            aaiDriver.LastLaptime = newAaiDriver.LastLaptime;
                        }
                    }
                    else
                    {
                        AaiDrivers.AddRange(newAaiDrivers);
                    }

                    // Old heavy way:
                    //AaiDrivers.Clear();
                    //AaiDrivers.AddRange(newAaiDrivers);
                });
            }
            catch (Exception ex)
            {
                AddLogItem($"Failed loading drivers: {ex.Message}", Logger.LogLevel.Exception);
            }
            finally 
            {
                if (gtr2ProcessPointer is not null)
                {
                    Gtr2ProgMemOps.CloseHandle((nint)gtr2ProcessPointer);
                }
            }
        }

    }
}
