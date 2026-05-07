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

        private void OnLaptimeChanged(object? sender, LaptimeChangedEventArgs e)
        {
            AddLogItem($"Laptime changed for driver {e.DriverName}: {e.NewLapTime}", Logger.LogLevel.Info);
            // Here you can add code to update the UI or perform other actions based on the laptime change.
            var driver = _aaiDrivers.FirstOrDefault(d => d.Name == e.DriverName);
            driver?.Laptimes.Add(e.NewLapTime);
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
