using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Growl.CoreLibrary;
using Growl.Connector;
using Growl.Destinations;

namespace GrowlExtras.Subscribers.PhonyBalloony
{
    [Serializable]
    public class PhonyBalloonySubscription : Subscription
    {
        [NonSerialized]
        object syncLock;
        [NonSerialized]
        bool starting = false;
        [NonSerialized]
        bool started = false;

        public PhonyBalloonySubscription(bool enabled)
            : base("Windows Notifications", enabled)
        {
            Initialize();
        }

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            Initialize();
        }

        void Initialize()
        {
            syncLock = new object();    // do this here because it wont be set if OnDeserialization is called
            closeGrowlTrayDel = new EnumWindowsProc(CloseGrowlTrayWindows);
            PhonyBalloonyHandler.Singleton = this;
        }

        public override string AddressDisplay
        {
            get
            {
                return "Routing Windows system balloons through Growl";
            }
        }

        public override DestinationBase Clone()
        {
            PhonyBalloonySubscription clone = new PhonyBalloonySubscription(this.Enabled);
            return clone;
        }

        public override System.Drawing.Image GetIcon()
        {
            return PhonyBalloonyHandler.Icon;
        }

        public override void Subscribe()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(StartWatchingForBalloons));
        }

        public override void Kill()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(StopWatchingForBalloons));
        }

        public override void Remove()
        {
            StopWatchingForBalloons(null);
            PhonyBalloonyHandler.Singleton = null;
        }

        void StartWatchingForBalloons(object state)
        {
            if (!starting && !started)
            {
                bool doStart = false;
                lock (syncLock)
                {
                    if (!starting && !started)
                    {
                        starting = true;
                        doStart = true;
                    }
                }
                if (!doStart) return;

                StopWatchingForBalloons(null);

                string pluginpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string path = Path.Combine(Path.GetDirectoryName(pluginpath), "GrowlTray");
                string filenamebase = "GrowlTray";
                string suffix = (Win32.Is64BitOperatingSystem() ? "64.exe" : "32.exe");
#if DEBUG
            suffix = ".exe";
#endif
                string growlTrayExe = Path.Combine(path, filenamebase + suffix);

                if (File.Exists(growlTrayExe))
                {
                    if (!started)
                    {
                        lock (syncLock)
                        {
                            if (!started)
                            {
                                Process.Start(growlTrayExe);
                                started = true;
                                starting = false;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("file does not exist: " + growlTrayExe);
                }
            }
        }

        void StopWatchingForBalloons(object state)
        {
            List<Process> processes = new List<Process>();
            Process[] gt32 = Process.GetProcessesByName("GrowlTray32");
            if (gt32 != null) processes.AddRange(gt32);
            Process[] gt64 = Process.GetProcessesByName("GrowlTray64");
            if (gt64 != null) processes.AddRange(gt64);

            foreach (Process process in processes)
            {
                EnumWindows(closeGrowlTrayDel, (uint)process.Id);
                process.WaitForExit(5000);
                process.Dispose();
            }

            started = false;
        }

        private bool CloseGrowlTrayWindows(IntPtr hwnd, uint lParam)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            if (pid == lParam)
            {
                PostMessage(hwnd, MSG_STOP, IntPtr.Zero, IntPtr.Zero);
                // dont stop enumerating
            }
            return true;
        }

        # region Win32 stuff

        const uint WM_USER = 0x400;
        const uint MSG_STOP = WM_USER + 100;
        public delegate bool EnumWindowsProc(IntPtr hwnd, uint lParam);
        EnumWindowsProc closeGrowlTrayDel;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        # endregion Win32 stuff
    }
}
