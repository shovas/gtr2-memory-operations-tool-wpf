using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using Gtr2MemOpsTool.Services;
using Gtr2MemOpsTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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
        public BulkObservableCollection<AaiDriver> AaiDrivers { get; set; } = []; // For now this needs to be in PM/SM Grid Vehicles sorting order (player is first)
        public BulkObservableCollection<LogItem> LogItems { get; set; } = [];

        //private readonly List<AaiDriver> _aaiDrivers = [];

        private DispatcherTimer? _driversRefreshTimer;
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

            //AddLogItem("Automatic AI tab started.", Logger.LogLevel.Info);
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
                //LogListView.ScrollIntoView(LogItems.Last());
            });
        }

        private async void Reset()
        {
            AddLogItem("Reset()", Logger.LogLevel.Debug);
            Application.Current.Dispatcher.Invoke(() =>
            {
                AaiDrivers.Clear();
                LogItems.Clear();
                Deactivate();
                Activate();
            });
        }

        private async void Activate()
        {
            //AddLogItem("Activate()", Logger.LogLevel.Debug);
            Application.Current.Dispatcher.Invoke(() =>
            {
                _gtr2SharedMemoryWatcher.WatchGtr2SharedMemory();
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
            RecordDriverPlace(e.VehicleSlotId, e.DriverName, e.Place);
        }

        private void OnLaptimeChanged(object? sender, LaptimeChangedEventArgs e)
        {
            if(e.NewLapTime < 0)
            {
                return;
            }
            AddLogItem($"Laptime changed for driver {e.DriverName}: {e.NewLapTime}", Logger.LogLevel.Info);
            RecordDriverLaptime(e.VehicleSlotId, e.DriverName, e.NewLapTime);
            UpdateDriverWeightPenalty(e.VehicleSlotId, e.DriverName);
        }

        private void RecordDriverPlace(int vehicleSlotId, string driverName, int newPlace)
        {
            var driver = AaiDrivers.FirstOrDefault(d => d.VehicleSlotId == vehicleSlotId);
            //var driver = AaiDrivers.FirstOrDefault(d => d.Name == driverName);
            if (driver is null)
            {
                AddLogItem($"RecordDriverPlace(): driver is null (vehicleSlotId: {vehicleSlotId}, driver name: {driverName})", Logger.LogLevel.Debug);
            }
            if ( driver is not null)
            {
                driver.Place = newPlace;
            }
        }

        private void RecordDriverLaptime(int vehicleSlotId, string driverName, float newLapTime)
        {
            var driver = AaiDrivers.FirstOrDefault(d => d.VehicleSlotId == vehicleSlotId);
            //var driver = AaiDrivers.FirstOrDefault(d => d.Name == driverName);
            if (driver is null)
            {
                AddLogItem($"RecordDriverLaptime(): driver is null (vehicleSlotId: {vehicleSlotId}, driver name: {driverName})", Logger.LogLevel.Debug);
            }
            driver?.Laptimes.Add(newLapTime);
        }

        private void UpdateDriverWeightPenalty(int vehicleSlotId, string driverName)
        {
            AddLogItem("UpdateDriverWeightPenalty()", Logger.LogLevel.Debug);
            if (AaiDrivers.Count == 0)
            {
                AddLogItem($"UpdateDriverWeightPenalty(): No drivers found", Logger.LogLevel.Debug);
            }
            AaiDriver playerDriver = AaiDrivers[AaiDriver.PlayerVehicleSlotId];
            var targetDriver = AaiDrivers.FirstOrDefault(d => d.VehicleSlotId == vehicleSlotId);
            //var targetDriver = AaiDrivers.FirstOrDefault(d => d.Name == driverName);
            if (targetDriver is null)
            {
                AddLogItem($"Failed to find driver {driverName} to update weight penalty.", Logger.LogLevel.Warning);
                return;
            }

            // Calculate new weight penalties relative to the player
            //if (targetDriver.Name == playerDriver.Name)
            if(vehicleSlotId == AaiDriver.PlayerVehicleSlotId)
            {
                CalculateWeightPenaltiesVsPlayer(targetDriver);
            }

        }

        private void CalculateWeightPenaltiesVsPlayer(AaiDriver playerDriver)
        {
            AddLogItem("CalculateWeightPenaltiesVsPlayer()", Logger.LogLevel.Debug);
            // Continue only if we have enough laptimes recorded
            int minLaptimeCount = int.TryParse(App.Config.IniData.Sections["AutomaticAi"]["MinLaptimeCount"], out int minLaptimeCountResult) ? minLaptimeCountResult : 1;
            //if (playerDriver.Laptimes.Count < minLaptimeCount)
            if (playerDriver.TotalLaps < minLaptimeCount)
            {
                AddLogItem($"CalculateWeightPenaltiesVsPlayer(): Skipping. Player has less than required laptimes (has: {playerDriver.TotalLaps}, needs: {minLaptimeCount})", Logger.LogLevel.Debug);
                return;
            }

            // Player must have a best laptime available
            float playerBestLaptime = playerDriver.BestLaptime; //playerDriver.Laptimes.Min();
            if (playerBestLaptime < 0)
            {
                AddLogItem($"CalculateWeightPenaltiesVsPlayer(): Skipping. Player driver ({playerDriver.Name}) does not have a best laptime yet", Logger.LogLevel.Debug);
                return;
            }
            float weightPenaltyPerSecond = float.TryParse(App.Config.IniData.Sections["AutomaticAi"]["WeightPenaltyPerSecond"], out float weightPenaltyPerSecondResult) ? weightPenaltyPerSecondResult : 33.333333f;

            // Get all AI drivers excluding the player driver (always first)
            List<AaiDriver> aiDrivers = AaiDrivers.ToList()[1..]; // Or AaiDrivers.Skip(1).ToList()

            // Skip until all AI drivers have enough laptimes otherwise calculations will be off
            foreach (var aiDriver in aiDrivers)
            {
                // AI drivers must have a minimum number of laps
                //if (aiDriver.Laptimes.Count < minLaptimeCount)
                if (aiDriver.TotalLaps < minLaptimeCount)
                {
                    AddLogItem($"CalculateWeightPenaltiesVsPlayer(): Skipping. AI driver ({aiDriver.Name}) has less than required laptimes (has: {aiDriver.TotalLaps}, needs: {minLaptimeCount})", Logger.LogLevel.Debug);
                    return;
                }

                // Ai drivers must have a best laptime recorded
                if (aiDriver.BestLaptime < 0)
                {
                    AddLogItem($"CalculateWeightPenaltiesVsPlayer(): Skipping. AI driver ({aiDriver.Name}) driver does not have a best laptime yet", Logger.LogLevel.Debug);
                    return;
                }

            }

            // We only need to calculate weight penalties against the player
            // - If player is in first place then we need AI to catch up and possibly we need to slow down the player if the AI can't catch up enough by just reducing their weight penalties
            
            AddLogItem($"Adjusting weight penalties vs player driver {playerDriver.Name} in place {playerDriver.Place} with best laptime {playerBestLaptime}.", Logger.LogLevel.Info);
            if ( playerDriver.Place == 1)
            {

                // Overview:
                // 1. Reduce P2 AI weight penalty
                // 2. Reduce weight penalties for the rest of the AI based on the percentage improvement of the first AI so they all maintain their relative gaps but also keep up to the driver ahead of them
                // 3. If P2 still isn't as fast as the player, add player weight penalty

                //
                // 1. Reduce P2 AI weight penalty  
                //

                // First reduce P2 AI weight penalty
                // - Calculate AI laptime saved by the reduction to determine if we still need to add a weight penalty to the player after adjusting AI
                var aiDriversByPlace = aiDrivers.OrderBy(d => d.Place);
                AaiDriver p2AiDriver = aiDriversByPlace.First();

                // Skip if P2 AI driver doesn't have enough laptimes recorded
                //if ( p2AiDriver.Laptimes.Count < minLaptimeCount)
                if (p2AiDriver.TotalLaps < minLaptimeCount)
                {
                    return;
                }

                // Determine best laptime and delta to player best laptime
                float p2AiBestLaptime = p2AiDriver.BestLaptime; //p2AiDriver.Laptimes.Min();
                float p2AiBestLaptimeDelta = p2AiBestLaptime - playerBestLaptime;

                // Calculate new AI weight penalty reduction
                float newP2AiWeightPenaltyCalculatedReduction = p2AiBestLaptimeDelta * weightPenaltyPerSecond;
                float newP2AiWeightPenaltyActualReduction = newP2AiWeightPenaltyCalculatedReduction;
                // - If the new weight penalty reduction is more than the current weight penalty then we need to zero the AI weight penalty and adjust the player's weight penalty instead
                float newP2AiWeightPenalty;
                if (newP2AiWeightPenaltyCalculatedReduction >= p2AiDriver.WeightPenalty)
                {
                    newP2AiWeightPenaltyActualReduction = p2AiDriver.WeightPenalty;
                    newP2AiWeightPenalty = 0;
                }
                else
                {
                    newP2AiWeightPenalty = p2AiDriver.WeightPenalty - newP2AiWeightPenaltyActualReduction;
                }
                float newP2AiWeightPenaltyLaptimeDecrease = newP2AiWeightPenaltyActualReduction / weightPenaltyPerSecond;

                // Log and apply new AI weight penalty
                AddLogItem($"Decreasing weight penalty for AI driver {p2AiDriver.Name} in place {p2AiDriver.Place} with best laptime {p2AiBestLaptime} from {p2AiDriver.WeightPenalty} to {newP2AiWeightPenalty} ({newP2AiWeightPenaltyActualReduction:+0.##;-0.##}) saving {newP2AiWeightPenaltyLaptimeDecrease} seconds/lap.", Logger.LogLevel.Info);
                p2AiDriver.WeightPenalty = newP2AiWeightPenalty;

                // Calculate new AI laptime with weight penalty adjustment taken into account
                p2AiDriver.BopProjectedLaptime = p2AiBestLaptime - newP2AiWeightPenaltyLaptimeDecrease;
                // Calculate relative performance improvement percentage of the first AI so we can apply that percentage improvement to the rest of the AI to maintain their relative gaps but also keep up to the driver ahead of them
                float p2AiLaptimeDecreaseFactor = newP2AiWeightPenaltyLaptimeDecrease / p2AiBestLaptime;

                //
                // 2. Apply a relative performance improvement to the rest of the AI based on the percentage improvement of the first AI so they all maintain their relative gaps but also keep up to the driver ahead of them
                //

                // Apply a relative performance improvement to the rest of the AI based on the percentage improvement of the first AI so they all maintain their relative gaps but also keep up to the driver ahead of them
                List<AaiDriver> otherAiDrivers = [.. aiDriversByPlace.Skip(1)];
                foreach (var aiDriver in otherAiDrivers)
                {
                    float aiDriverBestLaptime = aiDriver.BestLaptime; // aiDriver.Laptimes.Min();
                    float newAiWeightPenaltyLaptimeSaved = aiDriverBestLaptime * p2AiLaptimeDecreaseFactor; // Seconds saved per lap eg. 0.5 seconds/lap
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

                    AddLogItem($"Decreasing weight penalty for AI driver {aiDriver.Name} in place {aiDriver.Place} with best laptime {aiDriverBestLaptime} from {aiDriver.WeightPenalty} to {newAiWeightPenalty} ({newAiWeightPenaltyReduction:+0.##;-0.##} / {p2AiLaptimeDecreaseFactor * 100}%) saving {newAiWeightPenaltyLaptimeSaved} seconds/lap.", Logger.LogLevel.Info);
                    aiDriver.WeightPenalty = newAiWeightPenalty;
                    aiDriver.BopProjectedLaptime = aiDriverBestLaptime - newAiWeightPenaltyLaptimeSaved;
                }

                //
                // 3. If P2 still isn't as fast as the player, add player weight penalty
                //

                // Adjust ai weight penalties if the fastest ai is still slower than the player
                if (p2AiDriver.BopProjectedLaptime > playerBestLaptime)
                {
                    AaiDriver fastestAiDriver = aiDrivers.Where(d => d.WeightPenalty == 0).MinBy(d => d.BestLaptime) ?? throw new Exception("Failed to find fastest AI driver with zero weight penalty.");
                    float aiLaptimeDelta = fastestAiDriver.BopProjectedLaptime - playerBestLaptime;
                    float playerWeightPenaltyIncrease = aiLaptimeDelta * weightPenaltyPerSecond;
                    float newPlayerWeightPenalty = playerDriver.WeightPenalty + playerWeightPenaltyIncrease;
                    AddLogItem($"Increasing weight penalty for player driver {playerDriver.Name} in place {playerDriver.Place} with best laptime {playerBestLaptime} from {playerDriver.WeightPenalty} to {newPlayerWeightPenalty} ({playerWeightPenaltyIncrease:+0.##;-0.##}) adding {aiLaptimeDelta} seconds/lap.", Logger.LogLevel.Info);
                    playerDriver.WeightPenalty = newPlayerWeightPenalty;
                }

            }
            else // If player is not in first place then we need reduce the player's weight penalty and possibly also add weight penalties to the AI to slow them down enough to let the player catch up
            {

                // Overview:
                // 1. Reduce player weight penalty based on the delta to the first AI and a weight penalty per second factor from the config
                // 2. Increase AI weight penalties if player weight penalty is zero and player still isn't as fast as the first AI
                // 3. Apply a relative performance improvement to the rest of the AI based on the percentage improvement of the player so they all maintain their relative gaps but also keep up to the driver ahead of them

                //
                // 1. Reduce player weight penalty
                //

                var aiDriversByPlace = aiDrivers.OrderBy(d => d.Place);
                AaiDriver leaderDriver = aiDriversByPlace.First();
                float leaderBestLaptime = leaderDriver.BestLaptime; //leaderDriver.Laptimes.Min();

                // Skip if leader driver doesn't have enough laptimes recorded
                //if (leaderDriver.Laptimes.Count < minLaptimeCount)
                if (leaderDriver.TotalLaps < minLaptimeCount)
                {
                    return;
                }

                // Determine best laptime and delta to player best laptime
                float playerBestLaptimeToLeaderDelta = playerBestLaptime - leaderBestLaptime;

                // Calculate new player weight penalty reduction
                float newPlayerWeightPenaltyCalculatedReduction = playerBestLaptimeToLeaderDelta * weightPenaltyPerSecond;
                float newPlayerWeightPenaltyActualReduction = newPlayerWeightPenaltyCalculatedReduction;
                // - If the new weight penalty reduction is more than the current weight penalty then we need to zero the AI weight penalty and adjust the player's weight penalty instead
                float newPlayerWeightPenalty;
                if (newPlayerWeightPenaltyCalculatedReduction >= playerDriver.WeightPenalty)
                {
                    newPlayerWeightPenaltyActualReduction = playerDriver.WeightPenalty;
                    newPlayerWeightPenalty = 0;
                }
                else
                {
                    newPlayerWeightPenalty = playerDriver.WeightPenalty - newPlayerWeightPenaltyActualReduction;
                }
                float newPlayerWeightPenaltyLaptimeDecrease = newPlayerWeightPenaltyActualReduction / weightPenaltyPerSecond;

                // Log and apply new player weight penalty
                AddLogItem($"Decreasing weight penalty for player driver {playerDriver.Name} in place {playerDriver.Place} with best laptime {playerBestLaptime} from {playerDriver.WeightPenalty} to {newPlayerWeightPenalty} ({newPlayerWeightPenaltyActualReduction:+0.##;-0.##}) saving {newPlayerWeightPenaltyLaptimeDecrease} seconds/lap.", Logger.LogLevel.Info);
                playerDriver.WeightPenalty = newPlayerWeightPenalty;

                // Calculate new laptime with weight penalty adjustment taken into account
                playerDriver.BopProjectedLaptime = playerBestLaptime - newPlayerWeightPenaltyLaptimeDecrease;

                // Adjust AI weight penalties if the player's projected laptime is still slower than the leader AI driver
                if (playerDriver.BopProjectedLaptime > leaderBestLaptime)
                {
                    
                    //
                    // 2. Increase leader weight penalties if player still isn't as fast as the leader AI driver
                    //

                    float playerToLeaderLaptimeDelta = playerDriver.BopProjectedLaptime - leaderBestLaptime;
                    float leaderWeightPenaltyIncrease = playerToLeaderLaptimeDelta * weightPenaltyPerSecond;
                    float newLeaderWeightPenalty = leaderDriver.WeightPenalty + leaderWeightPenaltyIncrease;
                    AddLogItem($"Increasing weight penalty for leader AI driver {leaderDriver.Name} in place {leaderDriver.Place} with best laptime {leaderBestLaptime} from {leaderDriver.WeightPenalty} to {newLeaderWeightPenalty} ({leaderWeightPenaltyIncrease:+0.##;-0.##}) adding {playerToLeaderLaptimeDelta} seconds/lap.", Logger.LogLevel.Info);
                    leaderDriver.WeightPenalty = newLeaderWeightPenalty;
                    leaderDriver.BopProjectedLaptime = leaderBestLaptime + playerToLeaderLaptimeDelta;
                    float leaderLaptimePenaltyFactor = (leaderDriver.BopProjectedLaptime - leaderBestLaptime) / leaderBestLaptime;

                    //
                    // 3. Apply a relative performance penalty to the rest of the AI based on the percentage penalty of the leader so they all maintain their relative gaps but also keep up to the driver ahead of them
                    //

                    List<AaiDriver> otherAiDrivers = [.. aiDriversByPlace.Skip(1)];
                    foreach (var aiDriver in otherAiDrivers)
                    {
                        float aiDriverBestLaptime = aiDriver.BestLaptime; //aiDriver.Laptimes.Min();
                        float newAiWeightPenaltyLaptimeIncrease = aiDriverBestLaptime * leaderLaptimePenaltyFactor; // Seconds saved per lap eg. 0.5 seconds/lap
                        float newAiWeightPenaltyIncrease = newAiWeightPenaltyLaptimeIncrease * weightPenaltyPerSecond; // Convert seconds saved to weight penalty reduction eg. 16.666667 weight penalty reduction
                        float newAiWeightPenalty = aiDriver.WeightPenalty + newAiWeightPenaltyIncrease;
                        AddLogItem($"Increasing weight penalty for AI driver {aiDriver.Name} in place {aiDriver.Place} with best laptime {aiDriverBestLaptime} from {aiDriver.WeightPenalty} to {newAiWeightPenalty} ({newAiWeightPenaltyIncrease:+0.##;-0.##} / {leaderLaptimePenaltyFactor * 100}%) adding {newAiWeightPenaltyLaptimeIncrease} seconds/lap.", Logger.LogLevel.Info);
                        aiDriver.WeightPenalty = newAiWeightPenalty;
                        aiDriver.BopProjectedLaptime = aiDriverBestLaptime + newAiWeightPenaltyLaptimeIncrease;
                    }

                }
            }

            // Write new weight penalties to program memory
            Gtr2GridDrivers? gridDrivers = GetGtr2GridDrivers();
            if (gridDrivers is not null)
            {
                AddLogItem("Saving new weight penalties to program memory", Logger.LogLevel.Info);
                foreach(AaiDriver aaiDriver in AaiDrivers)
                {
                    AddLogItem($"Saving new weight penalty for {aaiDriver.Name} (Slot: {aaiDriver.VehicleSlotId}): {aaiDriver.WeightPenalty}", Logger.LogLevel.Debug);
                    var gridDriver = gridDrivers.Drivers.FirstOrDefault(
                        gd => {
                            var slotIdMemoryItem = gd.GetMemoryItemByName("slot_id");
                            if (slotIdMemoryItem is null) AddLogItem("slotIdMemoryItem is null", Logger.LogLevel.Debug);
                            var gridDriverSlotId = slotIdMemoryItem?.ValueAsInt32 ?? -1;
                            if (gridDriverSlotId < 0) AddLogItem("gridDriverSlotId < 0", Logger.LogLevel.Debug);
                            return ( gridDriverSlotId == aaiDriver.VehicleSlotId );
                        }
                    );
                    if (gridDriver is null)
                    {
                        
                        AddLogItem("gridDriver is null", Logger.LogLevel.Debug);
                        continue;
                    }
                    var weightPenaltyMemoryItem = gridDriver.GetMemoryItemByName("WeightPenalty");
                    if(weightPenaltyMemoryItem is null)
                    {
                        AddLogItem("weightPenaltyMemoryItem is null", Logger.LogLevel.Debug);
                        continue;
                    }
                    var success = weightPenaltyMemoryItem.Save(aaiDriver.WeightPenalty);
                    if (success)
                    {
                        AddLogItem("Successfully saved new weight penalty", Logger.LogLevel.Debug);
                    }
                    else
                    {
                        AddLogItem("Failed saving new weight penalty", Logger.LogLevel.Debug);
                    }

                }
            }
            else
            {
                AddLogItem("Cannot save new weight penalties to program memory: gridDrivers is null", Logger.LogLevel.Error);
            }
        }

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
            //AddLogItem("Handling drivers refresh timer tick...", Logger.LogLevel.Debug);
            RefreshDrivers();
        }

        private async void RefreshDrivers()
        {
            //AddLogItem("RefreshDrivers()", Logger.LogLevel.Debug);
            await Task.Run(() => LoadDrivers());
        }

        private void LoadDrivers()
        {
            //AddLogItem("LoadDrivers()", Logger.LogLevel.Debug);
            // Overview:
            // 1. Open the GT2 process with Gtr2MemOps functions.
            // 2. Read the Grid Slots in as AaiDriver objects.
            // 3. Add the AaiDriver objects to the AaiDrivers collection, which is bound to the UI.

            nint? gtr2ProcessPointer = null;
            try
            {
                // Read grid drivers
                //AddLogItem("LoadDrivers(): Start Gtr2MemOps.ReadGtr2GridDrivers()", Logger.LogLevel.Debug);
                Gtr2GridDrivers gtr2GridDrivers = Gtr2ProgMemOps.ReadGtr2GridDrivers() ?? throw new Exception("Failed reading GTR2 grid.");
                //AddLogItem("LoadDrivers(): End Gtr2MemOps.ReadGtr2GridDrivers()", Logger.LogLevel.Debug);

                // Check for shared memory vehicles present
                if ( _gtr2SharedMemoryWatcher.Gtr2SharMemOps.Gtr2Scoring.mVehicles is null || _gtr2SharedMemoryWatcher.Gtr2SharMemOps.Gtr2Scoring.mVehicles.Length == 0)
                {
                    throw new Exception("No vehicles found in shared memory.");
                }
                Gtr2VehicleScoring[] smVehicles = _gtr2SharedMemoryWatcher.Gtr2SharMemOps.Gtr2Scoring.mVehicles;

                // Convert Gtr2GridDrivers to AaiDriver list
                List<AaiDriver> newAaiDrivers = [];
                for (int i = 0; i < gtr2GridDrivers.Drivers.Count; i++)
                {
                    // Get shared memory vehicle
                    Gtr2VehicleScoring smVehicle = smVehicles[i];

                    // Get isPlayer
                    bool isPlayer = ( smVehicle.mIsPlayer != 0 );

                    // Get driver name
                    var smDriverName = MemUtils.GetStringFromBytes(smVehicle.mDriverName, Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE));
                    //AddLogItem($"smDriverName={smDriverName}", Logger.LogLevel.Debug);
                    List<Gtr2GridDriver> pmGridDrivers = gtr2GridDrivers.Drivers;
                    Gtr2GridDriver pmGridDriver = pmGridDrivers[i];

                    // Get Place
                    var place = (int)smVehicle.mPlace;

                    // Get TotalLaps
                    int totalLaps = smVehicle.mTotalLaps;

                    // Vehicle Slot Id is our unique id for each data grid row for now
                    var vehicleSlotIdMemoryItem = pmGridDriver.GetMemoryItemByName("slot_id") ?? throw new Exception($"Failed reading vehicle slot id memory item for driver at grid slot {i}.");
                    var vehicleSlotId = vehicleSlotIdMemoryItem.ValueAsInt32;

                    // Determine active driver
                    // - This is unnecessary as mDriverName already gives us the active driver name for each slot, but I'm doing it to learn.
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

                    // Get weight penalty
                    var weightPenaltyMemoryItem = pmGridDriver.GetMemoryItemByName("WeightPenalty") ?? throw new Exception($"Failed reading weight penalty memory item for driver at grid slot {i}.");
                    float weightPenalty = weightPenaltyMemoryItem.ValueAsFloat;

                    // Get last laptime
                    float bestLaptime = smVehicle.mBestLapTime;

                    // Get last laptime
                    MemoryItem lastLaptimeMemoryItem = pmGridDriver.GetMemoryItemByName("Timing_Laptime_A") ?? throw new Exception($"Failed reading laptime memory item for driver {driverName}.");
                    float lastLaptime = lastLaptimeMemoryItem.ValueAsFloat;

                    // Setup new/updated AaiDriver details
                    AaiDriver newDriver = new()
                    {
                        VehicleSlotId = vehicleSlotId,
                        IsPlayer = isPlayer,
                        Name = driverName,
                        Place = place,
                        TotalLaps = totalLaps,
                        BestLaptime = bestLaptime,
                        LastLaptime = lastLaptime,
                        WeightPenalty = weightPenalty
                    };

                    // Carry over current temporal data
                    var curAaiDriver = AaiDrivers.FirstOrDefault(d => d.VehicleSlotId == vehicleSlotId);
                    if ( curAaiDriver is not null)
                    {
                        newDriver.Laptimes = curAaiDriver.Laptimes;
                        newDriver.BopProjectedLaptime = curAaiDriver.BopProjectedLaptime;
                    }

                    newAaiDrivers.Add(newDriver);
                }
                
                // Update UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // This is a manual update of each row rather than a clear and re-add of the whole list as that seems too heavy for smooth UX
                    if (AaiDrivers.Count > 0)
                    {
                        // DataGrid will only update if the whole object changes not if just properties/fields change on the object

                        //for (int i = 0; i < newAaiDrivers.Count; i++)
                        //{
                        //    var newAaiDriver = newAaiDrivers[i];
                        //    var updateAaiDriver = AaiDrivers .First(d => d.VehicleSlotId == newAaiDriver.VehicleSlotId);
                        //    updateAaiDriver.Place = newAaiDriver.Place;
                        //    updateAaiDriver.TotalLaps = newAaiDriver.TotalLaps;
                        //    updateAaiDriver.LastLaptime = newAaiDriver.LastLaptime;
                        //    //updateAaiDriver.WeightPenalty = newAaiDriver.WeightPenalty; // Unmcomment when we start writing to memory
                        //}
                        for (int i = 0; i < AaiDrivers.Count; i++)
                        {
                            var newAaiDriver = newAaiDrivers.First(d => d.VehicleSlotId == AaiDrivers[i].VehicleSlotId);
                            AaiDrivers[i] = newAaiDriver;
                        }
                        //for (int i = 0; i < AaiDrivers.Count; i++)
                        //{
                        //    // This is bad because I forget to add new fields that I might want later
                        //    //var aaiDriver = ;
                        //    //var newAaiDriver = newAaiDrivers[i];
                        //    //aaiDriver.VehicleSlotId = newAaiDriver.VehicleSlotId;
                        //    //aaiDriver.Name = newAaiDriver.Name;
                        //    //aaiDriver.LastLaptime = newAaiDriver.LastLaptime;
                        //}
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

        private Gtr2GridDrivers? GetGtr2GridDrivers()
        {
            return Gtr2ProgMemOps.ReadGtr2GridDrivers();
        }
    }
}
