using Gtr2MemOpsTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public partial class Gtr2MemOps
    {
        public const string GTR2_PROCESS_NAME = "gtr2";

        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_OPERATION = 0x0008;

        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_READWRITE = 0x04;

        // Sizes
        private const int GTR2_MEMORY_SLOT_SIZE = 22624; // Byte length of each slot in the slot list

        // Memory signatures
        // - The base memory region is found by matching these byte signatures within it (they must all match)
        private const string GTR2_MEMORY_SIG_AIW = "6C010000FEFFFFFF312E312E3300"; // This exact byte pattern is located in the AIW data area of the base memory region
        private const string GTR2_MEMORY_SIG_PLR = "00401C47 CDCCCC3D"; // This exact byte pattern is located in the PLR data area of the base memory region
        private const string GTR2_MEMORY_SIG_GDB_HORIZON = "4C454E535F464C4152455F5241494E424F572E444453"; // Encoding.ASCII.GetBytes("LENS_FLARE_RAINBOW.DDS"); This exact byte pattern is located in the GDB data area of the base memory region.
        private const string GTR2_MEMORY_SIG_GDB_LIGHTING = "4459544558"; // Encoding.ASCII.GetBytes("DYTEX"); This exact byte pattern is located in the GDB data area of the base memory region.
        // ^ Warning: This almost certainly does not exist in the same base memory region as reported by VirtualQueryEx

        // Offsets 
        private const Int32 GTR2_MEMORY_GRID_HEADER_OFFSET = 30368; // Offset from dynamically located Grid memory region base address
        private const Int32 GTR2_MEMORY_SLOT_OFFSET_SLOT_ID = 4; // Offset from slot address
        private const Int32 GTR2_MEMORY_SLOT_OFFSET_PITGROUPID = 8; // Offset from slot address
        private const Int32 GTR2_MEMORY_SLOT_OFFSET_DRIVER_NAME = 21576; // Offset from slot address
        private const Int32 GTR2_MEMORY_SLOT_OFFSET_WEIGHT_PENALTY = 16084; // Offset from slot address
        private const Int32 GTR2_MEMORY_SLOT_OFFSET_CAR_FILEPATH = 22092; // Offset from slot address

        // Offsets from AIW address - based on original python code
        // - The base memory region is located by 
        private const Int32 GTR2_MEMORY_BASE_OFFSET_AIW = 11876; // AIW data found at offset from dynamically located base memory region address
        private const Int32 GTR2_MEMORY_AIW_OFFSET_GRID = 30368 - GTR2_MEMORY_BASE_OFFSET_AIW; // =18492 Grid data found at offset from base
        private const Int32 GTR2_MEMORY_AIW_OFFSET_GDB = 2440065 - GTR2_MEMORY_BASE_OFFSET_AIW; // =2428189 GDB data found at offset from base
        private const Int32 GTR2_MEMORY_AIW_OFFSET_PLR = 2463284 - GTR2_MEMORY_BASE_OFFSET_AIW; // =2451408 Offset from AIW address

        // Lengths
        private const int GTR2_MEMORY_DRIVER_NAME_LENGTH = 64; // Byte length of DriverName string in memory
        private const int GTR2_MEMORY_CAR_FILEPATH_LENGTH = 128; // Byte length of CarFilePath string in memory

        // GTR2 memory strings are encoded in Windows-1252 encoding (a common single-byte encoding that can represent standard ASCII characters and some additional characters used in Western European languages). This is important to know when reading/writing string data to ensure correct encoding/decoding.
        public const int GTR2_ENCODING_CODEPAGE = 1252; // GTR2 uses Windows-1252 encoding for strings

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public nint BaseAddress;
            public nint AllocationBase;
            public uint AllocationProtect;
            public nint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial nint OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress,
            [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress,
            [Out] byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial int VirtualQueryEx(nint hProcess, nint lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(nint hObject);

        public Gtr2MemOps()
        {
            // Register Code Pages provider so that Windows-1252 is available to use
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //TestGtr2ProcessMemory();

        }

        private static void TestGtr2ProcessMemory () {
            try
            {
                // Test GTR2 process memory reading and writing as a sanity check
                bool success = TestGtr2Process();
                if (!success)
                {
                    throw new Exception("GTR2 process memory read/write test failed.");
                }

                // Read in GTR2 grid data
                success = ReadGtr2TestGrid();
                if (!success)
                {
                    throw new Exception("Reading GTR2 grid data failed.");
                }

            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
            }
        }

        public static IEnumerable<MemoryItem> GetGtr2ProgramMemoryItems()
        {
            App.Log.AddInfo("Getting GTR2 Program Memory Items");

            Gtr2Grid? gtr2Grid = ReadGtr2Grid();
            if (gtr2Grid is null)
            {
                yield break;
            }
            foreach (var vehicleSlot in gtr2Grid.VehicleSlots)
            {
                foreach (var memoryItem in  vehicleSlot.MemoryItems)
                {
                    App.Log.AddDebug($"Yielding memory item: {memoryItem.Offset}, {memoryItem.Name}, {memoryItem.HeldType}, {memoryItem.Length}");
                    yield return memoryItem;
                }
            }

            App.Log.AddInfo("Finished Getting GTR2 Program Memory Items");
        }

        private static Gtr2Grid? ReadGtr2Grid()
        {
            // Overview:
            // 1. Find GTR2.EXE process
            // 2. Open GTR2.exe process for Read/Write
            // 3. Scan memory for the slot list header using multi-signature validation
            // 4. Walk the Grid linked list of Gtr2MemVehSlots and load in list of GridData

            Gtr2Grid? gridData = null;
            nint? gtr2ProcessPointer = null;
            try
            {

                // 1. Find gtr2.exe
                Process? gtr2Process = GetProcessByName(GTR2_PROCESS_NAME) ?? throw new Exception("Failed finding GTR2 process.");
                App.Log.AddDebug($"Found gtr2.exe (PID {gtr2Process.Id})");

                // 2. Open process
                gtr2ProcessPointer = OpenProcessForReadWrite(gtr2Process);
                if (gtr2ProcessPointer == null || gtr2ProcessPointer == nint.Zero)
                {
                    throw new Exception("Failed opening GTR2 process.");
                }
                //gtr2ProcessPointer = gtr2TempProcessPointer.Value;
                App.Log.AddDebug("Opened process");

                // 3. Scan memory for the slot list header
                nint gridAddr = FindGridAddress((nint)gtr2ProcessPointer);
                if (gridAddr == nint.Zero)
                {
                    throw new Exception("Failed to locate slot list header.");
                }
                App.Log.AddDebug($"Found slot list header at 0x{gridAddr:X}");

                // 4. Walk the linked list and locate the first WeightPenalty
                gridData = ReadGtr2GridData((nint)gtr2ProcessPointer, gridAddr);
                if (gridData.VehicleSlots.Count == 0)
                {
                    throw new Exception("Failed finding grid data in slot list.");
                }
                App.Log.AddDebug($"Found GridData for {gridData.VehicleSlots.Count} vehicles");

            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
            }
            finally
            {
                // Close process
                if (gtr2ProcessPointer != null && gtr2ProcessPointer != nint.Zero)
                {
                    CloseHandle((nint)gtr2ProcessPointer);
                }
            }
            return gridData;
        }

        // GridData is the name for the memory data structure containing slots for each vehicle in the grid. Each slot contains various data fields for the vehicle including DriverName, WeightPenalty, etc. This function walks the linked list of slots starting from the header and populates a GridData object with the data read from each slot.
        // - Note: All sessions will always have at least 20 slots, even if driver count is lower, and if driver count is >= 20 then slot count will match driver count.
        private static Gtr2Grid ReadGtr2GridData(nint hProc, nint gridAddr)
        {

            // Follow the linked list of slots and populate gridData.Slots with the data you want to read from each slot (e.g. driver name, weight penalty, etc.)
            Gtr2Grid gridData = new(gridAddr);

            const nint slotStep = GTR2_MEMORY_SLOT_SIZE;
            nint curSlotAddr = gridAddr;

            try
            {
                // TODO: Instead of a loop, could I just straight up read all the memory right through and put it into a struct? 
                while (true)
                {
                    // Validate grid slot
                    if (!ValidateGridSlot(hProc, curSlotAddr))
                        throw new Exception("Invalid slot header detected");

                    // Check final slot: pitGroupId will be -1
                    // - Read pitgroup_id (3rd int32, offset +8) to detect last slot
                    nint pitGroupIdAddr = curSlotAddr + GTR2_MEMORY_SLOT_OFFSET_PITGROUPID; // Offset of pitGroupId within each slot (int32 at offset 8 from slot base)
                    int? pitGroupId = ReadMemoryInt32(hProc, pitGroupIdAddr)!;
                    if (pitGroupId == null)
                    {
                        throw new Exception("Failed reading pitGroupId or reached end of slot list.");
                    }
                    else if (pitGroupId == -1)
                    {
                        // End of grid slots
                        App.Log.AddDebug("Reached end of slot list.");
                        break;
                    }
                    App.Log.AddDebug($"pitGroupId={pitGroupId}");

                    // Read data from the slot and add to gridData.Slots
                    Int32 slotOffset = (Int32)(curSlotAddr - gridAddr);
                    Gtr2GridVehicleSlot vehicleSlot = new(slotOffset);

                    // We can read each Gtr2GridVehicleSlot field individually by reading the specific memory address for that field based on the slot base address + the known offset for that field, or we could read the entire slot's worth of memory into a byte array and then parse out each field from that byte array based on the known offsets within the slot. The latter would be more efficient as it would involve fewer calls to ReadProcessMemory, but it would also be more complex to implement. For simplicity and clarity, I'll read each field individually for now.
                    foreach( var memoryItem in vehicleSlot.MemoryItems)
                    {
                        var heldTypeSize = Marshal.SizeOf(memoryItem.HeldType);
                        var readLength = heldTypeSize * memoryItem.Length;
                        var memoryItemAddress = curSlotAddr + memoryItem.Offset;
                        memoryItem.Data = ReadMemoryByteArray(hProc, memoryItemAddress, readLength);

                        // TODO: We have the memory data, the MemoryItem class can return the right type based on HeldType. I can return vehicleSlot poulated with these memoryItems and the caller then has the values to work with.

                    }

                    gridData.VehicleSlots.Add(vehicleSlot);

                    // Check for end of grid
                    nint nextSlotAddr = curSlotAddr + slotStep;
                    if (ValidateEndOfGrid(hProc, nextSlotAddr))
                    {
                        App.Log.AddDebug("Next slot is end of list marker. Ending read.");
                        break;
                    }

                    // Increment slot address
                    curSlotAddr += slotStep;
                }
            }
            catch (Exception ex)
            {
                App.Log.AddError($"Exception while reading grid data: {ex.Message}");
                //App.Log.AddException(ex);
            }

            // Record number of vehicles found based on how many valid slots we can read before hitting the end of the linked list (indicated by a header that fails validation)
            App.Log.AddDebug($"Total valid slots read: {gridData.VehicleSlots.Count}");

            return gridData;
        }

        private static bool ReadGtr2TestGrid()
        {
            // Overview:
            // 1. Find GTR2.EXE process
            // 2. Open GTR2.exe process for Read/Write
            // 3. Scan memory for the slot list header using multi-signature validation
            // 4. Walk the linked list of slots to find the first WeightPenalty field
            // 5. Read current value to verify we can read and for later comparison after new write
            // 6. Write new WeightPenalty value
            // 7. Read the new value back to verify write succeeded

            bool success = false;
            nint? gtr2ProcessPointer = null;
            try
            {

                // ---------------------------------------------------------
                // 1. Find gtr2.exe
                // ---------------------------------------------------------
                Process? gtr2Process = GetProcessByName(GTR2_PROCESS_NAME) ?? throw new Exception("Failed finding GTR2 process.");
                App.Log.AddDebug($"Found gtr2.exe (PID {gtr2Process.Id})");

                // ---------------------------------------------------------
                // 2. Open process
                // ---------------------------------------------------------
                gtr2ProcessPointer = OpenProcessForReadWrite(gtr2Process);
                if (gtr2ProcessPointer == null || gtr2ProcessPointer == nint.Zero)
                {
                    throw new Exception("Failed opening GTR2 process.");
                }
                //gtr2ProcessPointer = gtr2TempProcessPointer.Value;
                App.Log.AddDebug("Opened process");

                // ---------------------------------------------------------
                // 3. Scan memory for the slot list header
                // ---------------------------------------------------------
                nint gridAddr = FindGridAddress((nint)gtr2ProcessPointer);
                if (gridAddr == nint.Zero)
                {
                    throw new Exception("Failed to locate slot list header.");
                }
                App.Log.AddDebug($"Found slot list header at 0x{gridAddr:X}");

                // ---------------------------------------------------------
                // 4. Walk the linked list and locate the first WeightPenalty
                // ---------------------------------------------------------
                Gtr2TestGrid gridData = ReadTestGridData((nint)gtr2ProcessPointer, gridAddr);
                if (gridData.NumVeh == 0)
                {
                    throw new Exception("Failed finding grid data in slot list.");
                }
                App.Log.AddDebug($"Found GridData for {gridData.NumVeh} vehicles");

                success = true;
            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
            }
            finally
            {
                // Close process
                if (gtr2ProcessPointer != null && gtr2ProcessPointer != nint.Zero)
                {
                    CloseHandle((nint)gtr2ProcessPointer);
                }
            }
            return success;
        }

        private static Gtr2TestGrid ReadTestGridData(nint hProc, nint gridAddr)
        {

            // Follow the linked list of slots and populate gridData.Slots with the data you want to read from each slot (e.g. driver name, weight penalty, etc.)
            Gtr2TestGrid gridData = new();

            const Int32 slotStep = GTR2_MEMORY_SLOT_SIZE;
            nint curSlotAddr = gridAddr;

            try
            {
                while (true)
                {
                    // Validate grid slot
                    if (!ValidateGridSlot(hProc, curSlotAddr))
                        throw new Exception("Invalid slot header detected");

                    // Check final slot: pitGroupId will be -1
                    // - Read pitgroup_id (3rd int32, offset +8) to detect last slot
                    nint pitGroupIdAddr = curSlotAddr + GTR2_MEMORY_SLOT_OFFSET_PITGROUPID; // Offset of pitGroupId within each slot (int32 at offset 8 from slot base)
                    int? pitGroupId = ReadMemoryInt32(hProc, pitGroupIdAddr)!;
                    if (pitGroupId == null)
                    {
                        throw new Exception("Failed reading pitGroupId or reached end of slot list.");
                    }
                    else if (pitGroupId == -1)
                    {
                        // End of grid slots
                        App.Log.AddDebug("Reached end of slot list.");
                        break;
                    }
                    App.Log.AddDebug($"pitGroupId={pitGroupId}");

                    // Read data from the slot and add to gridData.Slots
                    Gtr2TestGridVehicleSlot slotData = new();
                    try
                    {
                        slotData.SlotId = FindSlotId(hProc, curSlotAddr);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    try
                    {
                        slotData.DriverName = FindSlotDriverName(hProc, curSlotAddr);
                        if (slotData.DriverName.Length == 0)
                        {
                            slotData.DriverName = "[MEMORY READ BLANK]";
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    try
                    {
                        slotData.WeightPenalty = FindSlotWeightPenalty(hProc, curSlotAddr);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    try
                    {
                        slotData.CarFilePath = FindSlotCarFilePath(hProc, curSlotAddr);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                    gridData.Slots.Add(slotData);

                    // Check for end of grid
                    nint nextSlotAddr = curSlotAddr + slotStep;
                    if (ValidateEndOfGrid(hProc, nextSlotAddr))
                    {
                        App.Log.AddDebug("Next slot is end of list marker. Ending read.");
                        break;
                    }
                    
                    // Increment slot address
                    curSlotAddr += slotStep;
                }
            }
            catch (Exception ex)
            {
                App.Log.AddError($"Exception while reading grid data: {ex.Message}");
                //App.Log.AddException(ex);
            }

            // Record number of vehicles found based on how many valid slots we can read before hitting the end of the linked list (indicated by a header that fails validation)
            gridData.NumVeh = gridData.Slots.Count;
            App.Log.AddDebug($"Total valid slots read: {gridData.NumVeh}");

            return gridData;
        }

        public static bool TestGtr2Process()
        {
            // Overview:
            // 1. Find GTR2.EXE process
            // 2. Open GTR2.exe process for Read/Write
            // 3. Scan memory for the slot list header using multi-signature validation
            // 4. Walk the linked list of slots to find the first WeightPenalty field
            // 5. Read current value to verify we can read and for later comparison after new write
            // 6. Write new WeightPenalty value
            // 7. Read the new value back to verify write succeeded

            bool success = false;
            nint? gtr2ProcessPointer = null;
            try
            {

                // ---------------------------------------------------------
                // 1. Find gtr2.exe
                // ---------------------------------------------------------
                Process? gtr2Process = GetProcessByName(GTR2_PROCESS_NAME) ?? throw new Exception("Failed finding GTR2 process.");
                App.Log.AddDebug($"Found gtr2.exe (PID {gtr2Process.Id})");

                // ---------------------------------------------------------
                // 2. Open process
                // ---------------------------------------------------------
                gtr2ProcessPointer = OpenProcessForReadWrite(gtr2Process);
                if (gtr2ProcessPointer == null || gtr2ProcessPointer == nint.Zero)
                {
                    throw new Exception("Failed opening GTR2 process.");
                }
                //gtr2ProcessPointer = gtr2TempProcessPointer.Value;
                App.Log.AddDebug("Opened process");

                // ---------------------------------------------------------
                // 3. Scan memory for the slot list header
                // ---------------------------------------------------------
                nint gridAddr = FindGridAddress((nint)gtr2ProcessPointer);
                if (gridAddr == nint.Zero)
                {
                    throw new Exception("Failed to locate slot list header.");
                }
                App.Log.AddDebug($"Found slot list header at 0x{gridAddr:X}");

                // ---------------------------------------------------------
                // 4. Walk the linked list and locate the first WeightPenalty
                // ---------------------------------------------------------
                nint weightPenaltyAddr = FollowGridAndGetWeightPenaltyAddr((nint)gtr2ProcessPointer, gridAddr);
                if (weightPenaltyAddr == nint.Zero)
                {
                    throw new Exception("Failed finding weight penalty in slot list.");
                }
                App.Log.AddDebug($"Found WeightPenalty address for first slot: 0x{weightPenaltyAddr:X}");

                // ---------------------------------------------------------
                // 5. Read current value to verify we can read and for later comparison after new write
                // ---------------------------------------------------------
                float? tempWeightPenaltyFloatData = ReadMemoryFloat((nint)gtr2ProcessPointer, weightPenaltyAddr) ?? throw new Exception("Failed reading current WeightPenalty value.");
                float weightPenaltyFloatData = tempWeightPenaltyFloatData.Value;
                App.Log.AddDebug($"Current WeightPenalty read: {weightPenaltyFloatData}");

                // ---------------------------------------------------------
                // 6. Write new value
                // ---------------------------------------------------------
                float newWeightPenaltyFloat = 123.45f;  //0.15f; // Change this to whatever you need
                bool result = WriteFloat((nint)gtr2ProcessPointer, weightPenaltyAddr, newWeightPenaltyFloat);
                if (!result)
                {
                    throw new Exception("Failed writing WeightPenalty value.");
                }
                App.Log.AddDebug($"New WeightPenalty written: {newWeightPenaltyFloat}");

                // ---------------------------------------------------------
                // 7. Read new value back to confirm the write worked
                // ---------------------------------------------------------
                float? newWeightPenaltyFloatData = ReadMemoryFloat((nint)gtr2ProcessPointer, weightPenaltyAddr) ?? throw new Exception("Failed reading new WeightPenalty value.");
                float newWeightPenaltyFloatValue = newWeightPenaltyFloatData.Value;
                App.Log.AddDebug($"New WeightPenalty read: {newWeightPenaltyFloatValue}");

                // Mark success
                success = true;
            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
            }
            finally
            {
                // Close process
                if (gtr2ProcessPointer != null && gtr2ProcessPointer != nint.Zero)
                {
                    CloseHandle((nint)gtr2ProcessPointer);
                }

            }

            return success;
        }

        public static bool TestGtr2GetProcess ()
        {
            try
            {
                Process? gtr2Process = GetProcessByName(GTR2_PROCESS_NAME) ?? throw new Exception("Failed finding GTR2 process.");
                App.Log.AddDebug($"Found gtr2.exe (PID {gtr2Process.Id})");
            } catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
                return false;
            }

            return true;
        }

        public static bool TestGtr2OpenProcess ()
        {
            try
            {
                // 1. Get Process
                Process? gtr2Process = GetProcessByName(GTR2_PROCESS_NAME) ?? throw new Exception("Failed finding GTR2 process.");
                App.Log.AddDebug($"Found gtr2.exe (PID {gtr2Process.Id})");

                // 2. Open process
                nint? gtr2ProcessPointer = OpenProcessForReadWrite(gtr2Process);
                if (gtr2ProcessPointer == null || gtr2ProcessPointer == nint.Zero)
                {
                    throw new Exception("Failed opening GTR2 process.");
                }
                //gtr2ProcessPointer = gtr2TempProcessPointer.Value;
                App.Log.AddDebug($"Opened process successfully");
            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
                return false;
            }

            return true;
        }

        public static bool IsGtr2ProcessRunning()
        {
            Process? gtr2Process;
            try
            {
                gtr2Process = GetProcessByName(GTR2_PROCESS_NAME);
            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
                return false;
            }
            if ( gtr2Process is null )
            {
                App.Log.AddDebug($"Failed finding GTR2 process");
                return false;
            }
            App.Log.AddDebug($"Found gtr2.exe (PID {gtr2Process!.Id})");
            return true;
        }

        public static nint? GetGtr2ProcessPointer()
        {
            nint? gtr2ProcessPointer;
            try
            {
                // Get Process
                Process? gtr2Process = GetProcessByName(GTR2_PROCESS_NAME) ?? throw new Exception("Failed finding GTR2 process.");

                // Open process
                gtr2ProcessPointer = OpenProcessForReadWrite(gtr2Process);
                if (gtr2ProcessPointer == null || gtr2ProcessPointer == nint.Zero)
                {
                    throw new Exception("Failed opening GTR2 process.");
                }
            }
            catch (Exception ex)
            {
                App.Log.AddDebug(ex.ToString());
                return nint.Zero;
            }
            return gtr2ProcessPointer!;
        }

        /// <summary>
        /// Finds the GTR2 process by name.
        /// </summary>
        /// <returns>Process if found otherwise null</returns>
        private static Process? GetProcessByName(string processName)
        {
            // Find process by name
            Process[] gtr2Processes = Process.GetProcessesByName(processName);
            if (gtr2Processes.Length == 0)
            {
                App.Log.AddDebug($"Process \"{processName}\" is not running.");
                return null;
            }
            else if (gtr2Processes.Length > 1)
            {
                App.Log.AddDebug($"Multiple \"{processName}\" processes found. Aborting.");
                return null;
            }
            // Exactly one process found
            Process gtr2Process = gtr2Processes[0];
            if (gtr2Process == null) // This shouldn't even be necessary but whatever
            {
                App.Log.AddDebug("Unexpected condition: Process \"{gtr2ProcessName}\" found but invalid (null).");
                return null;
            }
            return gtr2Process;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process">Process to open</param>
        /// <returns>nint process pointer on success otherwise null</returns>
        private static nint OpenProcessForReadWrite(Process process)
        {
            nint processPointer = OpenProcess(
                PROCESS_QUERY_INFORMATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION,
                false,
                process.Id);
            if (processPointer == 0)
            {
                App.Log.AddError($"Failed opening process: {process.Id}");
                return nint.Zero;
            }
            App.Log.AddDebug($"Opened process {process.Id} for READ/WRITE");
            return processPointer;
        }

        // GTR2 must be loaded into a driving session: Practice, Qualifying, Warmup, Race
        public static nint FindGridAddress(nint hProcess)
        {
            App.Log.AddInfo("Starting memory scan for Grid address...");

            /* --------------------------------------------------------------
             *  SIGNATURES (exact bytes from the Python code)
             * -------------------------------------------------------------- */
            // 1. AIW config – offset 11876 in Python
            byte[] sigAiw = Convert.FromHexString(GTR2_MEMORY_SIG_AIW);
            /*byte[] sigAiw = [ // Python: self.aiwcfg = (11876, "6C010000FEFFFFFF312E312E3300", 28)
                0x6C, 0x01, 0x00, 0x00, // GarageDepth = 1.1f
                0xFE, 0xFF, 0xFF, 0xFF, // GrooveWidth  = -1.0f
                0x31, 0x2E, 0x31, 0x2E, 0x33, 0x00 // "1.1.3\0"
            ];*/

            // 2. PLR – offset 2463284 in Python
            //byte[] sigPlr = Convert.FromHexString(GTR2_MEMORY_SIG_PLR);
            byte[] sigPlr = [ // Python: self.plrai = (2463284, "00401C47 CDCCCC3D", -508)  # f 40000
                0x00, 0x40, 0x1C, 0x47, // 40000.0f
                0xCD, 0xCC, 0xCC, 0x3D  // 0.1f
            ];

            // 3. GDB Horizon – offset 2440065 in Python
            //byte[] sigGdb = Convert.FromHexString(GTR2_MEMORY_SIG_GDB_HORIZON);
            byte[] sigGdb = Encoding.ASCII.GetBytes("LENS_FLARE_RAINBOW.DDS"); // Python: self.gdbhor = (2440065, "4C454E535F464C4152455F5241494E424F572E444453", 835)  # LENS_FLARE_RAINBOW.DDS

            // Relative offsets from dynamically located GTR2 memory region containing these nested memory regions
            //const nint aiwBaseOffset = GTR2_MEMORY_AIW_BASE_OFFSET;
            const Int32 gridOffsetFromAiw = GTR2_MEMORY_AIW_OFFSET_GRID;
            const Int32 gdbOffsetFromAiw = GTR2_MEMORY_AIW_OFFSET_GDB;
            const Int32 plrOffsetFromAiw = GTR2_MEMORY_AIW_OFFSET_PLR;

            App.Log.AddInfo("Scanning memory with multi-signature validation...");

            nint curAddr = 0;
            int mbiSize = Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();


            // This loop will iterate through the process's virtual memory regions. For each region, it checks if it's committed and has read-write permissions, then reads the entire region and looks for the three signatures at their expected offsets relative to each other. If all signatures match, it computes the grid address and validates it before returning.
            while (VirtualQueryEx(hProcess, curAddr, out MEMORY_BASIC_INFORMATION mbi, mbiSize) != 0)
            {

                // Only committed, read-write pages
                if (mbi.State != MEM_COMMIT || mbi.Protect != PAGE_READWRITE)
                {
                    curAddr = mbi.BaseAddress + mbi.RegionSize;
                    continue;
                }

                nint regionStart = mbi.BaseAddress;
                nint regionEnd = regionStart + mbi.RegionSize;

                // Read the whole region (safe – regions are < 4 MiB in GTR2)
                long regionSize = regionEnd - regionStart;
                byte[] region = new byte[regionSize];
                if (!ReadProcessMemory(hProcess, mbi.BaseAddress, region, (int)regionSize, out int read) ||
                    read != regionSize)
                {
                    curAddr = regionEnd;
                    continue;
                }

                /* ----------------------------------------------------------
                 *  1. Validate AIW Signature at any offset within the region. This is the anchor signature we look for first. Then every other signature is an offset from this.
                 * ---------------------------------------------------------- */
                nint aiwAddr = -1;
                for (Int32 aiwOffset = 0; aiwOffset <= regionSize - sigAiw.Length; aiwOffset++)
                {
                    if (MemorySignatureMatches(region, aiwOffset, sigAiw))
                    {
                        App.Log.AddInfo($"Found AIW signature at 0x{(regionStart + aiwOffset):X}");
                        aiwAddr = regionStart + aiwOffset;
                        break;
                    }
                }
                if (aiwAddr == -1) { curAddr = regionEnd; continue; }

                /* ----------------------------------------------------------
                 *  2. Validate PLR signature at expected offset
                 * ---------------------------------------------------------- */
                nint plrAddr = aiwAddr + plrOffsetFromAiw;
                Int32 plrOffsetFromRegionStart = (Int32)(plrAddr - regionStart);
                if (plrAddr < regionStart || plrAddr + sigPlr.Length > regionEnd ||
                    !MemorySignatureMatches(region, plrOffsetFromRegionStart, sigPlr))
                {
                    App.Log.AddError($"PLR signature mismatch at expected offset. Expected at 0x{plrAddr:X}");
                    curAddr = regionEnd;
                    continue;
                }
                else
                {
                    App.Log.AddInfo($"Confirmed PLR signature at expected offset: 0x{plrAddr:X}");
                }

                /* ----------------------------------------------------------
                 *  3. Validate GDB Horizon at expected offset
                 * ---------------------------------------------------------- */
                nint gdbAddr = aiwAddr + gdbOffsetFromAiw;
                Int32 gdbOffsetFromRegionStart = (Int32)(gdbAddr - regionStart);
                if (gdbAddr < regionStart || gdbAddr + sigGdb.Length > regionEnd ||
                    !MemorySignatureMatches(region, gdbOffsetFromRegionStart, sigGdb))
                {
                    App.Log.AddError($"GDB Horizon signature mismatch at expected offset. Expected at 0x{gdbAddr:X}");
                    curAddr = regionEnd;
                    continue;
                }
                else
                {
                    App.Log.AddInfo($"Confirmed GDB Horizon signature at expected offset: 0x{gdbAddr:X}");
                }

                /* ----------------------------------------------------------
                 *  All three signatures match → compute grid address
                 * ---------------------------------------------------------- */
                nint gridAddr = aiwAddr + gridOffsetFromAiw;

                App.Log.AddInfo($"Grid Address Memory Search Results:");
                App.Log.AddInfo($" - AIW        : 0x{aiwAddr:X}");
                App.Log.AddInfo($" - Grid Data  : 0x{gridAddr:X}");
                App.Log.AddInfo($" - GDB Horizon: 0x{gdbAddr:X}");
                App.Log.AddInfo($" - PLR AI     : 0x{plrAddr:X}");

                // If the header pattern is wrong, keep searching but that's extremely unlikely given the multi signature match it took to get here
                if (ValidateGridSlot(hProcess, gridAddr))
                    return gridAddr;

                curAddr = regionEnd;
            }

            App.Log.AddError("Failed searching memory for Grid address using multi-signature scan.");
            return nint.Zero;
        }

        private static bool MemorySignatureMatches(byte[] data, Int32 offset, byte[] pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
                if (data[offset + i] != pattern[i])
                    return false;
            return true;
        }

        // Validating grid as a linked list:
        // - First 4 bytes should equal the address of the slot itself
        // - Last 4 bytes should not be 0xFFFFFFFF ie. not an immediate terminator of a linked list
        private static bool ValidateGridSlot(nint hProcess, nint addr)
        {
            byte[] buf = new byte[12];
            if (!ReadProcessMemory(hProcess, addr, buf, 12, out int read) || read != 12)
                return false;

            uint match = BitConverter.ToUInt32(buf, 0);
            uint term = BitConverter.ToUInt32(buf, 8);

            return match == (uint)addr && term != 0xFFFFFFFFU;
        }

        private static bool ValidateEndOfGrid(nint hProcess, nint addr)
        {
            byte[] buf = new byte[12];
            if (!ReadProcessMemory(hProcess, addr, buf, 12, out int read) || read != 12)
                return false;
            uint match = BitConverter.ToUInt32(buf, 0);
            uint term = BitConverter.ToUInt32(buf, 8);
            return match == (uint)addr && term == 0xFFFFFFFFU;
        }
        private static Int32 FindSlotId(nint hProc, nint slotAddr)
        {
            Int32? slotId = FindSlotInt32Value(hProc, slotAddr, GTR2_MEMORY_SLOT_OFFSET_SLOT_ID, "SlotId") ?? throw new Exception("Failed finding Slot Id.");
            return slotId.Value;
        }
        private static string FindSlotDriverName(nint hProc, nint slotAddr)
        {
            string? driverName = FindSlotStringValue(hProc, slotAddr, GTR2_MEMORY_SLOT_OFFSET_DRIVER_NAME, GTR2_MEMORY_DRIVER_NAME_LENGTH, "DriverName") ?? throw new Exception("Failed finding DriverName.");
            return driverName!;
        }
        // - Note: I'm pretty confident GTR2 is defaulting to 0.1 for Weight Penalties on all cars. Not sure why. But I confirmed by printing the actual bytes read from memory.
        private static float FindSlotWeightPenalty(nint hProc, nint headerAddr)
        {
            float? weightPenalty = FindSlotFloatValue(hProc, headerAddr, GTR2_MEMORY_SLOT_OFFSET_WEIGHT_PENALTY, "WeightPenalty") ?? throw new Exception("Failed finding WeightPenalty.");
            return weightPenalty!.Value;
        }
        private static string FindSlotCarFilePath(nint hProc, nint slotAddr)
        {
            string? carFilePath = FindSlotStringValue(hProc, slotAddr, GTR2_MEMORY_SLOT_OFFSET_CAR_FILEPATH, GTR2_MEMORY_CAR_FILEPATH_LENGTH, "CarFilePath") ?? throw new Exception("Failed finding CarFilePath.");
            return carFilePath!;
        }
        private static string? FindSlotStringValue(nint hProc, nint slotAddr, Int32 findStringOffset, int findStringLength, string findName)
        {
            nint cur = slotAddr;

            if (!ValidateGridSlot(hProc, slotAddr))
                return null;

            nint stringAddr = cur + findStringOffset;
            if (IsAddressValid(hProc, stringAddr))
            {
                string? tempStringData = ReadMemoryString(hProc, stringAddr, findStringLength, Encoding.GetEncoding(GTR2_ENCODING_CODEPAGE)) ?? throw new Exception($"Failed reading string value at offset {findStringOffset}.");
                string stringData = tempStringData.ToString();
                App.Log.AddDebug($"Found {findName} string: {stringData}");
                return stringData;
            }

            return null;
        }
        private static Int32? FindSlotInt32Value(nint hProc, nint slotAddr, Int32 findOffset, string findName)
        {
            nint cur = slotAddr;

            if (!ValidateGridSlot(hProc, cur))
                return null;

            nint findAddr = cur + findOffset;
            if (IsAddressValid(hProc, findAddr))
            {
                Int32? tempInt32Data = ReadMemoryInt32(hProc, findAddr) ?? throw new Exception($"Failed reading current Int32 value at offset {findOffset}.");
                Int32 int32Data = tempInt32Data.Value;
                App.Log.AddDebug($"Found {findName} Int32: {int32Data}");
                return int32Data;
            }

            return null;
        }
        private static float? FindSlotFloatValue(nint hProc, nint slotAddr, Int32 findFloatOffset, string findName)
        {
            nint cur = slotAddr;

            if (!ValidateGridSlot(hProc, cur))
                return null;

            nint floatAddr = cur + findFloatOffset;
            if (IsAddressValid(hProc, floatAddr))
            {
                float? tempFloatData = ReadMemoryFloat(hProc, floatAddr) ?? throw new Exception($"Failed reading current float value at offset {findFloatOffset}.");
                float floatData = tempFloatData.Value;
                App.Log.AddDebug($"Found {findName} float: {floatData}");
                return floatData;
            }

            return null;
        }
        private static nint FollowGridAndGetWeightPenaltyAddr(nint hProc, nint headerAddr)
        {
            const nint weightPenaltySlotOffset = GTR2_MEMORY_SLOT_OFFSET_WEIGHT_PENALTY;   // slotBase + 16084 → WeightPenalty (float)
            const nint slotStep = GTR2_MEMORY_SLOT_SIZE;
            nint cur = headerAddr;

            while (true)
            {
                if (!ValidateGridSlot(hProc, cur))
                    break;

                nint wpAddr = cur + weightPenaltySlotOffset;
                if (IsAddressValid(hProc, wpAddr))
                    return wpAddr;

                cur += slotStep;
            }

            return nint.Zero;
        }

        private static bool IsAddressValid(nint hProc, nint addr)
        {
            MEMORY_BASIC_INFORMATION mbi = new();
            int sz = Marshal.SizeOf(mbi);
            return VirtualQueryEx(hProc, addr, out mbi, sz) != 0 &&
                   mbi.State == MEM_COMMIT &&
                   mbi.Protect == PAGE_READWRITE;
        }

        private static nint? ReadMemoryAddr(nint hProc, nint addr)
        {
            byte[] buf = new byte[4];
            var result = ReadProcessMemory(hProc, addr, buf, 4, out _);
            if (!result)
            {
                return null;
            }
            //nint addr? = (nint)BitConverter.ToUInt32(buf, 0);
            return (nint)BitConverter.ToUInt32(buf, 0);
        }

        // This is for reading strings that are stored directly in memory (not pointers to strings). GTR2 stores driver names as fixed-length 64-byte ASCII strings directly in the slot data, so we can read them with this helper.
        // - Caution: Callers must This will return the raw bytes as a string of hex values. You may want to convert it to ASCII, ANSI, UTF-8, etc, and trim null terminators depending on how you want to use it.
        private static string? ReadMemoryString(nint hProc, nint addr, int length, Encoding encoding)
        {
            byte[] buf = new byte[length];
            var result = ReadProcessMemory(hProc, addr, buf, length, out _);
            if (!result)
            {
                return null;
            }
            int nullPos = Array.IndexOf(buf, (byte)0);
            int actualLength = nullPos >= 0 ? nullPos : buf.Length;
            return encoding.GetString(buf, 0, actualLength);
            //return BitConverter.ToString(buf, 0);
        }
        private static Int32? ReadMemoryInt32(nint hProc, nint addr)
        {
            byte[] buf = new byte[4];
            var result = ReadProcessMemory(hProc, addr, buf, 4, out _);
            if (!result)
            {
                return null;
            }
            return BitConverter.ToInt32(buf, 0);
        }
        private static float? ReadMemoryFloat(nint hProc, nint addr)
        {
            byte[] buf = new byte[4];
            var result = ReadProcessMemory(hProc, addr, buf, 4, out _);
            if (!result)
            {
                return null;
            }
            //App.Log.Add($"Raw bytes in ReadMemoryFloat: {BitConverter.ToString(buf)} = 0x{BitConverter.ToUInt32(buf, 0):X8}");
            return BitConverter.ToSingle(buf, 0);
        }

        private static byte[] ReadMemoryByteArray(nint hProc, nint addr, int length)
        {
            byte[] buf = new byte[length];
            var result = ReadProcessMemory(hProc, addr, buf, length, out _);
            if (!result)
            {
                throw new Exception($"Failed reading byte array from memory at address 0x{addr:X} with length {length}.");
            }
            return buf;
        }

        public static bool WriteFloat(nint hProc, nint addr, float value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            return WriteProcessMemory(hProc, addr, buf, 4, out _);
        }

    }
}
