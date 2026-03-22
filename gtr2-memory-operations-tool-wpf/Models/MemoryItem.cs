using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryItem(int offset, int length, string name, Type heldType, byte[] data)
    {
        public int Offset { get; set; } = offset;
        public int Length { get; set; } = length;
        public string Name { get; set; } = name;
        public Type HeldType { get; set; } = heldType; // Can be determined with typeof(Type) eg. typeof(int) for int, typeof(List<string>) for List<string>, etc.
        public byte[] Data { get; set; } = data; // Raw byte data read from memory, which can be converted to the appropriate type based on HeldType when needed.
        public string? ValueAsString
        {
            get
            {
                try
                {
                    if (HeldType == typeof(int))
                    {
                        return BitConverter.ToInt32(Data, 0).ToString();
                    }
                    else if (HeldType == typeof(float))
                    {
                        return BitConverter.ToSingle(Data, 0).ToString();
                    }
                    else if (HeldType == typeof(double))
                    {
                        return BitConverter.ToDouble(Data, 0).ToString();
                    }
                    else if (HeldType == typeof(bool))
                    {
                        return BitConverter.ToBoolean(Data, 0).ToString();
                    }
                    else if (HeldType == typeof(string))
                    {
                        return Encoding.UTF8.GetString(Data).TrimEnd('\0'); // Assuming null-terminated strings
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
        }
        public int? ValueAsInt
        {
            get
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
        }
        public float? ValueAsFloat
        {
            get
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
        }
    }
}
