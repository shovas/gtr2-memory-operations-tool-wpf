using System;
using System.Collections.Generic;
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

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public BulkObservableCollection<LogItem> LogItems { get; set; } = new BulkObservableCollection<LogItem>();

        //private StringBuilder _logBuffer = new();
        //private DispatcherTimer? _logTimer;

        // CancellationTokenSource to signal the log consumer task to stop when the application is closing
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public LogView()
        {
            InitializeComponent();

            DataContext = this;

            _ = StartLogConsumerAsync(); // Fire and forget the log consumer task, it will run until the application is closed and the cancellation token is triggered.
            App.Log.AddDebug("AsyncBatchLogger: Application started");

            //InitializeLog();

            //InitializeLogBufferTimer();

            // Select the right log filter for display
            string loggingLevelLabel = App.Log.GetLogLevelLabel(App.Log.LoggingLevel);
            LogFilterSelector.SelectedItem = LogFilterSelector.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => i.Tag?.ToString() == loggingLevelLabel);

        }

        //private void LogBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if ( LogBox.IsFocused ) { // Auto-scrolling when the user is interacting with the control would be annoying
        //        return;
        //    }
        //    LogBox.ScrollToEnd();
        //}

        //private void InitializeLog()
        //{
        //    //App.Log.EntryAdded += OnEntryAdded;
        //}

        //private void InitializeLogBufferTimer()
        //{
        //    _logTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        //    _logTimer.Tick += (s, e) =>
        //    {
        //        if (_logBuffer.Length == 0) return;
        //        lock (_logBuffer)
        //        {
        //            LogBox.AppendText(_logBuffer.ToString());
        //            _logBuffer = new();
        //        }
        //        LogBox.ScrollToEnd();
        //    };
        //    _logTimer.Start();
        //}

        // AsyncBatchLogger calls this event which adds to our own _logBuffer and then our own timer in the constructor adds the buffer contents to the LogBox and clears the buffer
        //private void OnEntryAdded(string message, Log.LogLevel loggingLevel)
        //{
        //    if (loggingLevel < App.Log.LoggingLevel) return;
        //    lock (_logBuffer) _logBuffer.Append(message);
        //    //Dispatcher.Invoke(() => LogBox.AppendText(message));
        //}

        //protected override void OnClos(EventArgs e)
        //{
        //    base.OnClosed(e);
        //    _cts!.Cancel();
        //}

        private async Task StartLogConsumerAsync()
        {
            Int32 taskDelay = 100;
            ChannelReader<LogItem> reader = App.Log.Reader;
            CancellationToken ct = _cts.Token;
            var logItems = new List<LogItem>();
            var logItemsBatchMax = 1800;
            //StringBuilder messagesString = new StringBuilder();

            while (!ct.IsCancellationRequested)
            {
                // Wait for at least one item
                await reader.WaitToReadAsync(ct);

                // Drain everything available right now (the batch)
                //while (reader.TryRead(out var logItem))
                //    buffer.Add(logItem);

                while (reader.TryRead(out var logItem))
                {
                    logItems.Add(logItem);
                    if(logItems.Count>=logItemsBatchMax)
                    {
                        break;
                    }
                }

                //foreach (var logItem in logItems)
                //{
                //    if (logItem.LogLevel >= App.Log.LoggingLevel)
                //    {
                //        //var logMessage = logItem.Message;
                //        //var logLevelLabel = App.Log.GetLogLevelLabel(logItem.LogLevel);
                //        //string logItemTsStr = logItem.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                //        //string message = $"[{logItemTsStr}] [{logLevelLabel}] {logMessage}\n";
                //        //messagesString.Append(message);
                //    }
                //}

                // Marshal batch to UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    //LogBox.AppendText(messagesString.ToString());
                    //LogBox.ScrollToEnd();
                    LogItems.AddRange(logItems);
                });

                logItems.Clear();
                //buffer = new List<LogItem>();
                //messagesString.Clear();
                await Task.Delay(taskDelay, ct);
            }

        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            //LogBox.Clear();
            LogItems.Clear();
        }

        private void CopyLogButton_Click(object sender, RoutedEventArgs e)
        {
            //LogBox.SelectAll();
            //LogBox.Copy();
            StringBuilder clipboardString = new StringBuilder();
            foreach ( var logItem in LogItems) {
                var logLevelLabel = App.Log.GetLogLevelLabel(logItem.LogLevel);
                string logItemTsStr = logItem.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string message = $"[{logItemTsStr}] [{logLevelLabel}] {logItem.Message}";
                clipboardString.AppendLine(message);
            }
            Clipboard.SetText(clipboardString.ToString());
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder clipboardString = new StringBuilder();
            foreach (var logItem in LogItems)
            {
                var logLevelLabel = App.Log.GetLogLevelLabel(logItem.LogLevel);
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
            var selectedItem = LogFilterSelector.SelectedItem as ComboBoxItem;
            var selectedTag = selectedItem?.Tag as string;
            if ( selectedTag == null)
            {
                App.Log.AddError("Unexpected condition: Log filter selected but not content");
                return;
            }
            App.Log.LoggingLevel = App.Log.GetLogLevel(selectedTag);
            //switch ( selectedTag )
            //{
            //    //case "All":
            //    //    App.Log.LoggingLevel = AsyncBatchLogger.LogLevel.Debug;
            //    //    break;
            //    case "debug":
            //        App.Log.LoggingLevel = AsyncBatchLogger.LogLevel.Debug;
            //        break;
            //    case "info":
            //        App.Log.LoggingLevel = AsyncBatchLogger.LogLevel.Info;
            //        break;
            //    case "warning":
            //        App.Log.LoggingLevel = AsyncBatchLogger.LogLevel.Warning;
            //        break;
            //    case "error":
            //        App.Log.LoggingLevel = AsyncBatchLogger.LogLevel.Error;
            //        break;
            //    case "exception":
            //        App.Log.LoggingLevel = AsyncBatchLogger.LogLevel.Exception;
            //        break;
            //    default:
            //        App.Log.AddError($"Unexpected log filter selected: {selectedTag}");
            //        break;
            //}
        }
    }
}
