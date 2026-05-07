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

        public event EventHandler<GamePhaseChangedEventArgs>? GamePhaseChanged;
        protected virtual void OnGamePhaseChanged(GamePhaseChangedEventArgs e)
        => GamePhaseChanged?.Invoke(this, e);

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
            ReadGtr2SharedMemory();
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

        private void ReadGtr2SharedMemory()
        {
            App.Log.AddDebug("ReadGtr2SharedMemory(): Start read");
            Gtr2SharMemOps.ReadGtr2MemoryBuffers();
            App.Log.AddDebug("ReadGtr2SharedMemory(): End read");
        }

        private void ProcessGtr2SharedMemoryChanges()
        {
            App.Log.AddDebug("ProcessGtr2SharedMemoryChanges(): Start processing changes");

            // Session change
            int curSession = Gtr2SharMemOps.Gtr2Scoring.mScoringInfo.mSession; // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
            int oldSession = Gtr2SharMemOps.OldGtr2Scoring.mScoringInfo.mSession;
            if (curSession != oldSession)
            {
                App.Log.AddInfo($"Session change detected: {oldSession} -> {curSession}");
                OnSessionChanged(new SessionChangedEventArgs
                {
                    Session = curSession
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

                // Lap time change
                float curLapTime = curVehicle.mLastLapTime;
                float oldLapTime = oldVehicle.mLastLapTime;
                if (curLapTime != oldLapTime)
                {
                    Encoding encoding = Encoding.GetEncoding(Gtr2ProgMemOps.GTR2_ENCODING_CODEPAGE);
                    string driverName = MemUtils.GetStringFromBytes(curVehicle.mDriverName, encoding);
                    string vehicleName = MemUtils.GetStringFromBytes(curVehicle.mVehicleName, encoding);
                    App.Log.AddInfo($"Lap time change detected for {driverName} in {vehicleName}: {oldLapTime} -> {curLapTime}");
                    OnLaptimeChanged(new LaptimeChangedEventArgs
                    {
                        DriverName = driverName,
                        VehicleName = vehicleName,
                        OldLapTime = oldLapTime,
                        NewLapTime = curLapTime
                    });
                }
            }

            App.Log.AddDebug("ProcessGtr2SharedMemoryChanges(): End processing changes");
        }
    }

    public class LaptimeChangedEventArgs : EventArgs
    {
        public string DriverName { get; set; } = string.Empty;
        public string VehicleName { get; set; } = String.Empty;
        public float OldLapTime { get; set; } = 0.0f;
        public float NewLapTime { get; set; } = 0.0f;
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
}
