using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
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

namespace gtr2_memory_operations_tool_wpf.Views
{
    /// <summary>
    /// Interaction logic for SharedMemoryView.xaml
    /// </summary>
    public partial class SharedMemoryView : UserControl
    {
        private MappedBuffer<Gtr2Telemetry> TelemetryBuffer;
        private MappedBuffer<Gtr2Scoring> ScoringBuffer;
        private MappedBuffer<Gtr2Extended> ExtendedBuffer;
        public ObservableCollection<SharedMemoryItem> SharedMemoryItems { get; set; } = new ObservableCollection<SharedMemoryItem>();
        public SharedMemoryView()
        {
            InitializeComponent();
            DataContext = this;
            TelemetryBuffer = new MappedBuffer<Gtr2Telemetry>(Gtr2Constants.MM_TELEMETRY_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
            ScoringBuffer = new MappedBuffer<Gtr2Scoring>(Gtr2Constants.MM_SCORING_FILE_NAME, true  /*partial*/, true /*skipUnchanged*/);
            ExtendedBuffer = new MappedBuffer<Gtr2Extended>(Gtr2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
        }
        public void TestGtr2SharedMemory()
        {

            // Marshalled views:
            Gtr2Telemetry telemetry = new Gtr2Telemetry();
            Gtr2Scoring scoring = new Gtr2Scoring();
            Gtr2Extended extended = new Gtr2Extended();

            try
            {
                // Extended buffer is the last one constructed, so it is an indicator GTR2SM is ready.
                ExtendedBuffer.Connect();
                TelemetryBuffer.Connect();
                ScoringBuffer.Connect();

                ExtendedBuffer.GetMappedData(ref extended);
                ScoringBuffer.GetMappedData(ref scoring);
                TelemetryBuffer.GetMappedData(ref telemetry);

                App.Log.AddDebug($"Telemetry mVersion: {Utilities.GetStringFromBytes(extended.mVersion)}");

                // For debugging, log all fields and values of the structs
                SharedMemoryItems.Clear();
                var structsList = new List<IGtr2Struct> { telemetry, scoring, extended };
                foreach ( var structItem in structsList)
                {
                    string structName = structItem.GetType().Name;
                    App.Log.AddDebug($"Struct: {structName}");
                    foreach (FieldInfo field in structItem.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        // Skip JsonIgnore fields
                        if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                        object? value = field.GetValue(structItem);
                        if (value == null) continue;
                        string displayValue;

                        App.Log.AddDebug($"Processing field: {field.Name}, Type: {field.FieldType.Name}, Value: {value}");

                        if (field.FieldType == typeof(byte[]))
                            displayValue = Encoding.UTF8.GetString((byte[])value).TrimEnd('\0');
                        else if (field.FieldType.IsArray)
                            displayValue = $"[Array: {field.FieldType.GetElementType()?.Name}]"; // skip or handle separately
                        else if (field.FieldType.IsValueType && !field.FieldType.IsPrimitive && field.FieldType.IsLayoutSequential)
                            displayValue = $"[Struct: {field.FieldType.Name}]"; // nested struct
                        else
                            displayValue = value?.ToString() ?? "null";

                        SharedMemoryItems.Add(new SharedMemoryItem(structName, field.Name, displayValue, field.FieldType.Name));
                    }
                }
 //SharedMemoryItems.Clear();
                //foreach (FieldInfo field in typeof(Gtr2Extended).GetFields(BindingFlags.Public | BindingFlags.Instance))
                //{
                //    // Skip JsonIgnore fields
                //    if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                //    object? value = field.GetValue(extended);
                //    if (value == null) continue;
                //    string displayValue;

                //    App.Log.AddDebug($"Processing field: {field.Name}, Type: {field.FieldType.Name}, Value: {value}");

                //    if (field.FieldType == typeof(byte[]))
                //        displayValue = Encoding.UTF8.GetString((byte[])value).TrimEnd('\0');
                //    else if (field.FieldType.IsArray)
                //        displayValue = $"[Array: {field.FieldType.GetElementType()?.Name}]"; // skip or handle separately
                //    else if (field.FieldType.IsValueType && !field.FieldType.IsPrimitive && field.FieldType.IsLayoutSequential)
                //        displayValue = $"[Struct: {field.FieldType.Name}]"; // nested struct
                //    else
                //        displayValue = value?.ToString() ?? "null";

                //    SharedMemoryItems.Add(new SharedMemoryItem(field.Name, displayValue, field.FieldType.Name));
                //}
               

                App.Log.AddInfo("Test Pass: Test Shared Memory");
            }
            catch (Exception)
            {
                App.Log.AddError("Test Failed: Test Shared Memory");
                try
                {
                    ExtendedBuffer.Disconnect();
                    TelemetryBuffer.Disconnect();
                    ScoringBuffer.Disconnect(); ;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }

        private void KeyFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(SharedMemoryItems);
            view.Filter = item =>
            {
                var sharedMemoryItem = (SharedMemoryItem)item;
                return sharedMemoryItem.Key.Contains(KeyFilterBox.Text, StringComparison.OrdinalIgnoreCase);
            };
            //    = obj => {
            //    var p = (Person)obj;
            //    return p.Name
            //      .Contains(searchBox.Text,
            //        StringComparison.OrdinalIgnoreCase);
            //};
        }

        private void DataFilterSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataFilterSelector is null)
            {
                return;
            }
            if (DataFilterSelector.SelectedItem is null)
            {
                return;
            }
            ComboBoxItem selectedItem = (ComboBoxItem)DataFilterSelector.SelectedItem;
            if (selectedItem.Content is null)
            {
                return;
            }
            if ( selectedItem.Content.ToString() is null)
            {
                return; 
            }
            string filter = selectedItem.Tag.ToString()!;
            ICollectionView view = CollectionViewSource.GetDefaultView(SharedMemoryItems);
            if (filter == "All")
            {
                view.Filter = null; // Show all items
            }
            else
            {
                view.Filter = item =>
                {
                    var sharedMemoryItem = (SharedMemoryItem)item;
                    return sharedMemoryItem.StructName == filter;
                };
            }
        }
    }
}
