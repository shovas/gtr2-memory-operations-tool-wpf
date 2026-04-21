using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using Gtr2MemOpsTool.Services;
using Gtr2MemOpsTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for AutomaticAiView.xaml
    /// </summary>
    public partial class AutomaticAiView : UserControl
    {
        public BulkObservableCollection<AaiDriver> AaiDrivers { get; set; } = [];
        public BulkObservableCollection<LogItem> LogItems { get; set; } = [];
        public AutomaticAiView()
        {
            InitializeComponent();
            DataContext = this;
            AddLogItem("Automatic AI tab starting...", Logger.LogLevel.Info);

            Task.Run(() =>
            {
                LoadDrivers();
            });

            AddLogItem("Automatic AI tab started.", Logger.LogLevel.Info);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //LoadDrivers();
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
            LogItems.Add(logItem);
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
                Gtr2Grid gtr2Grid = Gtr2MemOps.ReadGtr2Grid() ?? throw new Exception("Failed reading GTR2 grid.");



                // FIXME: HERE




                //foreach (var vehicleSlot in gtr2Grid.VehicleSlots)
                //{
                //    string driverName = vehicleSlot.GetDriverName();
                //    MemoryItem lastLaptimeMemoryItem = vehicleSlot.GetMemoryItemByName("Timing_Laptime_A") ?? throw new Exception($"Failed reading laptime memory item for driver {driverName}.");
                //    float lastLaptime = lastLaptimeMemoryItem.ValueAsFloat;
                //    AaiDriver driver = new AaiDriver
                //    {
                //        Name = driverName,
                //        LastLaptime = lastLaptime
                //    };
                //    AaiDrivers.Add(driver);
                //}

            }
            catch (Exception ex)
            {
                AddLogItem($"Failed loading drivers: {ex.Message}", Logger.LogLevel.Exception);
            }
            finally 
            {
                if (gtr2ProcessPointer is not null)
                {
                    Gtr2MemOps.CloseHandle((nint)gtr2ProcessPointer);
                }
            }
        }

    }
}
