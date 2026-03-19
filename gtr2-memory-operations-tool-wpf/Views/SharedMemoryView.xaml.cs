using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
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

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for SharedMemoryView.xaml
    /// </summary>
    public partial class SharedMemoryView : UserControl
    {
        private MappedBuffer<Gtr2Telemetry> TelemetryBuffer;
        private MappedBuffer<Gtr2Scoring> ScoringBuffer;
        private MappedBuffer<Gtr2Extended> ExtendedBuffer;
        private Gtr2Telemetry Gtr2Telemetry;
        private Gtr2Scoring Gtr2Scoring;
        private Gtr2Extended Gtr2Extended;
        public BulkObservableCollection<SharedMemoryItem> SharedMemoryItems { get; set; } = new BulkObservableCollection<SharedMemoryItem>();
        public SharedMemoryView()
        {
            InitializeComponent();
            DataContext = this;
            TelemetryBuffer = new MappedBuffer<Gtr2Telemetry>(Gtr2Constants.MM_TELEMETRY_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
            ScoringBuffer = new MappedBuffer<Gtr2Scoring>(Gtr2Constants.MM_SCORING_FILE_NAME, true  /*partial*/, true /*skipUnchanged*/);
            ExtendedBuffer = new MappedBuffer<Gtr2Extended>(Gtr2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
        }
        public void AddSharedMemoryItem ( SharedMemoryItem item )
        {
            int index = SharedMemoryItems.Count;
            item.Index = index;
            SharedMemoryItems.Add(item);
        }
        public void AddSharedMemoryItems(SharedMemoryItem[] items)
        {
            int index = SharedMemoryItems.Count;
            foreach ( var item in items )
            {
                item.Index = index;
                index++;
            }
            SharedMemoryItems.AddRange(items);
        }
        public void TestGtr2SharedMemory()
        {
            App.Log.AddInfo("Testing GTR2 Shared Memory");
            try
            {
                MarshalGtr2MemoryRegions();

                ConnectGtr2MemoryBuffers();
                
                ReadGtr2MemoryBuffers();

                App.Log.AddDebug($"Telemetry mVersion: {Utilities.GetStringFromBytes(Gtr2Extended.mVersion)}");

                DisplayMemoryStructs();

                App.Log.AddInfo("Test Pass: Test Shared Memory");
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
                App.Log.AddError("Test Failed: Test Shared Memory");
                try
                {
                    DisconnectGtr2MemoryBuffers();
                }
                catch (Exception)
                {
                    // Ignore
                    App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
                }
            }
        }

        private void MarshalGtr2MemoryRegions()
        {
            Gtr2Telemetry = new Gtr2Telemetry();
            Gtr2Scoring = new Gtr2Scoring();
            Gtr2Extended = new Gtr2Extended();
        }

        private void ConnectGtr2MemoryBuffers()
        {
            // Extended buffer is the last one constructed, so it is an indicator GTR2SM is ready.
            ExtendedBuffer.Connect();
            TelemetryBuffer.Connect();
            ScoringBuffer.Connect();
        }

        private void DisconnectGtr2MemoryBuffers()
        {
            ExtendedBuffer.Disconnect();
            TelemetryBuffer.Disconnect();
            ScoringBuffer.Disconnect(); ;
        }

        private void ReadGtr2MemoryBuffers()
        {
            ExtendedBuffer.GetMappedData(ref Gtr2Extended);
            ScoringBuffer.GetMappedData(ref Gtr2Scoring);
            TelemetryBuffer.GetMappedData(ref Gtr2Telemetry);
        }

        private void DisplayMemoryStructs()
        {
            // Loop over all structs and their fields, and add them to the SharedMemoryItems collection for display
            SharedMemoryItems.Clear();
            var structsList = new List<IGtr2Struct> {
                    Gtr2Telemetry,
                    Gtr2Scoring,
                    Gtr2Extended
                };
            foreach (var structItem in structsList)
            {
                App.Log.AddDebug($"Processing struct: {structItem}");
                // Get structu fields

                // Display struct fields
                DisplayMemoryStruct(structItem);
            }
        }

        public IEnumerable<SharedMemoryItem> GetGtr2SharedMemoryItems()
        {
            App.Log.AddInfo("Getting GTR2 Shared Memory Items");
            //try
            //{
                MarshalGtr2MemoryRegions();

                ConnectGtr2MemoryBuffers();

                ReadGtr2MemoryBuffers();

                App.Log.AddDebug($"Telemetry mVersion: {Utilities.GetStringFromBytes(Gtr2Extended.mVersion)}");

                var structs = GetMemoryStructs();
                foreach (var structItem in structs ) {
                    App.Log.AddDebug($"Yielding struct item: {structItem.StructName}.{structItem.Key} = {structItem.Value} ({structItem.Type})");
                    yield return structItem;
                }

                App.Log.AddInfo("Finished Getting GTR2 Shared Memory Items");
            //}
            //catch (Exception ex)
            //{
            //    App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
            //    App.Log.AddError("Failed Getting GTR2 Shared Memory Items");
            //    try
            //    {
            //        DisconnectGtr2MemoryBuffers();
            //    }
            //    catch (Exception)
            //    {
            //        App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
            //    }
            //}
        }

        private IEnumerable<SharedMemoryItem> GetMemoryStructs()
        {
            // Loop over all structs and their fields, and add them to the SharedMemoryItems collection for display
            var structsList = new List<IGtr2Struct> {
                    Gtr2Telemetry,
                    Gtr2Scoring,
                    Gtr2Extended
                };
            //List<SharedMemoryItem> items = [];
            foreach (var structsItem in structsList)
            {
                App.Log.AddDebug($"Processing struct: {structsItem}");
                // Get struct fields

                // Get struct fields
                var structItems = GetMemoryStruct(structsItem);
                foreach (var structItem in structItems)
                {
                    yield return structItem;
                }
                
                //items.AddRange( structItems );
            }
            //return items;
        }

        private List<SharedMemoryItem> GetMemoryStruct(IGtr2Struct structItem)
        {
            App.Log.AddDebug($"Getting struct: {structItem}");
            string structName = structItem.GetType().Name; 
            App.Log.AddDebug($"Struct: {structName}"); //String.Format($"Struct: {0}", structItem.GetType.Name) // how you do printf like formatting in C#
            FieldInfo[] structItemFields = structItem.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            List<SharedMemoryItem> items = GetMemoryStructFields(structItemFields, structItem);
            return items;
        }

        private List<SharedMemoryItem> GetMemoryStructFields(FieldInfo[] structItemFields, IGtr2Struct structItem)
        {
            App.Log.AddDebug($"Getting fields for struct: {structItem}");
            List <SharedMemoryItem> items = new List<SharedMemoryItem>();
            foreach (FieldInfo structItemField in structItemFields)
            {
                // Skip JsonIgnore fields
                if (structItemField.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                List<SharedMemoryItem> nestedItems = GetMemoryStructField(structItemField, structItem);
                if (nestedItems.Count == 0)
                {
                    continue;
                }
                items.AddRange(nestedItems);
            }
            return items;
        }

        private List<SharedMemoryItem> GetMemoryStructField(FieldInfo structItemField, IGtr2Struct parentStructItem)
        {
            App.Log.AddDebug($"Getting field {structItemField} for struct {parentStructItem}");
            object? structItemFieldValue = structItemField.GetValue(parentStructItem);
            if (structItemFieldValue == null) return new List<SharedMemoryItem>();
            string parentStructName = parentStructItem.GetType().Name;

            App.Log.AddDebug($"Processing field: {structItemField.Name}, Type: {structItemField.FieldType.Name}, Value: {structItemFieldValue}");

            List<SharedMemoryItem> items = new List<SharedMemoryItem>();
            if (structItemFieldValue is IGtr2Struct)
            {
                // IGtr2Struct instances
                App.Log.AddDebug($"Field {structItemField.Name} is a IGtr2Struct: {structItemField}");

                string fieldValueString = $"IGtr2Struct: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldValueString, structItemField.FieldType.Name);
                items.Add(memoryItem);

                List <SharedMemoryItem> nestedItems =
                    GetMemoryStruct((IGtr2Struct)structItemFieldValue); // Recursively process nested struct fields

                items.AddRange(nestedItems);
            }
            else if (structItemFieldValue is IGtr2Struct[]) // Checks don't work this way for structs in C#. Fix is to catch FieldType.IsArray(), loop over the items, and check each item.
            //else if ( structItemField.FieldType == typeof(IGtr2Struct[]) )
            {
                // IGtr2Struct arrays
                App.Log.AddDebug($"Field {structItemField.Name} is an IGtr2Struct[]: {structItemField}");

                string fieldValueString = $"IGtr2Struct[]: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldValueString, structItemField.FieldType.Name);
                items.Add(memoryItem);

                IGtr2Struct[] iGtr2Structs = (IGtr2Struct[])structItemFieldValue;
                foreach (IGtr2Struct iGtr2Struct in iGtr2Structs)
                {
                    FieldInfo[] fields = structItemField.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    List<SharedMemoryItem> nestedItems =
                        GetMemoryStructFields(fields, iGtr2Struct);
                    items.AddRange(nestedItems);
                }
            }
            else if (structItemField.FieldType.IsValueType && !structItemField.FieldType.IsPrimitive && structItemField.FieldType.IsLayoutSequential) // This condition should not occur. We're not expecting non-IGtr2Struct structs.
            {
                // Structs: value type that is not a primitive and has sequential layout
                // - Value types: Directly holds a value not a reference to it; Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean, and structs
                // - Primitives: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean
                // - Sequential layout: Fields are laid out in memory in the order they are defined, with possible padding for alignment; typical for C-style structs
                //   - A class with [StructLayout(LayoutKind.Sequential)] would not be caught here because IsValueType would be false for it.
                App.Log.AddDebug($"Field {structItemField.Name} is an unexpected struct with sequential layout: {structItemField}");

                string fieldString = $"[Unexpected struct: {structItemField.FieldType.Name}]";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldString, structItemField.FieldType.Name);
                items.Add(memoryItem);
            }
            else if (structItemField.FieldType == typeof(byte[]))
            {
                // Byte arrays
                App.Log.AddDebug($"Field {structItemField.Name} is a byte array: {structItemField}");

                byte[] fieldValueBytes = (byte[])structItemFieldValue;
                Encoding encoding = Encoding.GetEncoding(Gtr2MemOps.GTR2_ENCODING_CODEPAGE);
                string fieldValue = encoding.GetString(fieldValueBytes).TrimEnd('\0');
                string fieldValueString = $"byte[]: {fieldValue}";

                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldValueString, structItemField.FieldType.Name);
                items.Add(memoryItem);
            }
            else if (structItemField.FieldType.IsArray) // Caution: This matches byte[] too so if-else order becomes important
            {
                // Arrays
                App.Log.AddDebug($"Field {structItemField.Name} is an array: {structItemField}");

                string fieldValueString = $"Array: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldValueString, structItemField.FieldType.Name);
                items.Add(memoryItem);

                Array fieldItemValues = (Array)structItemFieldValue;
                foreach (object fieldItemValue in fieldItemValues)
                {
                    if (fieldItemValue is IGtr2Struct)
                    {
                        string fieldItemValueString = $"IGtr2Struct: {fieldItemValue.ToString()}";
                        SharedMemoryItem newMemoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldItemValueString, structItemField.FieldType.Name);
                        items.Add(newMemoryItem);

                        IGtr2Struct fieldItemValueStruct = (IGtr2Struct)fieldItemValue;
                        FieldInfo[] fieldItemValueStructFields = fieldItemValueStruct.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                        ////////////////////////////////////////////////////////
                        // fixme? We are processing the fields of the struct here, but we are not adding the struct itself as a SharedMemoryItem to the list. This means the struct will not be displayed in the UI, only its fields will be. We may want to add a SharedMemoryItem for the struct itself before processing its fields, similar to how we do it in DisplayMemoryStructField.
                        // Not sure about this though
                        ////////////////////////////////////////////////////////

                        List<SharedMemoryItem> nestedItems = GetMemoryStruct(fieldItemValueStruct);
                        items.AddRange(nestedItems);
                    }
                    else
                    {
                        string fieldItemValueString = $"Unexpected type in array: {fieldItemValue.ToString()}";
                        SharedMemoryItem newMemoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldItemValueString, structItemField.FieldType.Name);
                        items.Add(newMemoryItem);
                    }
                }
            }
            else if (structItemField.FieldType.IsPrimitive)
            {
                // Primitives: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean
                App.Log.AddDebug($"Field {structItemField.Name} is a primitive: {structItemField}");

                string fieldValueString = $"Primitive: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldValueString, structItemField.FieldType.Name);
                items.Add(memoryItem);
            }
            else
            {
                App.Log.AddDebug($"Field {structItemField.Name} is an unexpected type: {structItemField}");
                // Primitives: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean
                string fieldValueString = $"Unexpected type: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldValueString, structItemField.FieldType.Name);
                items.Add(memoryItem);
            }
            return items;
        }

        private void DisplayMemoryStruct (IGtr2Struct structItem)
        {
            App.Log.AddDebug($"Displaying struct: {structItem}");
            string structName = structItem.GetType().Name;
            //String.Format($"Struct: {0}", structItem.GetType.Name) // how you do printf like formatting in C#
            App.Log.AddDebug($"Struct: {structName}");
            FieldInfo[] structItemFields = structItem.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            DisplayMemoryStructFields(structItemFields, structItem);
        }

        private void DisplayMemoryStructFields(FieldInfo[] structItemFields, IGtr2Struct structItem)
        {
            App.Log.AddDebug($"Displaying fields for struct: {structItem}");
            foreach (FieldInfo structItemField in structItemFields)
            {
                // Skip JsonIgnore fields
                if (structItemField.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
                DisplayMemoryStructField(structItemField, structItem);
            }
        }

        

        private void DisplayMemoryStructField(FieldInfo structItemField, IGtr2Struct parentStructItem)
        {
            App.Log.AddDebug($"Displaying field {structItemField} for struct {parentStructItem}");
            object? structItemFieldValue = structItemField.GetValue(parentStructItem);
            if (structItemFieldValue == null) return;
            string parentStructName = parentStructItem.GetType().Name;

            App.Log.AddDebug($"Processing field: {structItemField.Name}, Type: {structItemField.FieldType.Name}, Value: {structItemFieldValue}");

            if (structItemFieldValue is IGtr2Struct)
            {
                // IGtr2Struct instances
                App.Log.AddDebug($"Field {structItemField.Name} is a IGtr2Struct: {structItemField}");

                string displayValue = $"IGtr2Struct: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);

                DisplayMemoryStruct((IGtr2Struct)structItemFieldValue); // Recursively display nested struct fields
            }
            else if (structItemFieldValue is IGtr2Struct[]) // Checks don't work this way for structs in C#. Fix is to catch FieldType.IsArray(), loop over the items, and check each item.
            //else if ( structItemField.FieldType == typeof(IGtr2Struct[]) )
            {
                // IGtr2Struct arrays
                App.Log.AddDebug($"Field {structItemField.Name} is an IGtr2Struct[]: {structItemField}");

                string displayValue = $"IGtr2Struct[]: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);

                IGtr2Struct[] iGtr2Structs = (IGtr2Struct[])structItemFieldValue;
                foreach (IGtr2Struct iGtr2Struct in iGtr2Structs)
                {
                    FieldInfo[] fields = structItemField.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    DisplayMemoryStructFields(fields, iGtr2Struct);
                }
            }
            else if (structItemField.FieldType.IsValueType && !structItemField.FieldType.IsPrimitive && structItemField.FieldType.IsLayoutSequential) // This condition should not occur. We're not expecting non-IGtr2Struct structs.
            {
                // Structs: value type that is not a primitive and has sequential layout
                // - Value types: Directly holds a value not a reference to it; Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean, and structs
                // - Primitives: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean
                // - Sequential layout: Fields are laid out in memory in the order they are defined, with possible padding for alignment; typical for C-style structs
                //   - A class with [StructLayout(LayoutKind.Sequential)] would not be caught here because IsValueType would be false for it.
                App.Log.AddDebug($"Field {structItemField.Name} is an unexpected struct with sequential layout: {structItemField}");

                string displayValue = $"[Unexpected struct: {structItemField.FieldType.Name}]";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);
            }
            else if (structItemField.FieldType == typeof(byte[]))
            {
                // Byte arrays
                App.Log.AddDebug($"Field {structItemField.Name} is a byte array: {structItemField}");

                byte[] fieldValueBytes = (byte[])structItemFieldValue;
                Encoding encoding = Encoding.GetEncoding(Gtr2MemOps.GTR2_ENCODING_CODEPAGE);
                string fieldValue = encoding.GetString(fieldValueBytes).TrimEnd('\0');
                string displayValue = $"byte[]: {fieldValue}";
                //displayValue = "typeof(byte[]):" + Encoding.UTF8.GetString((byte[])structItemFieldValue).TrimEnd('\0');
                //displayValue = GetByteArrayString((byte[])structItemFieldValue);
                //DisplayMemoryStructFieldByteArray(structItem, field, (byte[])obj);

                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);
            }
            else if (structItemField.FieldType.IsArray) // Caution: This matches byte[] too so if-else order becomes important
            {
                // Arrays
                App.Log.AddDebug($"Field {structItemField.Name} is an array: {structItemField}");

                string displayValue = $"Array: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);

                Array fieldItemValues = (Array)structItemFieldValue;
                foreach (object fieldItemValue in fieldItemValues)
                {
                    if (fieldItemValue is IGtr2Struct)
                    {
                        string fieldItemValueDisplayValue = $"IGtr2Struct: {fieldItemValue.ToString()}";
                        SharedMemoryItem newMemoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldItemValueDisplayValue, structItemField.FieldType.Name);
                        AddSharedMemoryItem(newMemoryItem);

                        IGtr2Struct fieldItemValueStruct = (IGtr2Struct)fieldItemValue;
                        FieldInfo[] fieldItemValueStructFields = fieldItemValueStruct.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                        //DisplayMemoryStructFields(fieldItemValueStructFields, fieldItemValueStruct);

                        ////////////////////////////////////////////////////////
                        // fixme
                        ////////////////////////////////////////////////////////

                        //DisplayMemoryStruct(fieldItemValueStruct);
                    }
                    else
                    {
                        string fieldItemValueDisplayValue = $"Unexpected type in array: {fieldItemValue.ToString()}";
                        SharedMemoryItem newMemoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, fieldItemValueDisplayValue, structItemField.FieldType.Name);
                        AddSharedMemoryItem(newMemoryItem);
                    }

                }

                //FieldInfo[] fields = structItemField.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                //foreach (FieldInfo field in fields)
                //{
                //    DisplayMemoryStructFields(fields, (IGtr2Struct)structItemFieldValue); // Recursively display nested struct fields
                //}
            }
            else if (structItemField.FieldType.IsPrimitive)
            {
                // Primitives: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean
                App.Log.AddDebug($"Field {structItemField.Name} is a primitive: {structItemField}");

                //displayValue = $"[Unexpected: {structItemField.FieldType.Name}]";
                string displayValue = $"Primitive: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);
            }
            else
            {
                App.Log.AddDebug($"Field {structItemField.Name} is an unexpected type: {structItemField}");
                // Primitives: Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, Char, Boolean
                //displayValue = $"[Unexpected: {structItemField.FieldType.Name}]";
                string displayValue = $"Unexpected type: {structItemFieldValue.ToString()}";
                SharedMemoryItem memoryItem = new SharedMemoryItem(parentStructName, structItemField.Name, displayValue, structItemField.FieldType.Name);
                AddSharedMemoryItem(memoryItem);
            }

            //displayValue = structItemFieldObject?.ToString() ?? "null";


        }

        //private void DisplayMemoryStructFieldByteArray(IGtr2Struct structItem, FieldInfo field, byte[] bytes)
        //{
        //    string displayValue = GetByteArrayString(bytes);
        //    string structName = structItem.GetType().Name;
        //    AddSharedMemoryItem(new SharedMemoryItem(structName, field.Name, displayValue, field.FieldType.Name));
        //}
        //private void DisplayMemoryStructFieldArray(IGtr2Struct structItem, FieldInfo field, byte[] bytes)
        //{
        //    string displayValue = GetByteArrayString(bytes);
        //    string structName = structItem.GetType().Name;
        //    AddSharedMemoryItem(new SharedMemoryItem(structName, field.Name, displayValue, field.FieldType.Name));
        //}

        private string GetByteArrayString(byte[] byteArray)
        {
            return Encoding.UTF8.GetString(byteArray).TrimEnd('\0');
        }

        private string GetArrayString(Array array)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (var item in array)
            {
                sb.Append(item?.ToString() ?? "null");
                sb.Append(", ");
            }
            if (array.Length > 0)
                sb.Length -= 2; // Remove last comma and space
            sb.Append("]");
            return sb.ToString();
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

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            //TestGtr2SharedMemory();

            RefreshButton.IsEnabled = false;
            SharedMemoryItems.Clear();
            var progress = new Progress<List<SharedMemoryItem>>(items => {
                AddSharedMemoryItems(items.ToArray());
                //SharedMemoryItems.AddRange(items); // New custom BulkObservableCollection method to add a range of items with a single CollectionChanged event
            }); // runs on UI thread
            await Task.Run(() => LoadItems(progress));
            RefreshButton.IsEnabled = true;
        }

        private void LoadItems(IProgress<List<SharedMemoryItem>> progress)
        {
            int batchLimit = 50; // TODO: Make this a setting
            var batch = new List<SharedMemoryItem>();
            //List<SharedMemoryItem> items = GetGtr2SharedMemoryItems();
            //foreach (var item in items) // your slow data source
            foreach (var item in GetGtr2SharedMemoryItems()) // your slow data source
            {
                batch.Add(item);
                if (batch.Count < batchLimit)
                {
                    continue;
                }
                // do heavy work per item...
                progress.Report(batch); // marshals back to UI thread safely
                batch = new List<SharedMemoryItem>(); // Using batch.Clear() clears batch before progress.Report() finishes updating the UI
            }
            progress.Report(batch);
        }
    }
}
