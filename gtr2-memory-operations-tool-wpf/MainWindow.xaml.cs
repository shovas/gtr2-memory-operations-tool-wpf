using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Text;
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

namespace gtr2_memory_operations_tool_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Gtr2MemOps Gtr2MemOps { get; set; } = new Gtr2MemOps();
        public MainWindow()
        {
            InitializeComponent();
            
            InitializeGtr2MemoryOperationsTool();

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
            int timerTime = 5;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(timerTime) };
            timer.Tick += (s, e) =>
            {
                StatusBarItemText.Text = "Ready";
                timer.Stop();
            };
            timer.Start();
        }

        private void CheckGtr2Process()
        {
            bool isRunning = Gtr2MemOps.IsGtr2ProcessRunning();
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

        private void MenuItem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Tests_TestGTR2Process(object sender, RoutedEventArgs e)
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

        private void MenuItem_Tests_TestGetProcess(object sender, RoutedEventArgs e)
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

        private void MenuItem_Tests_TestOpenProcess(object sender, RoutedEventArgs e)
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

        private void MenuItem_Help_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("GTR2 Memory Operations Tool\nVersion 1.0 (WIP)", "About");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void MenuItem_Tests_TestSharedMemory(object sender, RoutedEventArgs e)
        {
            // Activate Shared Memory tab so user can see the results of the test there
            MainTabControl.SelectedItem = SharedMemoryTab;
            //MainTabControl.UpdateLayout(); // Ensure the UI updates before running the test
            await Task.Yield();
            SharedMemoryView.TestGtr2SharedMemory();
            
        }

        private void MenuItem_Help_SupportMyWork_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.simwiki.net/wiki/Help_Support_Simwiki") { UseShellExecute = true });
        }

        private void MenuItem_Process_Connect_Click(object sender, RoutedEventArgs e)
        {
            CheckGtr2Process();
        }

        private void MenuItem_Process_Check_Click(object sender, RoutedEventArgs e)
        {
            CheckGtr2Process();
        }
    }
}