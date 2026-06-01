using Gtr2MemOpsTool.Helpers;
using Gtr2MemOpsTool.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace Gtr2MemOpsTool.Models
{
    public class Gtr2SharedMemoryWatcher
    {
        public event EventHandler<SessionChangedEventArgs>? SessionChanged;
        protected virtual void OnSessionChanged(SessionChangedEventArgs e)
        => SessionChanged?.Invoke(this, e);

        public event EventHandler<SessionStartedChangedEventArgs>? SessionStartedChanged;
        protected virtual void OnSessionStartedChanged(SessionStartedChangedEventArgs e)
        => SessionStartedChanged?.Invoke(this, e);

        public event EventHandler<GamePhaseChangedEventArgs>? GamePhaseChanged;
        protected virtual void OnGamePhaseChanged(GamePhaseChangedEventArgs e)
        => GamePhaseChanged?.Invoke(this, e);

        public event EventHandler<PlaceChangedEventArgs>? PlaceChanged;
        protected virtual void OnPlaceChanged(PlaceChangedEventArgs e)
        => PlaceChanged?.Invoke(this, e);

        public event EventHandler<LaptimeChangedEventArgs>? LaptimeChanged;
        protected virtual void OnLaptimeChanged(LaptimeChangedEventArgs e)
        => LaptimeChanged?.Invoke(this, e);

        public readonly Gtr2SharMemOps Gtr2SharMemOps;

        private DispatcherTimer? _watchRefreshTimer;

        public Gtr2SharedMemoryWatcher()
        {
            Gtr2SharMemOps = new Gtr2SharMemOps();
        }

        public void WatchGtr2SharedMemory()
        {
            App.Log.AddInfo("Watching GTR2 Shared Memory...");
            StartWatchSharedMemoryRefreshTimer();
        }

        public void UnwatchGtr2SharedMemory()
        {
            App.Log.AddInfo("Unwatching GTR2 Shared Memory...");
            StopWatchSharedMemoryRefreshTimer();
        }

        public void PauseGtr2SharedMemoryWatcher()
        {
            if (_watchRefreshTimer is null)
            {
                throw new Exception("Watcher hasn't been started yet");
            }
            if (_watchRefreshTimer.IsEnabled)
            {
                App.Log.AddDebug("watch shared memory refresh timer already started");
            }
            else
            {
                _watchRefreshTimer.IsEnabled = true;

            }
            return;
        }

        public void UnpauseGtr2SharedMemoryWatcher()
        {
            if (_watchRefreshTimer is null)
            {
                throw new Exception("Watcher hasn't been started yet");
            }
            if (_watchRefreshTimer is not null)
            {
                _watchRefreshTimer.IsEnabled = false;
            }
        }

        private void StartWatchSharedMemoryRefreshTimer()
        {
            App.Log.AddDebug("Starting watch shared memory refresh timer...");

            // Enable existing timer
            if (_watchRefreshTimer is not null)
            {
                if (_watchRefreshTimer.IsEnabled)
                {
                    App.Log.AddDebug("Watch shared memory refresh timer already started");
                }
                else
                {
                    _watchRefreshTimer.IsEnabled = true;
                }
                return;
            }

            // Setup new timer
            int refreshTime = int.TryParse(App.Config.IniData.Sections["Gtr2SharedMemoryWatcher"]["WatchSharedMemoryTime"], out int result) ? result : 1;
            _watchRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(refreshTime)
            };
            _watchRefreshTimer.Tick += OnWatchSharedMemoryTimerTick;
            ConnectWatchSharedMemory();
            ReadGtr2SharedMemory(); // Immediate read on start
            _watchRefreshTimer.Start();
        }

        private void StopWatchSharedMemoryRefreshTimer()
        {
            App.Log.AddDebug("Stopping watch shared memory refresh timer...");
            if (_watchRefreshTimer is not null)
            {
                _watchRefreshTimer.IsEnabled = false;
            }
            DisconnectWatchSharedMemory();
        }

        private void OnWatchSharedMemoryTimerTick(object? sender, EventArgs e)
        {
            App.Log.AddDebug("Handling watch shared memory refresh timer tick...");
            RefreshWatchSharedMemory();
        }

        private async void RefreshWatchSharedMemory()
        {
            App.Log.AddDebug("RefreshWatchSharedMemory()");
            bool readSuccess = ReadGtr2SharedMemory();
            if (!readSuccess)
            {
                App.Log.AddDebug("Failed reading GTR2 Shared Memory");
                return;
            }
            ProcessGtr2SharedMemoryChanges();
        }

        private void ConnectWatchSharedMemory()
        {
            Gtr2SharMemOps.ConnectGtr2MemoryBuffers();
        }

        private void DisconnectWatchSharedMemory()
        {
            Gtr2SharMemOps.DisconnectGtr2MemoryBuffers();
        }

        private bool ReadGtr2SharedMemory()
        {
            App.Log.AddDebug("ReadGtr2SharedMemory(): Start read");
            bool readSuccess = Gtr2SharMemOps.ReadGtr2MemoryBuffers();
            App.Log.AddDebug("ReadGtr2SharedMemory(): End read");
            return readSuccess;
        }

        private void ProcessGtr2SharedMemoryChanges()
        {
            App.Log.AddDebug("ProcessGtr2SharedMemoryChanges(): Start processing changes");

            // Session change
            int curSession = Gtr2SharMemOps.Gtr2Scoring.mScoringInfo.mSession;
            int oldSession = Gtr2SharMemOps.OldGtr2Scoring.mScoringInfo.mSession;
            if (curSession != oldSession)
            {
                App.Log.AddInfo($"Session change detected: {oldSession} -> {curSession}");
                OnSessionChanged(new SessionChangedEventArgs
                {
                    Session = curSession
                });
            }

            // Session Started change
            int curSessionStarted = Gtr2SharMemOps.Gtr2Extended.mSessionStarted;
            int oldSessionStarted = Gtr2SharMemOps.OldGtr2Extended.mSessionStarted;
            if (curSessionStarted != oldSessionStarted)
            {
                App.Log.AddInfo($"Session Started change detected: {oldSessionStarted} -> {curSessionStarted}");
                OnSessionStartedChanged(new SessionStartedChangedEventArgs
                {
                    SessionStarted = curSessionStarted
                });
            }

            // Game Phase change
            int curGamePhase = Gtr2SharMemOps.Gtr2Scoring.mScoringInfo.mGamePhase;
            int oldGamePhase = Gtr2SharMemOps.OldGtr2Scoring.mScoringInfo.mGamePhase;
            if (curGamePhase != oldGamePhase)
            {
                App.Log.AddInfo($"Game phase change detected: {oldGamePhase} -> {curGamePhase}");
                OnGamePhaseChanged(new GamePhaseChangedEventArgs
                {
                    GamePhase = curGamePhase
                });
            }

            // Vehicle changes
            for (int i = 0; i < Gtr2SharMemOps.Gtr2Scoring.mVehicles.Length; i++)
            {
                Gtr2VehicleScoring curVehicle = Gtr2SharMemOps.Gtr2Scoring.mVehicles[i];
                Gtr2VehicleScoring oldVehicle = Gtr2SharMemOps.OldGtr2Scoring.mVehicles[i];
                int vehicleSlotId = curVehicle.mID;

                // Place change

                int curPlace = curVehicle.mPlace;
                int oldPlace = oldVehicle.mPlace;
                if (curPlace != oldPlace)
                {
                    Encoding encoding = Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE);
                    string driverName = MemUtils.GetStringFromBytes(curVehicle.mDriverName, encoding);
                    App.Log.AddInfo($"Place change detected for {driverName}: {oldPlace} -> {curPlace}");
                    OnPlaceChanged(new PlaceChangedEventArgs
                    {
                        VehicleSlotId = vehicleSlotId,
                        DriverName = driverName,
                        Place = curPlace
                    });
                }

                // Lap time change
                // - Needs to be based on *lap* change as multiple laps might record the same laptime and this logic wouldn't trigger ie. if (curLapTime != oldLapTime). So, base it on mTotalLaps being different and record the new laptime whatever it is.
                int curLap = curVehicle.mTotalLaps;
                int oldLap = oldVehicle.mTotalLaps;
                if (curLap != oldLap)
                {
                    float curLapTime = curVehicle.mLastLapTime;
                    float oldLapTime = oldVehicle.mLastLapTime;
                    Encoding encoding = Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE);
                    string driverName = MemUtils.GetStringFromBytes(curVehicle.mDriverName, encoding);
                    string vehicleName = MemUtils.GetStringFromBytes(curVehicle.mVehicleName, encoding);
                    App.Log.AddInfo($"Lap time change detected for {driverName} in {vehicleName}: {oldLapTime} (Lap {oldLap}) -> {curLapTime} (Lap {curLap}). Note: Identical laptimes across laps can happen.");
                    OnLaptimeChanged(new LaptimeChangedEventArgs
                    {
                        VehicleSlotId = vehicleSlotId,
                        DriverName = driverName,
                        VehicleName = vehicleName,
                        OldLap = oldLap,
                        CurLap = curLap,
                        OldLapTime = oldLapTime,
                        CurLapTime = curLapTime
                    });
                }
            }

            App.Log.AddDebug("ProcessGtr2SharedMemoryChanges(): End processing changes");
        }
    }

    public class SessionChangedEventArgs : EventArgs
    {
        public int Session { get; set; } = 0;
        public string SessionName
        {
            get
            {
                return Session switch
                {
                    0 => "Test Day",
                    1 => "Practice 1",
                    2 => "Practice 2",
                    3 => "Qualifying 1",
                    4 => "Qualifying 2",
                    5 => "Warmup",
                    6 => "Race",
                    _ => throw new NotImplementedException()
                };
            }
        }
    }

    public class SessionStartedChangedEventArgs : EventArgs
    {
        public int SessionStarted { get; set; } = 0;
    }

    public class GamePhaseChangedEventArgs : EventArgs
    {
        public int GamePhase { get; set; } = 0;
        public string GamePhaseName
        {
            get
            {
                // current game phase (0=unknown 1=pre-session 2=in-session 3=post-session)
                return GamePhase switch
                {
                    0 => "Garage",
                    1 => "WarmUp",
                    2 => "GridWalk",
                    3 => "Formation",
                    4 => "Countdown",
                    5 => "GreenFlag",
                    6 => "FullCourseYellow",
                    7 => "SessionStopped",
                    8 => "SessionOver",
                    _ => throw new NotImplementedException()
                };
            }
        }
    }

    public class PlaceChangedEventArgs : EventArgs
    {
        public int VehicleSlotId { get; set; } = 0;
        public string DriverName { get; set; } = string.Empty;
        public int Place { get; set; } = 0;
    }
    public class LaptimeChangedEventArgs : EventArgs
    {
        public int VehicleSlotId { get; set; } = 0;
        public string DriverName { get; set; } = string.Empty;
        public string VehicleName { get; set; } = String.Empty;
        public int OldLap { get; set; } = 0;
        public int CurLap { get; set; } = 0;
        public float OldLapTime { get; set; } = 0.0f;
        public float CurLapTime { get; set; } = 0.0f;
    }
}
