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
        public IEnumerable<MemoryItem> MemoryItems { get; set; } = [];
        public BulkObservableCollection<MemoryItem> ProgramMemoryItems { get; set; } = [];

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
                AddProgramMemoryItems([.. items]);
            }); // runs on UI thread
            await Task.Run(() => LoadItems(progress));
            RefreshButton.IsEnabled = true;
        }

        public void AddProgramMemoryItem(MemoryItem item)
        {
            ProgramMemoryItems.Add(item);
        }
        public void AddProgramMemoryItems(MemoryItem[] items)
        {
            ProgramMemoryItems.AddRange(items);
        }

        private void LoadItems(IProgress<List<MemoryItem>> progress)
        {
            int batchLimit = 50; // TODO: Make this a setting
            var batch = new List<MemoryItem>();
            //List<SharedMemoryItem> items = GetGtr2SharedMemoryItems();
            //foreach (var item in items) // your slow data source
            foreach (var item in GetGtr2ProgramMemoryItems()) // your slow data source
            {
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

        public IEnumerable<MemoryItem> GetGtr2ProgramMemoryItems()
        {
            App.Log.AddInfo("Getting GTR2 Program Memory Items");

            var memoryItems = Gtr2MemOps.GetGtr2ProgramMemoryItems();
            foreach (var memoryItem in memoryItems)
            {
                App.Log.AddDebug($"Yielding memory item: {memoryItem.Offset}, {memoryItem.Name}, {memoryItem.HeldType}, {memoryItem.Length}");
                yield return memoryItem;
            }

            App.Log.AddInfo("Finished Getting GTR2 Program Memory Items");
        }
    }
}
