using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Models;
using Gtr2MemOpsTool.ViewModels;
using System;
using System.Collections.Generic;
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
        //public BulkObservableCollection<AaiDriver> AaiDrivers { get; set; } = [];

        public AutomaticAiView()
        {
            InitializeComponent();
            DataContext = new AaiDriverViewModel();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDrivers();
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

        private void LoadDrivers()
        {

            // Todo: Use Gtr2MemOps functions to open the process and read the Grid Slots in as AaiDriver objects, then add them to the AaiDrivers collection.

        }
    }
}
