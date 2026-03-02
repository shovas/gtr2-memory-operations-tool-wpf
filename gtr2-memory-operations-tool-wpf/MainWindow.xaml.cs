using System;
using System.Collections.Generic;
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
        }

        private void MenuItem_File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Actions_Test_TestGTR2Process(object sender, RoutedEventArgs e)
        {
            bool success = Gtr2MemOps.TestGtr2Process();
            if (success)
            {
                App.Log.Add("Test Pass: Test GTR2 Process");
            }
            else
            {
                App.Log.Add("Test Failed: Test GTR2 Process");
            }
        }

        private void MenuItem_Actions_Test_TestGetProcess(object sender, RoutedEventArgs e)
        {
            bool success = Gtr2MemOps.TestGtr2GetProcess();
            if (success)
            {
                App.Log.Add("Test Pass: Test GTR2 Open Process");
            }
            else
            {
                App.Log.Add("Test Failed: Test GTR2 Open Process");
            }
        }

        private void MenuItem_Actions_Test_TestOpenProcess(object sender, RoutedEventArgs e)
        {
            bool success = Gtr2MemOps.TestGtr2OpenProcess();
            if (success)
            {
                App.Log.Add("Test Pass: Test GTR2 Open Process");
            }
            else
            {
                App.Log.Add("Test Failed: Test GTR2 Open Process");
            }
        }

        private void MenuItem_Help_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("GTR2 Memory Operations Tool\nVersion 1.0 (WIP)", "About");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}