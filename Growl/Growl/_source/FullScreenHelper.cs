using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Growl
{
    public class FullScreenHelper
    {
        public event EventHandler ExitedFullscreen;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT rc);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        const int PIXEL_BUFFER = 1;
        const string WINDOW_CLASS_CONSOLE = "ConsoleWindowClass";
        const string WINDOW_CLASS_WINTAB = "Flip3D";
        const string WINDOW_CLASS_WORKERW = "WorkerW";
        const int INTERVAL = 5;    //check every 5 seconds

        private System.Timers.Timer timer;

        public FullScreenHelper()
        {
            this.timer = new System.Timers.Timer(INTERVAL * 1000);
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }

        public bool IsFullScreenAppActive()
        {
            /* Here is our logic:
             * 1. Get the current active window
             *    - If the active window is null, we are NOT fullscreen
             *    - If the active window is the desktop or shell, we are NOT fullscreen
             * 2. Get the active window class name and dimensions
             *    - If the active winddow is the console (ConsoleWindowClass), we have to check for negative dimensions
             *    - If the active window is Win+Tab (Flip3D) or WorkerW (?), we are NOT fullscreen
             *    - If the active window dimensions == the screen dimensions (with accomodations for rounding errors), we ARE fullscreen
             * */
            bool runningFullScreen = false;

            IntPtr desktopHandle = GetDesktopWindow();
            IntPtr shellHandle = GetShellWindow();
            IntPtr hWnd = GetForegroundWindow();

            if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
            {
                if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetClassName(hWnd, sb, sb.Capacity);
                    string windowClass = sb.ToString();

                    RECT appBounds;
                    GetWindowRect(hWnd, out appBounds);

                    if (windowClass == WINDOW_CLASS_CONSOLE)
                    {
                        if (appBounds.Top < 0 && appBounds.Bottom < 0) runningFullScreen = true;
                    }
                    else if (windowClass != WINDOW_CLASS_WINTAB && windowClass != WINDOW_CLASS_WORKERW)
                    {
                        Rectangle screenBounds = Screen.FromHandle(hWnd).Bounds;
                        Rectangle screenBoundsBuffer = new Rectangle(screenBounds.X - PIXEL_BUFFER, screenBounds.Y - PIXEL_BUFFER, screenBounds.Width + (2 * PIXEL_BUFFER), screenBounds.Height + (2 * PIXEL_BUFFER));
                        if (appBounds.Width >= screenBounds.Width && appBounds.Width <= screenBoundsBuffer.Width && appBounds.Height >= screenBounds.Height && appBounds.Height <= screenBoundsBuffer.Height)
                            runningFullScreen = true;
                    }
                }
            }

            return runningFullScreen;
        }

        public void StartChecking()
        {
            timer.Start();
        }

        public void OnExitedFullscreen()
        {
            if (ExitedFullscreen != null)
            {
                ExitedFullscreen(this, EventArgs.Empty);
            }
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!IsFullScreenAppActive())
            {
                timer.Stop();
                OnExitedFullscreen();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get
                {
                    return Right - Left;
                }
            }

            public int Height
            {
                get
                {
                    return Bottom - Top;
                }
            }

            public override string ToString()
            {
                return ("{X=" + this.Left.ToString() + ",Y=" + this.Top.ToString() + ",Width=" + this.Width.ToString() + ",Height=" + this.Height.ToString() + "}");
            }
        }
    }
}
