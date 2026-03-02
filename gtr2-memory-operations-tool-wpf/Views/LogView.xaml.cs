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

namespace gtr2_memory_operations_tool_wpf.Views
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
            App.Log.EntryAdded += OnEntryAdded;
            App.Log.Add("GTR2 Memory Operations Tool Log\n");
            //LogBox.AppendText("GTR2 Memory Operations Tool Log\n");
        }
        private void OnEntryAdded(string message)
        {
            Dispatcher.Invoke(() => LogBox.AppendText(message));
        }
    }
}
