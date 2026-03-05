using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<SharedMemoryItem> SharedMemoryItems { get; set; } = new ObservableCollection<SharedMemoryItem>();
        public SharedMemoryView()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void TestGtr2SharedMemory()
        {
            MappedBuffer<Gtr2Telemetry> telemetryBuffer = new MappedBuffer<Gtr2Telemetry>(Gtr2Constants.MM_TELEMETRY_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
            MappedBuffer<Gtr2Scoring> scoringBuffer = new MappedBuffer<Gtr2Scoring>(Gtr2Constants.MM_SCORING_FILE_NAME, true  /*partial*/, true /*skipUnchanged*/);
            MappedBuffer<Gtr2Extended> extendedBuffer = new MappedBuffer<Gtr2Extended>(Gtr2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);

            // Marshalled views:
            Gtr2Telemetry telemetry = new Gtr2Telemetry();
            Gtr2Scoring scoring = new Gtr2Scoring();
            Gtr2Extended extended = new Gtr2Extended();

            try
            {
                // Extended buffer is the last one constructed, so it is an indicator GTR2SM is ready.
                extendedBuffer.Connect();
                telemetryBuffer.Connect();
                scoringBuffer.Connect();

                extendedBuffer.GetMappedData(ref extended);
                scoringBuffer.GetMappedData(ref scoring);
                telemetryBuffer.GetMappedData(ref telemetry);

                App.Log.AddDebug($"Telemetry mVersion: {Utilities.GetStringFromBytes(extended.mVersion)}");

                SharedMemoryItems.Clear();
                foreach (FieldInfo field in typeof(Gtr2Extended).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    // Skip JsonIgnore fields
                    if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                    
                    object? value = field.GetValue(extended);
                    if ( value == null) continue;
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

                    SharedMemoryItems.Add(new SharedMemoryItem(field.Name, displayValue, field.FieldType.Name));
                }

                App.Log.AddInfo("Test Pass: Test Shared Memory");
            }
            catch (Exception)
            {
                App.Log.AddError("Test Failed: Test Shared Memory");
                try
                {
                    extendedBuffer.Disconnect();
                    telemetryBuffer.Disconnect();
                    scoringBuffer.Disconnect(); ;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }
    }
}
