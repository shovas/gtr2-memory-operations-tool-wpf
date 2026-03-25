using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
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
using static Gtr2MemOpsTool.Services.AsyncBatchLogger;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public BulkObservableCollection<LogItem> LogItems { get; set; } = [];

        // CancellationTokenSource to signal the log consumer task to stop when the application is closing
        private readonly CancellationTokenSource _cts = new();

        public LogView()
        {
            // Initialize the UI components (defined in LogView.xaml)
            InitializeComponent();

            // Set the DataContext to itself so that the LogItems property can be bound to the UI
            DataContext = this;

            // Start the log consumer task
            _ = StartLogConsumerAsync(); // Fire and forget the log consumer task, it will run until the application is closed and the cancellation token is triggered.

            // Select the right log filter for display
            SelectStartupLogFilter();
            
            //App.Config.ConfigLoaded += OnConfigLoaded;
        }

        //protected override void OnClos(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    _cts!.Cancel();
        //}

        //private void OnConfigLoaded(object? sender, EventArgs e)
        //{
        //    //App.Log.AddDebug("LogView: OnConfigLoaded()");
        //    //string startupLoggingLevelLabel = App.Config.IniData.Global["StartupLoggingLevel"];
        //    ////LogLevel logLevel = Services.AsyncBatchLogger.GetLogLevel(startupLoggingLevelLabel);
        //    ////string logLevelLabel = Services.AsyncBatchLogger.GetLogLevelLabel(logLevel);
        //    //SelectLogFilterByLogLevelLabel(logLevelLabel);
        //}

        private void SelectStartupLogFilter()
        {
            App.Log.AddDebug("LogView: SelectStartupLogFilter()");
            string startupLoggingLevel = App.Config.IniData.Global["StartupLoggingLevel"];
            SelectLogFilterByLogLevelLabel(startupLoggingLevel);
            //LogFilterSelector.SelectedItem = LogFilterSelector.Items
            //    .OfType<ComboBoxItem>()
            //        .FirstOrDefault(i => i.Tag?.ToString() == startupLoggingLevel);
        }

        private void SelectLogFilterByLogLevelLabel(string logLevelLabel)
        {
            LogFilterSelector.SelectedItem = LogFilterSelector.Items
                .OfType<ComboBoxItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == logLevelLabel);
        }

        private async Task StartLogConsumerAsync()
        {
            Int32 taskDelay = 100;
            ChannelReader<LogItem> reader = App.Log.Reader;
            CancellationToken ct = _cts.Token;
            var logItems = new List<LogItem>();
            var logItemsBatchMax = 1800;

            while (!ct.IsCancellationRequested)
            {
                // Wait for at least one item
                await reader.WaitToReadAsync(ct);

                // Read batch
                while (reader.TryRead(out var logItem))
                {
                    logItems.Add(logItem);
                    if(logItems.Count>=logItemsBatchMax)
                    {
                        break;
                    }
                }

                // Marshal batch to UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    LogItems.AddRange(logItems);
                    LogListView.ScrollIntoView(LogItems.Last()); // Known issue: This won't have any effect if the user is on a different tab
                });

                logItems.Clear();
                await Task.Delay(taskDelay, ct);
            }

        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogItems.Clear();
        }

        private void CopyLogButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder clipboardString = new();
            foreach ( var logItem in LogItems) {
                var logLevelLabel = Services.AsyncBatchLogger.GetLogLevelLabel(logItem.LogLevel);
                string logItemTsStr = logItem.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string message = $"[{logItemTsStr}] [{logLevelLabel}] {logItem.Message}";
                clipboardString.AppendLine(message);
            }
            Clipboard.SetText(clipboardString.ToString());
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder clipboardString = new();
            foreach (var logItem in LogItems)
            {
                var logLevelLabel = Services.AsyncBatchLogger.GetLogLevelLabel(logItem.LogLevel);
                string logItemTsStr = logItem.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string message = $"[{logItemTsStr}] [{logLevelLabel}] {logItem.Message}";
                clipboardString.AppendLine(message);
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"GTR2_Memory_Operations_Tool_Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, clipboardString.ToString());
            }
        }

        private void LogFilterSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the logging level in the App.Log based on the selected filter
            var selectedItem = LogFilterSelector.SelectedItem as ComboBoxItem;
            if (selectedItem?.Tag is not string selectedTag)
            {
                App.Log.AddError("Unexpected condition: Log filter selection changed but selection has null value");
                return;
            }
            App.Log.LoggingLevel = Services.AsyncBatchLogger.GetLogLevel(selectedTag);

            // Refresh the CollectionView filter to apply the new logging level filter
            ICollectionView view = CollectionViewSource.GetDefaultView(LogItems);
            view.Filter = item =>
            {
                var logItem = (LogItem)item;
                return logItem.LogLevel >= App.Log.LoggingLevel;
            };
            view.Refresh();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(LogItems);
            view.Filter = item =>
            {
                var logItem = (LogItem)item;
                return logItem.Message.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
            };
        }
    }
}
