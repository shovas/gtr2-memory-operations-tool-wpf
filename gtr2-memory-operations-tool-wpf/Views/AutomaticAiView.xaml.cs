using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using Gtr2MemOpsTool.Services;
using Gtr2MemOpsTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {

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

                // Convert Gtr2GridDrivers to AaiDriver list
                List<AaiDriver> newAaiDrivers = [];
                foreach (var gridDriver in gtr2GridDrivers.Drivers)
                {
                    string driverName = gridDriver.GetFirstDriverName();
                    MemoryItem lastLaptimeMemoryItem = gridDriver.GetMemoryItemByName("Timing_Laptime_A") ?? throw new Exception($"Failed reading laptime memory item for driver {driverName}.");
                    float lastLaptime = lastLaptimeMemoryItem.ValueAsFloat;
                    AaiDriver driver = new AaiDriver
                    {
                        Name = driverName,
                        LastLaptime = lastLaptime
                    };
                    newAaiDrivers.Add(driver);
                }

                // Update UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AaiDrivers.Clear();
                    AaiDrivers.AddRange(newAaiDrivers);
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
