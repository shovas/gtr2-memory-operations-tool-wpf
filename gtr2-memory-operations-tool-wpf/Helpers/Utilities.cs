using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Helpers
{
    internal class Utilities
    {
        public static string FormatHexAddress(int address)
        {
            return $"0x{address:X8}";
        }

        public static string GetStringFromBytes(byte[] bytes)
        {
            if (bytes == null)
                return "";

            var nullIdx = Array.IndexOf(bytes, (byte)0);

            return nullIdx >= 0
              ? Encoding.Default.GetString(bytes, 0, nullIdx)
              : Encoding.Default.GetString(bytes);
        }
    }
}
