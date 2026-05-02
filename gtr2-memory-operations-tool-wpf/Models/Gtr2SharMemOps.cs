using Gtr2MemOpsTool.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{
    public class Gtr2SharMemOps
    {
        private readonly MappedBuffer<Gtr2Telemetry> TelemetryBuffer = new(Gtr2Constants.MM_TELEMETRY_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
        private readonly MappedBuffer<Gtr2Scoring> ScoringBuffer = new(Gtr2Constants.MM_SCORING_FILE_NAME, true  /*partial*/, true /*skipUnchanged*/);
        private readonly MappedBuffer<Gtr2Extended> ExtendedBuffer = new(Gtr2Constants.MM_EXTENDED_FILE_NAME, false /*partial*/, true /*skipUnchanged*/);
        
        public Gtr2Telemetry Gtr2Telemetry;
        public Gtr2Scoring Gtr2Scoring;
        public Gtr2Extended Gtr2Extended;

        public Gtr2SharMemOps()
        {

        }

        public void FetchGtr2SharedMemoryStructs()
        {
            App.Log.AddInfo("Getting GTR2 Shared Memory Structs");
            try
            {
                CreateGtr2MemoryStructs();

                ConnectGtr2MemoryBuffers();

                ReadGtr2MemoryBuffers();

                App.Log.AddInfo("Finished Getting GTR2 Shared Memory Structs");

                App.Log.AddDebug($"Telemetry mVersion: {MemUtils.GetStringFromBytes(Gtr2Extended.mVersion)}");
                
            }
            catch (Exception ex)
            {
                App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
                App.Log.AddError("Failed Getting GTR2 Shared Memory Structs");
                try
                {
                    DisconnectGtr2MemoryBuffers();
                }
                catch (Exception)
                {
                    App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
                }
            }
        }

        // Left here for reference but it's not the right idea. The idea should be to populate properties of this class.
        //public IGtr2Struct GetGtr2SharedScoringMemoryStruct()
        //{
        //    App.Log.AddInfo("Getting GTR2 Shared Scoring Memory Struct");
        //    try
        //    {
        //        Gtr2Telemetry = new Gtr2Telemetry();
        //
        //        ScoringBuffer.Connect();
        //
        //        ScoringBuffer.GetMappedData(ref Gtr2Scoring);
        //
        //        //Gtr2Scoring.mVehicles[0].mDriverName
        //
        //        ScoringBuffer.Disconnect();
        //
        //        App.Log.AddInfo("Finished Getting GTR2 Shared Scoring Memory Struct");
        //
        //        App.Log.AddDebug($"Telemetry mVersion: {MemUtils.GetStringFromBytes(Gtr2Extended.mVersion)}");
        //
        //    }
        //    catch (Exception ex)
        //    {
        //        App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
        //        App.Log.AddError("Failed Getting GTR2 Shared Scoring Memory Struct");
        //        try
        //        {
        //            ScoringBuffer.Disconnect();
        //        }
        //        catch (Exception)
        //        {
        //            App.Log.AddDebug($"Exception: {ex.Message} at {ex.StackTrace}");
        //        }
        //    }
        //
        //    return Gtr2Scoring;
        //}

        private void CreateGtr2MemoryStructs()
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

        // This reads the byte data into buffers. The buffers will then be parsed into the Gtr2Telemetry, Gtr2Scoring, and Gtr2Extended structs by the caller.
        private void ReadGtr2MemoryBuffers()
        {
            ExtendedBuffer.GetMappedData(ref Gtr2Extended);
            ScoringBuffer.GetMappedData(ref Gtr2Scoring);
            TelemetryBuffer.GetMappedData(ref Gtr2Telemetry);
        }
    }
}
