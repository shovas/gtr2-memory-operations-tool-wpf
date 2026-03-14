using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gtr2MemOpsTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Gtr2MemOps instance to handle all memory operations related to GTR2, ensuring a single point of access and consistent state management across the application.
        private Gtr2MemOps Gtr2MemOps { get; set; } = new Gtr2MemOps();

        
        // CancellationTokenSource to signal the log consumer task to stop when the application is closing
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            
            InitializeGtr2MemoryOperationsTool();

        }

        /// <summary>
        /// Note: OnInitialized is called after the window is created but before it's rendered. It's a good place to start background tasks that will update the UI.
        /// </summary>
        /// <param name="e">The event data associated with the window initialization event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            App.Log.AddDebug("MainWindow initialized");

            _ = StartLogConsumerAsync(); // Fire and forget the log consumer task, it will run until the application is closed and the cancellation token is triggered.
            App.Log.AddDebug("AsyncBatchLogger: Application started");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cts!.Cancel();
        }

        private async Task StartLogConsumerAsync()
        {
            Int32 taskDelay = 1000;
            ChannelReader<LogItem> reader = App.Log.Reader;
            CancellationToken ct = _cts.Token;
            var buffer = new List<LogItem>();

            while (!ct.IsCancellationRequested)
            {
                // Wait for at least one item
                await reader.WaitToReadAsync(ct);

                // Drain everything available right now (the batch)
                while (reader.TryRead(out var logItem))
                    buffer.Add(logItem);

                // Marshal batch to UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var logItem in buffer)
                        App.LogObj.Add(logItem.Message, logItem.LogLevel);
                });

                //buffer.Clear();
                buffer = new List<LogItem>();
                await Task.Delay(taskDelay, ct);
            }
        }

        private void InitializeGtr2MemoryOperationsTool()
        {
            App.Log.AddInfo("GTR2 Memory Operations Tool Initialized");
            ShowStatusWelcome();
            CheckGtr2Process();
        }

        private void ShowStatusWelcome()
        {
            // Show a Welcome message on the StatusBar
            StatusBarItemText.Text = "GTR2 Memory Operations Tool Loaded";
            int timerTime = 1;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(timerTime) };
            timer.Tick += (s, e) =>
            {
                StatusBarItemText.Text = "Ready";
                timer.Stop();
            };
            timer.Start();
        }

        private async void CheckGtr2Process()
        {
            bool isRunning = false;
            await Task.Run(() => isRunning = Gtr2MemOps.IsGtr2ProcessRunning());
            if (isRunning)
            {
                App.Log.AddInfo("GTR2 Process Detected");
                StatusBarItemText.Text = "GTR2 Detected";
            }
            else
            {
                App.Log.AddError("GTR2 Process Not Detected");
                App.Log.AddInfo("Select Process > Check to check for GTR2 process again");
                //StatusBarItemText.Text = "GTR2 Not Detected";
            }
        }

        private async void MenuItem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void MenuItem_Tests_TestGTR2Process(object sender, RoutedEventArgs e)
        {
            bool success = Gtr2MemOps.TestGtr2Process();
            if (success)
            {
                App.Log.AddInfo("Test Pass: Test GTR2 Process");
            }
            else
            {
                App.Log.AddError("Test Failed: Test GTR2 Process");
            }
        }

        private async void MenuItem_Tests_TestGetProcess(object sender, RoutedEventArgs e)
        {
            bool success = Gtr2MemOps.TestGtr2GetProcess();
            if (success)
            {
                App.Log.AddInfo("Test Pass: Test GTR2 Open Process");
            }
            else
            {
                App.Log.AddError("Test Failed: Test GTR2 Open Process");
            }
        }

        private async void MenuItem_Tests_TestOpenProcess(object sender, RoutedEventArgs e)
        {
            bool success = Gtr2MemOps.TestGtr2OpenProcess();
            if (success)
            {
                App.Log.AddInfo("Test Pass: Test GTR2 Open Process");
            }
            else
            {
                App.Log.AddError("Test Failed: Test GTR2 Open Process");
            }
        }

        private async void MenuItem_Help_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("GTR2 Memory Operations Tool\nVersion 1.0 (WIP)", "About");
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void MenuItem_Tests_TestSharedMemory(object sender, RoutedEventArgs e)
        {
            // Activate Shared Memory tab so user can see the results of the test there
            //MainTabControl.SelectedItem = SharedMemoryTab;
            //MainTabControl.UpdateLayout(); // Ensure the UI updates before running the test
            await Task.Yield(); // Allow the UI to update before running the test when auto-selecting the tab
            SharedMemoryView.TestGtr2SharedMemory();
            App.Log.AddInfo("Test Completed: View the results in the Shared Memory tab");

        }

        private async void MenuItem_Help_SupportMyWork_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.simwiki.net/wiki/Help_Support_Simwiki") { UseShellExecute = true });
        }

        private async void MenuItem_Process_Connect_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => CheckGtr2Process());
        }

        private async void MenuItem_Process_Check_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => CheckGtr2Process());
        }
    }
}