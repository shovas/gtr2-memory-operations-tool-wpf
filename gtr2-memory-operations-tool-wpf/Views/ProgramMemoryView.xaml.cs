using Gtr2MemOpsTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ProgramMemoryView.xaml
    /// </summary>
    public partial class ProgramMemoryView : UserControl
    {
        public IEnumerable<MemoryItem> MemoryItems { get; set; } = [];

        public ProgramMemoryView()
        {
            InitializeComponent();
        }

        private void SearchFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(MemoryItems);
            view.Filter = item =>
            {
                var memoryItem = (MemoryItem)item;
                if(memoryItem.Name.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase)){
                    return true;
                }
                else
                {
                    string? valueAsString = memoryItem.ValueAsString;
                    if(valueAsString is not null)
                    {
                        if (valueAsString.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                return false;
            };
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //bool success = Gtr2MemOps.TestGtr2Process();
            //if (success)
            //{
            //    App.Log.AddInfo("Test Pass: Test GTR2 Process");
            //}
            //else
            //{
            //    App.Log.AddError("Test Failed: Test GTR2 Process");
            //}
        }
    }
}
