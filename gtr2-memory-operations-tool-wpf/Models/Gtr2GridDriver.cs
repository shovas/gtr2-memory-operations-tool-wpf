using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Gtr2MemOpsTool.Models
{
    public class Gtr2GridDriver
    {
        public uint Address { get; set; }
        public uint GridAddress { get; set; }
        public uint GridOffset { get; set; } // Offset from Grid Address. Grid Address + Grid Offset = Vehicle Slot Address
        public List<MemoryItem> MemoryItems { get; set; } = [
            new MemoryItem("Timing_Laptime_A", typeof(float), 1, 0x4B68), // 19304 (4B68)
            new MemoryItem("NameFull_One", typeof(byte), 64, 0x5448, true), // 21576 (5448)
        ];
        public Gtr2GridDriver(uint address, uint gridAddress, uint gridOffset)
        {
            Address = address;
            GridAddress = gridAddress;
            GridOffset = gridOffset;
            CalculateMemoryLocations();
        }

        /// <summary>
        /// Calculates item memory address from GridAddress + Offset
        /// </summary>
        public void CalculateMemoryLocations()
        {
            uint offset = 0;
            foreach (var item in MemoryItems)
            {
                item.Address = Address + offset;
                var typeSize = Marshal.SizeOf(item.HeldType);
                offset += (uint)(typeSize * item.Length);
            }
        }

        public MemoryItem? GetMemoryItemByName(string name)
        {
            return MemoryItems.FirstOrDefault(item => item.Name == name);
        }

        public string GetFirstDriverName()
        {
            var nameItem = GetMemoryItemByName("NameFull_One");
            if (nameItem != null)
            {
                return nameItem.ValueAsString ?? string.Empty;
            }
            return string.Empty;
        }


    }
}