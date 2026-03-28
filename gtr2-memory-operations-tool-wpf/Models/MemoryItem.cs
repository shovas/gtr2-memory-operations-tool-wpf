using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class MemoryItem(string name, Type heldType, int length, nint offset, byte[] data)
    {
        public string Name { get; set; } = name;
        public Type HeldType { get; set; } = heldType; // Can be determined with typeof(Type) eg. typeof(int) for int, typeof(List<string>) for List<string>, etc.
        public int Length { get; set; } = length;
        public nint Offset { get; set; } = offset;
        public byte[] Data { get; set; } = data ?? new byte[length]; // Raw byte data read from memory, which can be converted to the appropriate type based on HeldType when needed.
        public MemoryItem(string name, Type heldType, int length, nint offset) : this(name, heldType, length, offset, new byte[length])
        {
            // Convenience constructor auto-creates byte[] Data
            //var heldTypeSize = Marshal.SizeOf(heldType);
            //var readLength = heldTypeSize * length;
            //Data = new byte[readLength];
        }
        public string? ValueAsString
        {
            get
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
                        
                        //return "TODO:int";
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
                        //return BitConverter.ToSingle(Data, 0).ToString();
                        //return "TODO:float";
                    }
                    // No doubles used yet
                    //else if (HeldType == typeof(double))
                    //{
                    //    return BitConverter.ToDouble(Data, 0).ToString();
                    //}
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
                    else if (HeldType == typeof(char))
                    {
                        //String? str = null;
                        //try
                        //{
                        //    str = BitConverter.ToString(Data, 0);
                        //}
                        //catch
                        //{
                        //    return "[Convert char/String Error]";
                        //}
                        //return str;
                        App.Log.AddDebug($"Converting char from data: name={Name}, Length={Length}");
                        App.Log.AddDebug($"Data (Data Length={Data.Length}) =>>>{BitConverter.ToString(Data)}<<<");
                        //var str2 = Encoding.UTF8.GetString(Data).TrimEnd('\0');
                        //App.Log.AddDebug($"str2=>>>{str2}<<<");
                        var encoding = Encoding.GetEncoding(Gtr2MemOps.GTR2_ENCODING_CODEPAGE);
                        
                        int nullIndex = Array.IndexOf(Data, (byte)0);
                        int byteLength = nullIndex >= 0 ? nullIndex : Data.Length;
                        var str = encoding.GetString(Data, 0, byteLength);

                        //var str = encoding.GetString(Data, 0, Data.Length);
                        App.Log.AddDebug($"str=>>>{str}<<<");
                        return str;
                    }
                    // No strings used yet
                    //else if (HeldType == typeof(string))
                    //{
                    //    var encoding = Encoding.GetEncoding(Gtr2MemOps.GTR2_ENCODING_CODEPAGE);
                    //    return encoding.GetString(Data, 0, Length);
                    //    //return Encoding.UTF8.GetString(Data).TrimEnd('\0'); // Assuming null-terminated strings
                    //}
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
