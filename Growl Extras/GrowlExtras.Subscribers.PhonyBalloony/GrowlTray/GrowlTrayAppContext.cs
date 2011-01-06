using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using Growl.Connector;

namespace GrowlTray
{
    class GrowlTrayAppContext : ApplicationContext
    {
        // DEV ONLY - SUPER IMPORTANT
#if DEBUG
        bool GROWL = false;
#else
        bool GROWL = true;
#endif

        const uint MSG_STOP = Win32.WM_USER + 100;

        bool restoreBalloonRegistryOnQuit = false;
        bool isXP = isXPOS();
        bool started = false;
        bool stopping = false;
        Timer timer = null;

        Hwnd hwnd;
        GrowlConnector growl;
        string appName = "Windows Notifications";
        string ntNameInfo = "Information";
        string ntNameWarning = "Warning";
        string ntNameError = "Error";
        string ntNameOther = "Other";

        public GrowlTrayAppContext()
            : base()
        {
            this.hwnd = new Hwnd(WndProc);

            NotificationType ntInfo = new NotificationType(ntNameInfo, ntNameInfo, Properties.Resources.info, true);
            NotificationType ntWarning = new NotificationType(ntNameWarning, ntNameWarning, Properties.Resources.warning, true);
            NotificationType ntError = new NotificationType(ntNameError, ntNameError, Properties.Resources.error, true);
            NotificationType ntOther = new NotificationType(ntNameOther, ntNameOther, Properties.Resources.windows, true);

            NotificationType[] types = new NotificationType[] { ntInfo, ntWarning, ntError, ntOther };
            Growl.Connector.Application app = new Growl.Connector.Application(appName);
            app.Icon = Properties.Resources.windows;

            this.growl = new GrowlConnector();
            this.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;
            if(GROWL) this.growl.Register(app, types);

            timer = new Timer();
            timer.Interval = 5 * 1000;
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.Stop();
                if (started && !GrowlConnector.IsGrowlRunningLocally())
                {
                    Win32.PostMessage(this.hwnd.Handle, MSG_STOP, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch {}
            finally
            {
                timer.Start();
            }
        }

        private void WndProc(ref Message m)
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("{0} - {1} - {2}", m.Msg, m.WParam, m.WParam));

            // close down if we were abandonded
            if(m.Msg == (int)MSG_STOP || m.Msg == (int)Win32.WM_CLOSE || m.Msg == (int)Win32.WM_QUIT)
            {
                Log("Close message received - stopping Growl Tray");
                Stop();
            }
            else if (GROWL && started && !GrowlConnector.IsGrowlRunningLocally())
            {
                Win32.PostMessage(this.hwnd.Handle, MSG_STOP, IntPtr.Zero, IntPtr.Zero);
            }
            else if (m.Msg == (int)Win32.WM_COPYDATA)
            {
                Win32.COPYDATASTRUCT cds = new Win32.COPYDATASTRUCT();
                cds = (Win32.COPYDATASTRUCT)m.GetLParam(typeof(Win32.COPYDATASTRUCT));
                HandleCopyData(m.HWnd, m.WParam, cds);
            }
        }

        public bool Start()
        {
            if (!Start(this.hwnd.Handle))
            {
                Stop();
                Log("[!] An error occured during startup.");
                return false;
            }

            // start a timer to watch for Growl closing so we are not orphaned
            if (GROWL) timer.Start();

            return true;
        }

        private bool Start(IntPtr hWnd)
        {
            bool ok = false;

            Log("Setting hook...");
            string className = GetCurrentClassName();
            Log("className: " + className);
            bool is64bit = Win32.Is64BitOperatingSystem();
            Log("64-bit: " + is64bit.ToString());
            bool res;
            if (is64bit) res = SetTrayHook64(className);
            else res = SetTrayHook(className);
            if (res)
            {
                Log("gTraySpy hook installed");

                RegHelper reghelper = new RegHelper();
                Object key = reghelper.get(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableBalloonTips");
                if (key != null && (int)key == 0)
                {
                    Log("System balloons already disabled.");
                    ok = true;
                }
                else
                {
                    Log("Disabling system balloons...please wait a moment");
                    bool keynew = reghelper.set(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableBalloonTips", "0", RegistryValueKind.DWord);
                    if (!keynew)
                    {
                        Log("[!] Registry error. System balloon setting left unchanged.");
                    }
                    else
                    {
                        restoreBalloonRegistryOnQuit = true;
                        if (reghelper.refreshenv())
                        {
                            Log("Disabled system balloons");
                            ok = true;
                        }
                        else
                        {
                            Log("[!] Failed to update environment!");
                        }
                    }
                }
            }
            else
            {
                Log("[!] gTraySpy hook error"); 
            }

            started = ok;
            return ok;
        }

        public void Stop()
        {
            if (!stopping)
            {
                stopping = true;
                Log("Shutting down...");
                timer.Stop();

                Log("Removing trayhook...");
                if (IntPtr.Size == 8) UnsetTrayHook64();
                else UnsetTrayHook();
                Log("Trayhook removed");

                if (restoreBalloonRegistryOnQuit)
                {
                    RegHelper reghelper = new RegHelper();
                    Log("Enabling system balloons and refreshing environment...");
                    reghelper.set(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableBalloonTips", "1", RegistryValueKind.DWord);
                    reghelper.refreshenv(); // TODO
                    Log("System balloon registry restored");
                }
                started = false;
                stopping = false;
                System.Windows.Forms.Application.Exit();
            }
        }

        private bool IsFlagSet(int val, int flag)
        {
            return ((val & flag) == flag);
        }

        void Log(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
            string file = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "log.txt");
            File.AppendAllText(file, text + Environment.NewLine);
        }

        private void HandleCopyData(IntPtr hWnd, IntPtr WParam, Win32.COPYDATASTRUCT cds)
        {
            string szTitle;
            string szText;
            Win32.BalloonFlags info;
            string szClass = "NONE";
            string szIcon = "";
            uint handle;
            uint uID;
            UInt32 szPid;
            uint CustomBalloonIconHandle;
            Image image = null;

            if (isXP)
            {
                Win32.TRAYINFO ti = (Win32.TRAYINFO)Marshal.PtrToStructure(cds.lpData, typeof(Win32.TRAYINFO));
                Win32.NOTIFYICONDATA data = ti.nid;
                if (!IsFlagSet((int)ti.nid.Flags, (int)Win32.IconDataMembers.Info)) return;
                if (data.szInfo == "") return; //ignore dummy notification
                szTitle = data.szInfoTitle;
                szText = data.szInfo;
                info = data.dwInfoFlags;
                handle = data.hWnd;
                uID = data.uID;
                Win32.GetWindowThreadProcessId((IntPtr)data.hWnd, out szPid);
                CustomBalloonIconHandle = 0;

                // handle custom icons
                if (IsFlagSet((int)ti.nid.dwInfoFlags, (int)Win32.BalloonFlags.USER) && (IntPtr)ti.nid.hIcon != IntPtr.Zero)
                    if ((IntPtr)ti.nid.hIcon != IntPtr.Zero)
                    {
                        System.Drawing.Icon icon = System.Drawing.Icon.FromHandle((IntPtr)ti.nid.hIcon);
                        using (icon)
                        {
                            image = icon.ToBitmap();
                        }
                    }
            }
            else
            {
                Win32.TRAYINFO6 ti = (Win32.TRAYINFO6)Marshal.PtrToStructure(cds.lpData, typeof(Win32.TRAYINFO6));
                Win32.NOTIFYICONDATA6 data = ti.nid;
                if (!IsFlagSet((int)ti.nid.Flags, (int)Win32.IconDataMembers.Info)) return;
                if (data.szInfo == "") return; //ignore dummy notification
                szTitle = data.szInfoTitle;
                szText = data.szInfo;
                info = data.dwInfoFlags;
                handle = data.hWnd;
                uID = data.uID;
                Win32.GetWindowThreadProcessId((IntPtr)data.hWnd, out szPid);
                CustomBalloonIconHandle = data.CustomBalloonIconHandle;

                // handle custom icons
                //if (IsFlagSet((int)ti.nid.dwInfoFlags, (int)Win32.BalloonFlags.USER) && (IntPtr)ti.nid.CustomBalloonIconHandle != IntPtr.Zero)
                if ((IntPtr)ti.nid.CustomBalloonIconHandle != IntPtr.Zero)
                {
                    System.Drawing.Icon icon = System.Drawing.Icon.FromHandle((IntPtr)ti.nid.CustomBalloonIconHandle);
                    using (icon)
                    {
                        image = icon.ToBitmap();
                    }
                }
            }

            switch (info)
            {
                case Win32.BalloonFlags.INFO:
                    image = Properties.Resources.info;
                    szClass = ntNameInfo;
                    break;
                case Win32.BalloonFlags.WARN:
                    image = Properties.Resources.warning;
                    szClass = ntNameWarning;
                    break;
                case Win32.BalloonFlags.CRIT:
                    image = Properties.Resources.error;
                    szClass = ntNameError;
                    break;
                case Win32.BalloonFlags.USER:
                    // image is already set from above
                    szClass = ntNameOther;
                    break;
            }

            // DEBUG INFO
            Process pGfxApp = new Process();
            pGfxApp = Process.GetProcessById((Int32)szPid);
            string sGfxApp = pGfxApp.MainModule.FileName; // Full path to sending app EXE
            FileVersionInfo sGfxApv = pGfxApp.MainModule.FileVersionInfo;
            Log("");
            Log("[#] New notification from " + System.IO.Path.GetFileName(sGfxApp));
            Log("    [#] Title: " + szTitle);
            Log("    [#] Text: " + szText);
            Log("    [*] hWnd: " + handle
                + "; uID: " + uID
                //+ "; Flags: " + data.Flags.ToString()
                //+ "; uCallbackMsg: " + data.uCallbackMsg
                //+ "; hIcon: " + data.hIcon);
                //Log("        State: " + data.State
                //+ "; StateMask: " + data.StateMask
                //+ "; uTimeoutOrVersion: " + data.uTimeoutOrVersion
                //+ "; guidItem: " + data.guidItem
                );
            Log("    [i] Type: " + szClass);
            string szFilename = Path.GetFileNameWithoutExtension(sGfxApv.FileName);
            string szFilever = sGfxApv.FileVersion.Replace(",", ".").Replace(" ", "");
            Log("    [#] Additional file version information:");
            Log("        Source File name: " + (szFilename != "" ? szFilename : "ERROR (please report this bug)"));
            Log("        Source File version: " + (szFilever != "" ? szFilever : "[N/A]"));
            if (szTitle == "") szTitle = szFilename; // Use program name if no balloon title was set

            /*
            // CUSTOM ICON CODE
            if (CustomBalloonIconHandle == 0)
            {
                Icon gfxSource = Icon.ExtractAssociatedIcon(sGfxApp);
                if (gfxSource != null)
                {
                    Log("    [i] Using application icon...");
                    szIcon = sGfxApp + ",-1";
                }
            }
            if ((CustomBalloonIconHandle != 0) || (info == Win32.BalloonFlags.USER))
            {
                //szClass = ALERT_USER;    // not used
                szIcon = "%" + CustomBalloonIconHandle;
                Log("[i] Using requested USER icon...");
            }
             * */

            Log("");

            // TODO: NOTIFY
            Notification n = new Notification(appName, ntNameOther, String.Empty, szTitle, szText);
            n.Icon = image;
            if (GROWL) growl.Notify(n);
        }

        [DllImport("gTraySpy.dll")]
        private static extern bool SetTrayHook(string szServerName);
        [DllImport("gTraySpy.dll")]
        private static extern bool UnsetTrayHook();
        [DllImport("gTraySpy64.dll")]
        private static extern bool SetTrayHook64(string szServerName);
        [DllImport("gTraySpy64.dll")]
        private static extern bool UnsetTrayHook64();


 
        protected string GetCurrentClassName()
        {
            System.Text.StringBuilder className = new System.Text.StringBuilder(255);
            Win32.GetClassName(this.hwnd.Handle, className, 255);
            return className.ToString();
        }

        static bool isXPOS()
        {
            System.IO.FileInfo shell32 = new System.IO.FileInfo(System.IO.Path.Combine(System.Environment.SystemDirectory, "shell32.dll"));
            if (shell32.Exists)
            {
                System.Diagnostics.FileVersionInfo theVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(shell32.FullName);
                int i = theVersion.FileVersion.IndexOf('.');
                if (i > 0)
                {
                    try
                    {
                        int ver = int.Parse(theVersion.FileVersion.Substring(0, i));
                        if (ver >= 6) return false;
                        else return true;
                    }
                    catch { }
                }
            }
            return false;
        }
    }
}
