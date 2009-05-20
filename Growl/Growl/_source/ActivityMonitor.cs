using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Growl
{
    internal class ActivityMonitor : IDisposable
    {
        public event ActivityMonitorEventHandler WentIdle;
        public event ActivityMonitorEventHandler ResumedActivity;
        public event EventHandler StillActive;

        public delegate void ActivityMonitorEventHandler(ActivityMonitorEventArgs args);

        private bool isStarted;
        private bool isInactive;
        private bool isLocked;
        private bool isPaused;
        private bool checkForIdle;
        private int idleAfterSeconds = 180;
        private int timerIntervalNormal = 5;
        private int timerIntervalIdle = 2;
        private long lastInputTime;
        private System.Timers.Timer timer;
        private bool disposed;

        public ActivityMonitor()
        {
            this.timer = new System.Timers.Timer(this.timerIntervalNormal * 1000);
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);

            try
            {
                SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            }
            catch
            {
                // Windows 2000 (W2K) doesn't support the SessionSwitch event, but it wont kill us to not have it
            }
        }

        public int IdleAfterSeconds
        {
            get
            {
                return this.idleAfterSeconds;
            }
            set
            {
                this.idleAfterSeconds = value;
                MaybeStartTimer();
            }
        }

        public bool CheckForIdle
        {
            get
            {
                return checkForIdle;
            }
            set
            {
                this.checkForIdle = value;
                MaybeStartTimer();
            }
        }

        public bool Start()
        {
            this.isStarted = true;
            this.isPaused = false;  // reset ispaused if we do a hard start
            MaybeStartTimer();  // this will start the idle timer if necessary
            return true;
        }

        public void Stop()
        {
            StopTimer();
            this.isStarted = false;
            this.isPaused = false;  // reset ispaused if we do a hard stop
        }

        public void PauseApplication()
        {
            if (!this.isPaused) this.OnWentIdle(new ActivityMonitorEventArgs(ActivityMonitorEventReason.ApplicationPaused));
            //StopTimer();    // dont check for idle while paused
            Stop();
            this.isPaused = true;
        }

        public void UnpauseApplication()
        {
            if (this.isPaused) this.ResumedActivity(new ActivityMonitorEventArgs(ActivityMonitorEventReason.ApplicationUnpaused));
            //StartTimer();   // start checking for idle again
            Start();
            this.isPaused = false;
        }

        public bool IsIdle
        {
            get
            {
                return (this.isInactive || this.isLocked);
            }
        }

        public bool IsStarted
        {
            get
            {
                return this.isStarted;
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.isPaused;
            }
        }

        protected void OnWentIdle(ActivityMonitorEventArgs args)
        {
            if (this.WentIdle != null)
            {
                this.WentIdle(args);
            }
        }

        protected void OnResumedActivity(ActivityMonitorEventArgs args)
        {
            if (this.ResumedActivity != null)
            {
                this.ResumedActivity(args);
            }
        }

        protected void OnStillActive(EventArgs args)
        {
            if (this.StillActive != null)
            {
                this.StillActive(this, args);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (this.timer != null) this.timer.Dispose();
                        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                    }
                    catch
                    {
                        // suppress
                    }
                }
                this.disposed = true;
            }
        }

        #endregion

        private void MaybeStartTimer()
        {
            if (this.checkForIdle && this.idleAfterSeconds > 0 && this.isStarted && !this.isPaused)
                StartTimer();
            else
                StopTimer();
        }

        private void StartTimer()
        {
            this.timer.Start();
            Console.WriteLine("idle timer started. updating every {0} seconds", this.idleAfterSeconds);
        }

        private void StopTimer()
        {
            if (this.timer != null) this.timer.Stop();
            Console.WriteLine("idle timer stopped.");
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int lastInputAt = GetIdleTime();
            int diff = Environment.TickCount - lastInputAt;
            TimeSpan idleFor = new TimeSpan(0, 0, 0, 0, diff);
            int idleForSeconds = idleFor.Seconds;

            if (this.IsIdle)
            {
                if (lastInputAt > this.lastInputTime)
                {
                    this.isInactive = false;
                    this.timer.Interval = this.timerIntervalNormal * 1000; // go back to polling at the normal interval
                    this.OnResumedActivity(new ActivityMonitorEventArgs(ActivityMonitorEventReason.UserActivity));
                }
            }
            else
            {
                if (idleForSeconds > this.idleAfterSeconds)
                {
                    this.isInactive = true;
                    this.timer.Interval = this.timerIntervalIdle * 1000;    // poll more frequently while inactive so we know more quickly when we awaken
                    this.OnWentIdle(new ActivityMonitorEventArgs(ActivityMonitorEventReason.UserInactivity));
                }
                else if (lastInputAt > this.lastInputTime && !this.IsIdle)
                {
                    this.OnStillActive(EventArgs.Empty);
                }
            }
            this.lastInputTime = lastInputAt;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                this.isLocked = true;
                StopTimer();    // dont check for idle while locked
                this.OnWentIdle(new ActivityMonitorEventArgs(ActivityMonitorEventReason.DesktopLocked));
                //Console.WriteLine("locked");
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                this.ResumedActivity(new ActivityMonitorEventArgs(ActivityMonitorEventReason.DesktopUnlocked));
                this.isLocked = false;
                StartTimer();   // start checking for idle again
                //Console.WriteLine("unlocked");
            }
        }

        public class ActivityMonitorEventArgs : EventArgs
        {
            public ActivityMonitorEventArgs(ActivityMonitorEventReason reason)
                : base()
            {
                this.Reason = reason;
            }

            public ActivityMonitorEventReason Reason;
        }

        public enum ActivityMonitorEventReason
        {
            UserInactivity,
            UserActivity,
            DesktopLocked,
            DesktopUnlocked,
            ApplicationPaused,
            ApplicationUnpaused
        }

        # region detect idle

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public Int32 cbSize;
            public Int32 dwTime;
        };

        [DllImport("USER32.DLL", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO ii);

        private static int GetIdleTime()
        {
            LASTINPUTINFO ii = new LASTINPUTINFO();
            ii.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(ii);
            if (GetLastInputInfo(ref ii))
            {
                return ii.dwTime;
            }
            else
            {
                // if it fails, return far future (to avoid triggering false 'idle' messages)
                return int.MaxValue;
            }
        }

        # endregion detect idle
    }
}
