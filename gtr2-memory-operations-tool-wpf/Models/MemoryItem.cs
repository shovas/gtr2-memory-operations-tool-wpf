using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryItem(string name, Type heldType, uint length, uint offset, byte[] data, bool stringType, Int32 offsetCheck) : INotifyPropertyChanged
    {
        
        public string Name { get; set; } = name;
        public Type HeldType { get; set; } = heldType; // Can be determined with typeof(Type) eg. typeof(int) for int, typeof(List<string>) for List<string>, etc.
        public uint Length { get; set; } = length;
        public uint BaseAddress { get; set; } = 0; // Process.MainModule.BaseAddress
        public uint BaseOffset { get; set; } = 0; // Offset from BaseAddress
        public uint Address { get; set; } = 0; // Address of this memory item
        public uint Offset { get; set; } = offset; // Offset from Address (eg. Gtr2GridVehicleSlot address)
        public byte[] Data { get; set; } = data ?? new byte[length]; // Raw byte data read from memory, which can be converted to the appropriate type based on HeldType when needed.
        public bool StringType { get; set; } = stringType; // Indicates when a byte type is actually a string
        public Int32 OffsetCheck { get; set; } = offsetCheck; // Used for checking if offsets are correct by comparing to expected values, can be set to 0 when not used
        public MemoryItem(string name, Type heldType, uint length, uint offset) : this(name, heldType, length, offset, new byte[length], false, 0)
        {
            // Convenience constructor auto-creates byte[] Data
            //var heldTypeSize = Marshal.SizeOf(heldType);
            //var readLength = heldTypeSize * length;
            //Data = new byte[readLength];
            _value = ValueToString() ?? "";
        }
        public MemoryItem(string name, Type heldType, uint length, uint offset, bool stringType) : this(name, heldType, length, offset, new byte[length], stringType, 0)
        {
            // Convenience constructor to help with byte strings
            _value = ValueToString() ?? "";
        }
        //public MemoryItem(string name, Type heldType, int length, nint offset, bool stringType, Int32 offsetCheck) : this(name, heldType, length, offset, new byte[length], stringType, offsetCheck)
        //{
        //    // Convenience constructor to help with offsetCheck
        //}
        public string HeldTypeAsString
        {
            get
            {
                return HeldType.Name;
            }
        }
        public string BaseOffsetAsHex => $"{BaseOffset:X}";
        public string OffsetWithHex => $"{Offset} ({Offset:X})";
        private string _value = "";
        public string Value
        {
            get
            {
                return _value;
            }
            //set
            //{
            //    if (_value != value)
            //    {
            //        _value = value;
            //        Save();
            //        OnPropertyChanged(nameof(Value));
            //    }
            //}
        }
        public string ValueAsString
        {
            get
            {
                return ValueToString() ?? "";
            }
            set
            {
                if (_value == value) return;
                App.Log.AddDebug("Setting new value");
                if (!Save(value))
                {
                    App.Log.AddDebug("Setting new value: Save failed");
                    return;
                }
                App.Log.AddDebug("Setting new value: Save succeeded");
                _value = value;
                OnPropertyChanged(nameof(ValueAsString));
            }
        }
        public Int32 ValueAsInt32
        {
            get
            {
                return ValueToInt32() ?? 0;
            }
        }
        public float ValueAsFloat
        {
            get
            {
                return ValueToSingle() ?? 0.00f;
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public string? ValueToString()
        {
            try
            {
                if (HeldType == typeof(Int32))
                {
                    String? str = null;
                    try
                    {
                        str = BitConverter.ToInt32(Data, 0).ToString();
                    }
                    catch
                    {
                        return "[Convert Int32 Error]";
                    }
                    return str;
                }
                else if (HeldType == typeof(float))
                {
                    String? str = null;
                    try
                    {
                        str = BitConverter.ToSingle(Data, 0).ToString();
                    }
                    catch
                    {
                        return "[Convert float/Single Error]";
                    }
                    return str;
                }
                else if (HeldType == typeof(bool))
                {
                    String? str = null;
                    try
                    {
                        str = BitConverter.ToBoolean(Data, 0).ToString();
                    }
                    catch
                    {
                        return "[Convert bool/Boolean Error]";
                    }
                    return str;
                }
                else if (HeldType == typeof(byte))
                {
                    if (!StringType)
                    {
                        return $"Unsupported type: {HeldType.Name} (Non-String)";
                    }
                    //App.Log.AddDebug($"Converting char from data: name={Name}, Length={Length}");
                    //App.Log.AddDebug($"Data (Data Length={Data.Length}) =>>>{BitConverter.ToString(Data)}<<<");
                    var encoding = Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE);
                    int nullIndex = Array.IndexOf(Data, (byte)0);
                    int byteLength = nullIndex >= 0 ? nullIndex : Data.Length;
                    var str = encoding.GetString(Data, 0, byteLength);
                    //App.Log.AddDebug($"str=>>>{str}<<<");
                    return str;
                }
                else
                {
                    return $"Unsupported type: {HeldType.Name}";
                }
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Error converting to string: {ex.Message}");
                return null;
            }
        }
        public Int32? ValueToInt32()
        {
            try
            {

                if (HeldType == typeof(int))
                {
                    return BitConverter.ToInt32(Data, 0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot convert to int: HeldType is {HeldType.Name}");
                }
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Error converting to int: {ex.Message}");
                return null;

            }
        }

        public Single? ValueToSingle()
        {
            try
            {

                if (HeldType == typeof(float))
                {
                    return BitConverter.ToSingle(Data, 0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot convert to float: HeldType is {HeldType.Name}");
                }
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Error converting to float: {ex.Message}");
                return null;
            }
        }

        public bool? ValueToBool()
        {
            try
            {

                if (HeldType == typeof(bool))
                {
                    return BitConverter.ToBoolean(Data, 0);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot convert to bool: HeldType is {HeldType.Name}");
                }
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Error converting to bool: {ex.Message}");
                return null;
            }
        }

        public bool Save( string newValue )
        {
            bool success = false;
            try
            {
                nint? gtr2ProcessPointer = Gtr2ProgMemOps.GetGtr2ProcessPointer() ?? throw new Exception("GTR2 process not found");
                if (HeldType == typeof(Int32))
                {
                    App.Log.AddDebug($"Write new Int32 to memory: {newValue}");
                    Int32 int32Value = int.Parse(newValue);
                    if (!Gtr2ProgMemOps.WriteInt32((nint)gtr2ProcessPointer, Address, int32Value))
                    {
                        throw new Exception("Failed to write Int32 to memory");
                    }
                    if (!Read())
                    {
                        throw new Exception("Failed to read back Int32 from memory after writing");
                    }
                    Int32 newReadInt32 = ValueToInt32() ?? 0;
                    if (int32Value != newReadInt32)
                    {
                        throw new Exception($"Int32 verification failed after writing. Expected: {int32Value}, Read Back: {newReadInt32}");
                    }
                    success = true;
                }
                else if (HeldType == typeof(float))
                {
                    App.Log.AddDebug($"Write new float to memory: {newValue}");
                    float floatValue = float.Parse(newValue);
                    if (!Gtr2ProgMemOps.WriteFloat((nint)gtr2ProcessPointer, Address, floatValue))
                    {
                        throw new Exception("Failed to write float to memory");
                    }
                    if (!Read())
                    {
                        throw new Exception("Failed to read back float from memory after writing");
                    }
                    float newReadFloat = ValueToSingle() ?? 0.0f;
                    if (floatValue != newReadFloat)
                    {
                        throw new Exception($"Float verification failed after writing. Expected: {floatValue}, Read Back: {newReadFloat}");
                    }
                    success = true;
                }
                else if (HeldType == typeof(bool))
                {
                    App.Log.AddDebug($"Write new bool to memory: {newValue}");
                    bool boolValue = newValue.Equals("true", StringComparison.CurrentCultureIgnoreCase) || newValue == "1";
                    if (!Gtr2ProgMemOps.WriteBool((nint)gtr2ProcessPointer, Address, boolValue))
                    {
                        throw new Exception("Failed to write bool to memory");
                    }
                    if (!Read())
                    {
                        throw new Exception("Failed to read back bool from memory after writing");
                    }
                    bool newReadBool = ValueToBool() ?? false;
                    if (boolValue != newReadBool)
                    {
                        throw new Exception($"Bool verification failed after writing. Expected: {boolValue}, Read Back: {newReadBool}");
                    }
                    success = true;
                }
                else if (HeldType == typeof(byte) && StringType)
                {
                    App.Log.AddDebug($"Writing new string to memory: {newValue}");
                    string stringValue = newValue;
                    Encoding encoding = Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE);
                    if(!Gtr2ProgMemOps.WriteString((nint)gtr2ProcessPointer, Address, stringValue, encoding, Length))
                    {
                        throw new Exception("Failed to write string to memory");
                    }
                    byte[] oldData = Data;
                    if (!Read())
                    {
                        throw new Exception("Failed to read back string from memory after writing");
                    }
                    string newReadString = ValueToString() ?? "";
                    if ( stringValue != newReadString)
                    {
                        throw new Exception($"String verification failed after writing. Expected: {stringValue}, Read Back: {newReadString}");
                    }
                    success = true;
                }
                else
                {
                    throw new Exception($"Unsupported HeldType for saving: {HeldType.Name}");
                }
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Exception saving MemoryItem: {ex.Message}");
            }
            return success;
        }

        private bool Read()
        {
            uint heldTypeSize = (uint)Marshal.SizeOf(HeldType);
            uint byteLength = heldTypeSize * Length;
            byte[] buf = Gtr2ProgMemOps.ReadGtr2MemoryByteArray(Address, byteLength);
            if (buf.Length is 0) return false;
            Data = buf;
            return true;
        }
    }
}
