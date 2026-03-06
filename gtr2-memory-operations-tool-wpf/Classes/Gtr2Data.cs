/*
From ccgepmon f0c377e https://gitlab.com/TheIronWolfModding/ccgepmonitor

GTR2 internal state mapping structures.  Allows access to native C++ structs from C#.
Must be kept in sync with Include\GTR2State.h.

See: MainForm.MainUpdate for sample on how to marshall from native in memory struct.

Authors: The Iron Wolf, The Sparten
Website: thecrewchief.org
*/
using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

// CC specific: Mark more common unused members with JsonIgnore for reduced trace sizes.



  namespace gtr2_memory_operations_tool_wpf
{
    public interface IGtr2Struct { }

    ////////////////////////////////////////////////////////////////////////////
    // DIFFERENCE FROM CC: in CC, GTR2 reuses rF2 Version Blocks.
    ////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2MappedBufferVersionBlock : IGtr2Struct
    {
      // If both version variables are equal, buffer is not being written to, or we're extremely unlucky and second check is necessary.
      // If versions don't match, buffer is being written to, or is incomplete (game crash, or missed transition).
      [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
      [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2MappedBufferVersionBlockWithSize : IGtr2Struct
    {
      [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
      [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

      [JsonIgnore] public int mBytesUpdatedHint;                                // How many bytes of the structure were written during the last update.
                                                                                // 0 means unknown (whole buffer should be considered as updated).
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2Vec3 : IGtr2Struct
    {
      public float x, y, z;
    }

    /////////////////////////////////////
    // Based on TelemWheelV2
    ////////////////////////////////////
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2Wheel : IGtr2Struct
    {
      [JsonIgnore] public float mRotation;             // radians/sec
      [JsonIgnore] public float mSuspensionDeflection; // meters
      [JsonIgnore] public float mRideHeight;           // meters
      [JsonIgnore] public float mTireLoad;             // Newtons
      [JsonIgnore] public float mLateralForce;         // Newtons
      [JsonIgnore] public float mGripFract;            // an approximation of what fraction of the contact patch is sliding
      public float mBrakeTemp;            // Celsius
      public float mPressure;             // kPa
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
      public float[] mTemperature;       // Celsius, left/center/right (not to be confused with inside/center/outside!)

      public float mWear;           // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
      [JsonIgnore] public byte[] mTerrainName;                                  // the material prefixes from the TDF file
      public byte mSurfaceType; // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip
      public byte mFlat;                 // whether tire is flat
      public byte mDetached;             // whether wheel is detached

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
      [JsonIgnore] byte[] mExpansion;                                           // for future use
    };

    //////////////////////////////////////////////////////////////////////////////////////////
    // Identical to TelemInfoV2, except where noted by MM_NEW/MM_NOT_USED comments.
    //////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2VehicleTelemetry : IGtr2Struct
    {
      // Time
      public float mDeltaTime;                                    // time since last update (seconds)
      [JsonIgnore] public int mLapNumber;                                       // current lap number
      [JsonIgnore] public float mLapStartET;                                   // time this lap was started
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      [JsonIgnore] public byte[] mVehicleName;                                  // current vehicle name
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      [JsonIgnore] public byte[] mTrackName;                                    // current track name

      // Position and derivatives
      public Gtr2Vec3 mPos;                                         // world position in meters
      public Gtr2Vec3 mLocalVel;                                    // velocity (meters/sec) in local vehicle coordinates
      [JsonIgnore] public Gtr2Vec3 mLocalAccel;                                  // acceleration (meters/sec^2) in local vehicle coordinates

      // Orientation and derivatives
      public Gtr2Vec3 mOriX; // top row of orientation matrix (also converts local vehicle vectors into world X using dot product)
      public Gtr2Vec3 mOriY; // mid row of orientation matrix (also converts local vehicle vectors into world Y using dot product)
      public Gtr2Vec3 mOriZ; // bot row of orientation matrix (also converts local vehicle vectors into world Z using dot product)
      public Gtr2Vec3 mLocalRot;      // rotation (radians/sec) in local vehicle coordinates
      public Gtr2Vec3 mLocalRotAccel; // rotational acceleration (radians/sec^2) in local vehicle coordinates

      // Vehicle status
      public int mGear;             // -1=reverse, 0=neutral, 1+=forward gears
      public float mEngineRPM;       // engine RPM
      public float mEngineWaterTemp; // Celsius
      public float mEngineOilTemp;   // Celsius
      public float mClutchRPM;       // clutch RPM

      // Driver input
      public float mUnfilteredThrottle; // ranges  0.0-1.0
      public float mUnfilteredBrake;    // ranges  0.0-1.0
      [JsonIgnore] public float mUnfilteredSteering; // ranges -1.0-1.0 (left to right)
      public float mUnfilteredClutch;   // ranges  0.0-1.0

      // Misc
      public float mSteeringArmForce; // force on steering arms

      // state/damage info
      public float mFuel;                    // amount of fuel (liters)
      public float mEngineMaxRPM;            // rev limit
      public byte mScheduledStops;  // number of scheduled pitstops
      public byte mOverheating;              // whether overheating icon is shown
      public byte mDetached;                 // whether any parts (besides wheels) have been detached
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
      public byte[] mDentSeverity;                                 // dent severity at 8 locations around the car (0=none, 1=some, 2=more)
      public float mLastImpactET;            // time of last impact
      public float mLastImpactMagnitude;     // magnitude of last impact
      public Gtr2Vec3 mLastImpactPos;        // location of last impact

      // Future use
      [JsonIgnore] public float mLastSteeringFFBValue;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 60)]
      [JsonIgnore] public byte[] mExpansion;

      // keeping this at the end of the structure to make it easier to replace in future versions
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
      public Gtr2Wheel[] mWheel; // wheel info (front left, front right, rear left, rear right)
    };

    //////////////////////////////////////////////////////////////////////////////////////////
    // Identical to ScoringInfoV2, except where noted by MM_NEW/MM_NOT_USED comments.
    //////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2ScoringInfo : IGtr2Struct
    {
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] mTrackName;                                    // current track name
      public int mSession;                                         // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
      public float mCurrentET;                                    // current time
      public float mEndET;                                        // ending time
      public int mMaxLaps;                                         // maximum laps
      public float mLapDist;                                      // distance around track
                                                                  // MM_NOT_USED
                                                                  //char *mResultsStream;                                                   // results stream additions since last update (newline-delimited and NULL-terminated)
                                                                  // MM_NEW
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
      [JsonIgnore] public byte[] pointer1;

      public int mNumVehicles; // current number of vehicles

      // Game phases:
      // 0 Before session has begun
      // 1 Reconnaissance laps (race only)
      // 2 Grid walk-through (race only)
      // 3 Formation lap (race only)
      // 4 Starting-light countdown has begun (race only)
      // 5 Green flag
      // 6 Full course yellow / safety car
      // 7 Session stopped
      // 8 Session over
      public byte mGamePhase;

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
      public byte mYellowFlagState;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
      public sbyte[] mSectorFlag;                                  // whether there are any local yellows at the moment in each sector (not sure if sector 0 is first or last, so test)
      [JsonIgnore] public byte mStartLight;                                     // start light frame (number depends on track)
      [JsonIgnore] public byte mNumRedLights;                                   // number of red lights in start sequence
      public byte mInRealtime;                                     // in realtime as opposed to at the monitor
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
      public byte[] mPlayerName;                                   // player name (including possible multiplayer override)
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      [JsonIgnore] public byte[] mPlrFileName;                                  // may be encoded to be a legal filename

      // weather
      public float mDarkCloud;      // cloud darkness? 0.0-1.0
      public float mRaining;        // raining severity 0.0-1.0
      public float mAmbientTemp;    // temperature (Celsius)
      public float mTrackTemp;      // temperature (Celsius)
      public Gtr2Vec3 mWind;        // wind speed
      public float mOnPathWetness;  // on main path 0.0-1.0
      public float mOffPathWetness; // on main path 0.0-1.0

      // Future use
      public float mCurrentDT;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 252)]
      [JsonIgnore] public byte[] mExpansion;

      // MM_NOT_USED
      // VehicleScoringInfoV2 *mVehicle;  // array of vehicle scoring info's
      // MM_NEW
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
      [JsonIgnore] public byte[] pointer2;
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    // Identical to VehicleScoringInfoV2, except where noted by MM_NEW/MM_NOT_USED comments.
    //////////////////////////////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2VehicleScoring : IGtr2Struct
    {
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
      public byte[] mDriverName;                                   // driver name
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] mVehicleName;                                  // vehicle name
      public short mTotalLaps;                                     // laps completed
      public sbyte mSector;                                        // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
      public sbyte mFinishStatus;                                  // 0=none, 1=finished, 2=dnf, 3=dq
      public float mLapDist;                                      // current distance around track
      public float mPathLateral;                                  // lateral position with respect to *very approximate* "center" path
      public float mTrackEdge;                                    // track edge (w.r.t. "center" path) on same side of track as vehicle

      public float mBestSector1;                                  // best sector 1
      public float mBestSector2;                                  // best sector 2 (plus sector 1)
      public float mBestLapTime;                                  // best lap time
      public float mLastSector1;                                  // last sector 1
      public float mLastSector2;                                  // last sector 2 (plus sector 1)
      public float mLastLapTime;                                  // last lap time
      public float mCurSector1;                                   // current sector 1 if valid
      public float mCurSector2;                                   // current sector 2 (plus sector 1) if valid
                                                                  // no current laptime because it instantly becomes "last"

      public short mNumPitstops;                                   // number of pitstops made
      public short mNumPenalties;                                  // number of outstanding penalties
      public byte mIsPlayer;                                       // is this the player's vehicle

      public sbyte mControl;                                       // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote, 3=replay (shouldn't get this)
      public byte mInPits;                                         // between pit entrance and pit exit (not always accurate for remote vehicles)
      public byte mPlace;                                          // 1-based position
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
      [JsonIgnore] public byte[] mVehicleClass;                                 // vehicle class

      // Dash Indicators
      public float mTimeBehindNext;                               // time behind vehicle in next higher place
      [JsonIgnore] public int mLapsBehindNext;                                  // laps behind vehicle in next higher place
      [JsonIgnore] public float mTimeBehindLeader;                             // time behind leader
      [JsonIgnore] public int mLapsBehindLeader;                                // laps behind leader
      public float mLapStartET;                                   // time this lap was started

      // Position and derivatives
      public Gtr2Vec3 mPos;                                         // world position in meters
      public Gtr2Vec3 mLocalVel;                                    // velocity (meters/sec) in local vehicle coordinates
      public Gtr2Vec3 mLocalAccel;                                  // acceleration (meters/sec^2) in local vehicle coordinates

      // Orientation and derivatives
      public Gtr2Vec3 mOriX; // top row of orientation matrix (also converts local vehicle vectors into world X using dot product)
      public Gtr2Vec3 mOriY; // mid row of orientation matrix (also converts local vehicle vectors into world Y using dot product)
      public Gtr2Vec3 mOriZ; // bot row of orientation matrix (also converts local vehicle vectors into world Z using dot product)
      public Gtr2Vec3 mLocalRot;      // rotation (radians/sec) in local vehicle coordinates
      public Gtr2Vec3 mLocalRotAccel; // rotational acceleration (radians/sec^2) in local vehicle coordinates

      public int mID;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 124)]
      [JsonIgnore] public byte[] mExpansion;                                    // for future use
    };

    ///////////////////////////////////////////
    // Mapped wrapper structures
    ///////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2Telemetry : IGtr2Struct
    {
      [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
      [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

      public Gtr2VehicleTelemetry mPlayerTelemetry;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2Scoring : IGtr2Struct
    {
      [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
      [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

      [JsonIgnore] public int mBytesUpdatedHint;                                // How many bytes of the structure were written during the last update.
                                                                                // 0 means unknown (whole buffer should be considered as updated).

      public Gtr2ScoringInfo mScoringInfo;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_MAPPED_VEHICLES)]
      public Gtr2VehicleScoring[] mVehicles;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2TrackedDamage : IGtr2Struct
    {
      public float mMaxImpactMagnitude;                           // Max impact magnitude.  Tracked on every telemetry update, and reset on visit to pits or Session restart.
      public float mAccumulatedImpactMagnitude;                   // Accumulated impact magnitude.  Tracked on every telemetry update, and reset on visit to pits or Session restart.
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2ExtendedBaseWheel : IGtr2Struct
    {
      [JsonIgnore] public float mWear;
      [JsonIgnore] public byte mFlat;
      [JsonIgnore] public byte mDetached;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
      [JsonIgnore] public byte[] mReserved;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2ExtendedVehicleScoring : IGtr2Struct
    {
      public int mPitState;
      // LIMITATION: Currently, this is the rear compound only.  Reason: atm front compound for AI is not known.  Perhaps it
      // is the fundamental limitation in the game.
      public int mTireCompoundIndex;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] mCurrCompoundName;
      public float mFuelCapacityLiters;
      public int mBlueFlag;
      public float mBlueFlagET;
      public byte mSpeedLimiter;
      public byte mAITireChangeDelayReason;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
      [JsonIgnore] public byte[] mReserved1;

      public int mWpBranchID;
      public float mPitLapDist;
      public int mCountLapFlag;
      public int mSpeedLimiterAvailable;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] mCarModelName;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
      public byte[] mTeamName;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
      public byte[] mCarClass;
      public int mYearAndCarNumber;
      public int mMechanicalFailureID;

      [JsonIgnore] public float mFuel;

      [JsonIgnore] public byte mDetached;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
      [JsonIgnore] public Gtr2ExtendedBaseWheel[] mWheels;

      [JsonIgnore] public byte mHasRearWing;
      [JsonIgnore] public byte mRearWingDetached;

      [JsonIgnore] public byte mGear;
      [JsonIgnore] public float mEngineRPM;

      [JsonIgnore] public float mSteeringInput;
      [JsonIgnore] public float mThrottleInput;
      [JsonIgnore] public float mBrakeInput;

      [JsonIgnore] public byte mDRSLEDState;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
      [JsonIgnore] public byte[] mReserved4;

      [JsonIgnore] public float mActiveTireWearAdjustmentPct;
      [JsonIgnore] public float mActiveFuelUseAdjustmentPct;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 88)]
      [JsonIgnore] public byte[] mReserved5;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2VehScoringCapture : IGtr2Struct
    {
      // VehicleScoringInfoV2 members:
      public int mID;                                              // slot ID (note that it can be re-used in multiplayer after someone leaves)
      public byte mPlace;
      public byte mIsPlayer;
      [JsonIgnore] public sbyte mFinishStatus;                                  // 0=none, 1=finished, 2=dnf, 3=dq
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2SessionTransitionCapture : IGtr2Struct
    {
      // ScoringInfoV2 members:
      [JsonIgnore] public byte mGamePhase;
      [JsonIgnore] public int mSession;

      // VehicleScoringInfoV2 members:
      public int mNumScoringVehicles;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_MAPPED_VEHICLES)]
      public Gtr2VehScoringCapture[] mScoringVehicles;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2ExtendedWheel : IGtr2Struct
    {
      public float mFlatSpotSeverity;
      public float mDirtPickupSeverity;

      // Currently written out only if FS emulation is on.
      public float mOptimalTempK;
      public float mColdTempK;
      public float mRadiusMeters;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
      [JsonIgnore] public byte[] mReserved;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2TimeGap : IGtr2Struct
    {
      public float mTimeDifference;
      public int mLapsDifference;
      public bool mGapIsKnown;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Gtr2SafetyCar : IGtr2Struct
    {
      public float mLapDist;
      public Gtr2Vec3 mPos;
      public Gtr2Vec3 mLocalVel;

      public Gtr2Vec3 mOriX;
      public Gtr2Vec3 mOriY;
      public Gtr2Vec3 mOriZ;
    };


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct Gtr2Extended : IGtr2Struct
    {
      [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
      [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 12)]
      public byte[] mVersion;                                      // API version

      // Damage tracking for each vehicle (indexed by mID % GTR2MappedBufferHeader::MAX_MAPPED_IDS):
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_MAPPED_IDS)]
      public Gtr2TrackedDamage[] mTrackedDamages;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_MAPPED_IDS)]
      public Gtr2ExtendedVehicleScoring[] mExtendedVehicleScoring;

      // Function call based flags:
      public byte mInRealtimeFC;                                   // in realtime as opposed to at the monitor (reported via last EnterRealtime/ExitRealtime calls).

      public byte mSessionStarted;                                 // Set to true on Session Started, set to false on Session Ended.
      [JsonIgnore] public Int64 mTicksSessionStarted;                           // Ticks when session started.
      public Int64 mTicksSessionEnded;                             // Ticks when session ended.
      public Gtr2SessionTransitionCapture mSessionTransitionCapture;// Contains partial internals capture at session transition time.

      // Direct Memory access stuff
      public byte mUnused;

      public Int64 mTicksFirstHistoryMessageUpdated;                // Ticks when first message history message was updated;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_STATUS_MSG_LEN)]
      public byte[] mFirstHistoryMessage;

      public Int64 mTicksSecondHistoryMessageUpdated;                // Ticks when second message history message was updated;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_STATUS_MSG_LEN)]
      public byte[] mSecondHistoryMessage;

      public Int64 mTicksThirdHistoryMessageUpdated;                // Ticks when third message history message was updated;
      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Gtr2Constants.MAX_STATUS_MSG_LEN)]
      public byte[] mThirdHistoryMessage;

      public float mCurrentPitSpeedLimit;                          // speed limit m/s.
      public float mFormationLapSpeeed;
      public float mTimedRaceTotalSeconds;
      public float mPitApproachLapDist;
      public int mFuelMult;
      public int mTireMult;
      public byte mInvulnerable;
      public byte mRaceDistanceIsLaps;
      public int mGameMode;

      public byte mFlatSpotEmulationEnabled;
      public byte mDirtPickupEmulationEnabled;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
      public Gtr2ExtendedWheel[] mWheels;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
      public double[] mPerGearDamage;
      public double mTotalGearboxDamage;

      public byte mAntistallActive;

      // DRS stuff.
      public byte mActiveDRSRuleSet;
      public float mActiveDRSActivationThresholdSeconds;
      public byte mActiveDRSDTM18ActivationsPerLap;
      public byte mActiveDRSDTM18ActivationsPerRace;

      public byte mCurrDRSSystemState;
      public byte mCurrDRSLEDState;
      public byte mCurrActivationsInRace;

      [JsonIgnore] public Gtr2TimeGap mGapBehind;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20)]
      [JsonIgnore] public byte[] mGapBehindStr;

      [JsonIgnore] public Gtr2TimeGap mGapAhead;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20)]
      [JsonIgnore] public byte[] mGapAheadStr;

      [JsonIgnore] public Gtr2TimeGap mDeltaPersonalBest;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20)]
      [JsonIgnore] public byte[] mDeltaPersonalBestStr;

      [JsonIgnore] public Gtr2TimeGap mDeltaClassBest;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20)]
      [JsonIgnore] public byte[] mDeltaClassBestStr;

      public Gtr2SafetyCar mSafetyCar;

      [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 192)]
      [JsonIgnore] public byte[] mReserved;

      [JsonIgnore] public int mUnsubscribedBuffersMask;                         // Currently active UnsbscribedBuffersMask value.  This will be allowed for clients to write to in the future, but not yet.

      public byte mHWControlInputEnabled;                          // HWControl input buffer is enabled.
      public byte mPluginControlInputEnabled;                      // Plugin Control input buffer is enabled.
    }
  }