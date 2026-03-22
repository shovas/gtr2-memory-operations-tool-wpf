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
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public IEnumerable<SettingItem> SettingItems { get; set; } = [];

        public SettingsView()
        {
            InitializeComponent();
        }

        private void SearchFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(SettingItems);
            view.Filter = item =>
            {
                var settingItem = (SettingItem)item;
                return
                    settingItem.Key.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    settingItem.Value.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    settingItem.Description.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase)
                ;
            };
        }
    }
}
