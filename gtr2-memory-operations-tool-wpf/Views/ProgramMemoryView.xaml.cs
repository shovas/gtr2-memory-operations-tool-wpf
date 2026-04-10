using Gtr2MemOpsTool.Helpers;
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
        //public IEnumerable<MemoryItem> MemoryItems { get; set; } = [];
        public BulkObservableCollection<MemoryItem> ProgramMemoryItems { get; set; } = [];

        public ProgramMemoryView()
        {
            InitializeComponent();
        }

        private void SearchFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ProgramMemoryItems);
            view.Filter = item =>
            {
                var memoryItem = (MemoryItem)item;
                if (memoryItem.Name.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    string? valueAsString = memoryItem.ValueAsString;
                    if (valueAsString is not null)
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            ProgramMemoryItems.Clear();
            var progress = new Progress<List<MemoryItem>>(items =>
            {
                //App.Log.AddDebug("Loading items");
                AddProgramMemoryItems([.. items]);
            }); // runs on UI thread
            await Task.Run(() => LoadItems(progress));
            RefreshButton.IsEnabled = true;
        }

        public void AddProgramMemoryItem(MemoryItem item)
        {
            //App.Log.AddDebug("AddProgramMemoryItem()");
            ProgramMemoryItems.Add(item);
        }
        public void AddProgramMemoryItems(MemoryItem[] items)
        {
            //App.Log.AddDebug("AddProgramMemoryItems()");
            ProgramMemoryItems.AddRange(items);
        }

        private static void LoadItems(IProgress<List<MemoryItem>> progress)
        {
            int batchLimit = 50;
            var batch = new List<MemoryItem>();
            foreach (var item in Gtr2MemOps.GetGtr2ProgramMemoryItems()) // your slow data source
            {
                //App.Log.AddDebug($"Loaded item: {item.Name} at offset {item.Offset} with length {item.Length}");
                batch.Add(item);
                if (batch.Count < batchLimit)
                {
                    continue;
                }
                // do heavy work per item...
                progress.Report(batch); // marshals back to UI thread safely
                batch = []; // Using batch.Clear() clears batch before progress.Report() finishes updating the UI
            }
            progress.Report(batch);
        }

        private void ValueTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            tb?.Focus();
            e.Handled = false;
        }
    }
}
