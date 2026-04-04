using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryItem(string name, Type heldType, int length, Int32 offset, byte[] data, bool stringType, Int32 offsetCheck)
    {
        
        public string Name { get; set; } = name;
        public Type HeldType { get; set; } = heldType; // Can be determined with typeof(Type) eg. typeof(int) for int, typeof(List<string>) for List<string>, etc.
        public int Length { get; set; } = length;
        public nint Address { get; set; } = 0;
        public Int32 Offset { get; set; } = offset;
        public byte[] Data { get; set; } = data ?? new byte[length]; // Raw byte data read from memory, which can be converted to the appropriate type based on HeldType when needed.
        public bool StringType { get; set; } = stringType; // Indicates when a byte type is actually a string
        public Int32 OffsetCheck { get; set; } = offsetCheck; // Used for checking if offsets are correct by comparing to expected values, can be set to 0 when not used
        public MemoryItem(string name, Type heldType, int length, Int32 offset) : this(name, heldType, length, offset, new byte[length], false, 0)
        {
            // Convenience constructor auto-creates byte[] Data
            //var heldTypeSize = Marshal.SizeOf(heldType);
            //var readLength = heldTypeSize * length;
            //Data = new byte[readLength];
            _value = ValueToString() ?? "";
        }
        public MemoryItem(string name, Type heldType, int length, Int32 offset, bool stringType) : this(name, heldType, length, offset, new byte[length], stringType, 0)
        {
            // Convenience constructor to help with byte strings
            _value = ValueToString() ?? "";
        }
        //public MemoryItem(string name, Type heldType, int length, nint offset, bool stringType, Int32 offsetCheck) : this(name, heldType, length, offset, new byte[length], stringType, offsetCheck)
        //{
        //    // Convenience constructor to help with offsetCheck
        //}
        private string _value = "";
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    Save();
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
        public string? ValueAsString
        {
            get
            {
                return ValueToString();
            }
        }
        public int? ValueAsInt
        {
            get
            {
                return ValueToInt();
            }
        }
        public float? ValueAsFloat
        {
            get
            {
                return ValueToSingle();
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
                    //return BitConverter.ToBoolean(Data, 0).ToString();
                    //return "TODO:bool";
                }
                else if (HeldType == typeof(byte))
                {
                    if (!StringType)
                    {
                        return $"Unsupported type: {HeldType.Name} (Non-String)";
                    }
                    App.Log.AddDebug($"Converting char from data: name={Name}, Length={Length}");
                    App.Log.AddDebug($"Data (Data Length={Data.Length}) =>>>{BitConverter.ToString(Data)}<<<");
                    var encoding = Encoding.GetEncoding(Gtr2MemOps.GTR2_ENCODING_CODEPAGE);
                    int nullIndex = Array.IndexOf(Data, (byte)0);
                    int byteLength = nullIndex >= 0 ? nullIndex : Data.Length;
                    var str = encoding.GetString(Data, 0, byteLength);
                    App.Log.AddDebug($"str=>>>{str}<<<");
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
        public int? ValueToInt()
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

        public bool Save()
        {
            bool success = false;
            try
            {
                if (HeldType == typeof(Int32))
                {

                }
                else if (HeldType == typeof(float))
                {
                    nint? gtr2ProcessPointer = Gtr2MemOps.GetGtr2ProcessPointer() ?? throw new Exception("GTR2 process not found");
                    float floatValue = ValueToSingle() ?? 0.0f;
                    Gtr2MemOps.WriteFloat((nint)gtr2ProcessPointer, Address, floatValue);
                    success = true;
                }
                else if (HeldType == typeof(bool))
                {
                }
                else if (HeldType == typeof(byte) && StringType)
                {

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
    }
}
