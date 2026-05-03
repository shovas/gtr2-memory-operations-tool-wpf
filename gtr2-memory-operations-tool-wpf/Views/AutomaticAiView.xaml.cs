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
        private DispatcherTimer _refreshTimer;
        public AutomaticAiView()
        {
            InitializeComponent();
            DataContext = this;
            AddLogItem("Automatic AI tab starting...", Logger.LogLevel.Info);
            AddLogItem("Automatic AI tab started.", Logger.LogLevel.Info);
            if ( Gtr2ProgMemOps.IsGtr2ProcessRunning())
            {
                AddLogItem("GTR2 process detected. Loading drivers...", Logger.LogLevel.Info);
                RefreshDrivers();
                StartRefreshTimer();
            }
            else
            {
                AddLogItem("GTR2 process not detected. Please start GTR2 to load drivers.", Logger.LogLevel.Warning);
            }
        }

        private void StartRefreshTimer()
        {
            int refreshTime = int.TryParse(App.Config.IniData.Sections["AutomaticAiView"]["RefreshDriversTime"], out int result) ? result : 1;
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(refreshTime)
            };
            _refreshTimer.Tick += OnRefreshTimerTick;
            _refreshTimer.Start();
        }

        private void OnRefreshTimerTick(object sender, EventArgs e)
        {
            RefreshDrivers();
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

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddLogItem(string message, Logger.LogLevel logLevel)
        {
            LogItem logItem = new(DateTime.Now, message, logLevel);
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogItems.Add(logItem);
            });
        }

        private async void Reset()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AaiDrivers.Clear();
            });
        }

        private async void RefreshDrivers()
        {
            await Task.Run(() => LoadDrivers());
        }

        private void LoadDrivers()
        {
            // Overview:
            // 1. Open the GT2 process with Gtr2MemOps functions.
            // 2. Read the Grid Slots in as AaiDriver objects.
            // 3. Add the AaiDriver objects to the AaiDrivers collection, which is bound to the UI.

            nint? gtr2ProcessPointer = null;
            try
            {
                // Read grid drivers
                App.Log.AddDebug("LoadDrivers(): Start Gtr2MemOps.ReadGtr2GridDrivers()");
                Gtr2GridDrivers gtr2GridDrivers = Gtr2ProgMemOps.ReadGtr2GridDrivers() ?? throw new Exception("Failed reading GTR2 grid.");
                App.Log.AddDebug("LoadDrivers(): End Gtr2MemOps.ReadGtr2GridDrivers()");

                // Get drivers from shared memory (SM) to match against drivers from program memory (PM) to determine active driver ie. driver one or two in each slot
                // - SM mDriver is currently active driver. The name we pick from PM should match SM mDriver.
                Gtr2SharMemOps gtr2SharMemOps = new();
                gtr2SharMemOps.FetchGtr2SharedMemoryStructs();
                Gtr2Scoring scoring = gtr2SharMemOps.Gtr2Scoring;
                //var mDriverNameTmp = MemUtils.GetStringFromBytes(scoring.mVehicles[0].mDriverName, Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE));
                //AddLogItem($"mDriverNameTmp={mDriverNameTmp}", Logger.LogLevel.Debug);

                // Convert Gtr2GridDrivers to AaiDriver list
                List<AaiDriver> newAaiDrivers = [];
                //foreach (var gridDriver in gtr2GridDrivers.Drivers)
                for (int i = 0; i < gtr2GridDrivers.Drivers.Count; i++)
                {
                    var mVehicle = scoring.mVehicles[i];
                    var mDriverName = MemUtils.GetStringFromBytes(mVehicle.mDriverName, Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE));
                    //AddLogItem($"mDriverName={mDriverName}", Logger.LogLevel.Debug);
                    var gridDriver = gtr2GridDrivers.Drivers[i];

                    // Vehicle Slot Id is our unique id for each data grid row for now
                    var vehicleSlotIdMemoryItem = gridDriver.GetMemoryItemByName("slot_id") ?? throw new Exception($"Failed reading vehicle slot id memory item for driver at grid slot {i}.");
                    var vehicleSlotId = vehicleSlotIdMemoryItem.ValueAsInt32;

                    // Determine active driver
                    // XXX: This is unnecessary as mDriverName already gives us the active driver name for each slot, but I'm doing it to learn.
                    MemoryItem driverNameOneMemoryItem = gridDriver.GetMemoryItemByName("NameFull_One") ?? throw new Exception($"Failed reading driver name memory item for driver at grid slot {i}.");
                    MemoryItem driverNameTwoMemoryItem = gridDriver.GetMemoryItemByName("NameFull_Two") ?? throw new Exception($"Failed reading driver name memory item for driver at grid slot {i}.");
                    string driverNameOne = driverNameOneMemoryItem.ValueAsString;
                    string driverNameTwo = driverNameTwoMemoryItem.ValueAsString;
                    string driverName = "";
                    if ( mDriverName == driverNameOne )
                    {
                        //AddLogItem($"Chose driverNameOne={driverNameOne}", Logger.LogLevel.Info);
                        driverName = driverNameOne;
                    } else
                    {
                        //AddLogItem($"Chose driverNameTwo={driverNameTwo}", Logger.LogLevel.Info);
                        driverName = driverNameTwo;
                    }
                    //string driverName = gridDriver.GetFirstDriverName();

                    // Get last laptime
                    MemoryItem lastLaptimeMemoryItem = gridDriver.GetMemoryItemByName("Timing_Laptime_A") ?? throw new Exception($"Failed reading laptime memory item for driver {driverName}.");
                    float lastLaptime = lastLaptimeMemoryItem.ValueAsFloat;

                    // Add new AaiDriver to list
                    AaiDriver driver = new AaiDriver
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
                    // This is a manual update of each row rather than a clear and re-add of a whole list as that seems to heavy for a smooth UX
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
                    //AaiDrivers.Clear();
                    //AaiDrivers.AddRange(newAaiDrivers);
                    LogListView.ScrollIntoView(LogItems.Last());
                });
            }
            catch (Exception ex)
            {
                // Todo: Not a failure if not in a racing session. Be quiet unless we failed while in a racing session. Shared Memory might be an easy way to check this.
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
