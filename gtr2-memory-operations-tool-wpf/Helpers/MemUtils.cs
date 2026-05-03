using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Helpers
{
    internal class MemUtils
    {
        public static string FormatHexAddress(int address)
        {
            return $"0x{address:X8}";
        }

        public static string GetStringFromBytes(byte[] bytes, Encoding encoding)
        {
            if (bytes == null)
                return "";
            
            /*
            var encoding = Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE);
                int nullIndex = Array.IndexOf(fieldValueBytes, (byte)0);
                int byteLength = nullIndex >= 0 ? nullIndex : fieldValueBytes.Length;
                string fieldValue = encoding.GetString(fieldValueBytes, 0, byteLength);
            */

            var nullIdx = Array.IndexOf(bytes, (byte)0);

            return nullIdx >= 0
              ? encoding.GetString(bytes, 0, nullIdx)
              : encoding.GetString(bytes);
        }
    }
}
