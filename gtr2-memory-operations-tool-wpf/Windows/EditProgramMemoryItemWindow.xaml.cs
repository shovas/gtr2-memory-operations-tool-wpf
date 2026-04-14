using Gtr2MemOpsTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gtr2MemOpsTool.Windows
{
    

    /// <summary>
    /// Interaction logic for EditProgramMemoryWindow.xaml
    /// </summary>
    public partial class EditProgramMemoryItemWindow : Window, INotifyPropertyChanged
    {
        private string _value = "";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public string EditedValue { get; private set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public EditProgramMemoryItemWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EditProgramMemoryItemWindow(MemoryItem item) : this()
        {
            Value = item.ValueAsString ?? "";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            EditedValue = Value;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
        }
    }
}
