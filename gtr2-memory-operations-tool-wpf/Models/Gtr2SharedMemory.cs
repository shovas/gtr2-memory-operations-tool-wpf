using System;
using System.Collections.Generic;
using System.Text;

namespace Gtr2MemOpsTool.Models
{

    // Marshalled types:
    // C++                 C#
    // char          ->    byte
    // unsigned char ->    byte
    // signed char   ->    sbyte
    // bool          ->    byte
    // long          ->    int
    // unsigned long ->    uint
    // short         ->    short
    // unsigned short ->   ushort
    // ULONGLONG     ->    Int64
    public class Gtr2Constants
    {
        public const string MM_TELEMETRY_FILE_NAME = "$GTR2CrewChief_Telemetry$";
        public const string MM_SCORING_FILE_NAME = "$GTR2CrewChief_Scoring$";
        public const string MM_PITINFO_FILE_NAME = "$GTR2CrewChief_PitInfo$";
        public const string MM_EXTENDED_FILE_NAME = "$GTR2CrewChief_Extended$";

        public const string MM_HWCONTROL_FILE_NAME = "$GTR2CrewChief_HWControl$";
        public const int MM_HWCONTROL_LAYOUT_VERSION = 1;

        public const int MM_WEATHER_CONTROL_LAYOUT_VERSION = 1;

        public const int MAX_MAPPED_VEHICLES = 104;
        public const int MAX_MAPPED_IDS = 512;
        public const int MAX_STATUS_MSG_LEN = 128;
        public const int MAX_RULES_INSTRUCTION_MSG_LEN = 96;
        public const int MAX_HWCONTROL_NAME_LEN = 96;
        public const string GTR2_PROCESS_NAME = Gtr2MemOps.GTR2_PROCESS_NAME;

        public const byte RowX = 0;
        public const byte RowY = 1;
        public const byte RowZ = 2;

        // 0 Before session has begun
        // 1 Reconnaissance laps (race only)
        // 2 Grid walk-through (race only)
        // 3 Formation lap (race only)
        // 4 Starting-light countdown has begun (race only)
        // 5 Green flag
        // 6 Full course yellow / safety car
        // 7 Session stopped
        // 8 Session over
        public enum Gtr2GamePhase
        {
            Garage = 0,
            WarmUp = 1,
            GridWalk = 2,
            Formation = 3,
            Countdown = 4,
            GreenFlag = 5,
            FullCourseYellow = 6,
            SessionStopped = 7,
            SessionOver = 8
        }

        // Yellow flag states (applies to full-course only)
        // -1 Invalid
        //  0 None
        //  1 Pending
        //  2 Pits closed
        //  3 Pit lead lap
        //  4 Pits open
        //  5 Last lap
        //  6 Resume
        //  7 Race halt (not currently used)
        public enum Gtr2YellowFlagState
        {
            Invalid = -1,
            NoFlag = 0,
            Pending = 1,
            PitClosed = 2,
            PitLeadLap = 3,
            PitOpen = 4,
            LastLap = 5,
            Resume = 6,
            RaceHalt = 7
        }

        public enum Gtr2SurfaceType
        {
            Dry = 0,
            Wet = 1,
            Grass = 2,
            Dirt = 3,
            Gravel = 4,
            Kerb = 5
        }

        // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
        public enum Gtr2Sector
        {
            Sector3 = 0,
            Sector1 = 1,
            Sector2 = 2
        }

        // 0=none, 1=finished, 2=dnf, 3=dq
        public enum Gtr2FinishStatus
        {
            None = 0,
            Finished = 1,
            Dnf = 2,
            Dq = 3
        }

        // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote
        public enum Gtr2Control
        {
            Nobody = -1,
            Player = 0,
            AI = 1,
            Remote = 2
        }

        // wheel info (front left, front right, rear left, rear right)
        public enum Gtr2WheelIndex
        {
            FrontLeft = 0,
            FrontRight = 1,
            RearLeft = 2,
            RearRight = 3
        }

        // 0=none, 1=request, 2=entering, 3=stopped, 4=exiting
        public enum Gtr2PitState
        {
            None = 0,
            Request = 1,
            Entering = 2,
            Stopped = 3,
            Exiting = 4
        }

        public enum IsiTyreCompound
        {
            Default_Compound = 1,
            Hard_Compound,
            Medium_Compound,
            Soft_Compound,
            Intermediate_Compound,
            Wet_Compound,
            Monsoon_Compound
        }

        // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
        public enum Gtr2CountLapFlag
        {
            DoNotCountLap = 0,
            CountLapButNotTime = 1,
            CountLapAndTime = 2
        }

        public enum Gtr2GameMode
        {
            Unknown = 0,
            OpenPractice = 1,
            RaceWeekendOr24Hr = 3,
            Championship = 4,
            Online = 5,
            DrivingSchool = 8
        }

        public enum Gtr2MechanicalFailure
        {
            None = 0,
            Engine = 1,
            Gearbox = 2,
            Suspension = 4,
            Brakes = 5,
            Accident = 6,
            Clutch = 7,
            Electronics = 8,
            Fuel = 9
        }

        public enum Gtr2DrsRuleSet
        {
            None = 0,
            DTM18,
            F1_2011,
            F1_2013
        }

        public enum Gtr2Dtm18DrsState
        {
            Inactive = 0,
            Available3,
            Active3,
            Available2,
            Active2,
            Available1,
            Active1
        }

        public enum Gtr2F1DrsState
        {
            Inactive = 0,
            Eligible,  // Qualified to use in DRS zone.
            Available, // Qualified and is in DRS zone.
            Active
        }

        public enum Gtr2DrsSystemState
        {
            Disabled = 0,
            Enabled
        }

        public enum Gtr2AiTireChangeDelayReason
        {
            NoDelay = 0,
            Exempt = 1,
            InsufficientWetnessChange = 10,
            PitStallRequested = 11,
            PitStallEntering = 12,
            PitStallStopped = 13,
            RaceEnd = 14,
            PitsTooBusy = 15
        }
    }
}
