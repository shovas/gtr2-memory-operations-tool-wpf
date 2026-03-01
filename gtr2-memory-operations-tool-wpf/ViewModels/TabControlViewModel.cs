using CommunityToolkit.Mvvm.ComponentModel;
using gtr2_memory_operations_tool_wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace gtr2_memory_operations_tool_wpf.ViewModels
{
    internal class TabControlViewModel : ObservableObject
    {
        public ObservableCollection<TabItem> TabItems { get; set; }
        public TabControlViewModel()
        {
            //TabItems = new ObservableCollection<TabItem>();
            //var home = new TabItem { Header = "Home", Content = new HomeView() };
            //var data = new TabItem { Header = "Data", Content = new DataView() };
            //var log = new TabItem { Header = "Log", Content = new LogView() };
            //TabItems.Add(home);
            //TabItems.Add(data);
            //TabItems.Add(log);
        }
    }
}
