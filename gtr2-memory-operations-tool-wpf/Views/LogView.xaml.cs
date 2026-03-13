using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
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

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
            LogBox.TextChanged += LogBox_TextChanged;
            App.LogObj.EntryAdded += OnEntryAdded;
        }
        private void LogBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ( LogBox.IsFocused ) { // Auto-scrolling when the user is interacting with the control would be annoying
                return;
            }
            LogBox.ScrollToEnd();
        }
        private void OnEntryAdded(string message, Log.LogLevel loggingLevel)
        {
            if (loggingLevel < App.Log.LoggingLevel)
            {
                return;
            }
            Dispatcher.Invoke(() => LogBox.AppendText(message));
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogBox.Clear();
        }

        private void CopyLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogBox.SelectAll();
            LogBox.Copy();
            //LogBox.Select(0, 0);
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"GTR2_Memory_Operations_Tool_Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, LogBox.Text);
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
            switch ( selectedTag )
            {
                //case "All":
                //    App.Log.LoggingLevel = Log.LogLevel.Debug;
                //    break;
                case "debug":
                    App.Log.LoggingLevel = Log.LogLevel.Debug;
                    break;
                case "info":
                    App.Log.LoggingLevel = Log.LogLevel.Info;
                    break;
                case "warning":
                    App.Log.LoggingLevel = Log.LogLevel.Warning;
                    break;
                case "error":
                    App.Log.LoggingLevel = Log.LogLevel.Error;
                    break;
                case "exception":
                    App.Log.LoggingLevel = Log.LogLevel.Exception;
                    break;
                default:
                    App.Log.AddError($"Unexpected log filter selected: {selectedTag}");
                    break;
            }
        }
    }
}
