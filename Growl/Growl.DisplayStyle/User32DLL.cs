using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Provides access to Win32 unmanaged APIs
    /// </summary>
    public static class User32DLL
    {
        #region Class Variables
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5; 
        public const int SW_RESTORE = 9; 
        public const int HWND_TOPMOST = -0x1;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOSIZE = 0x0001;
        #endregion

        #region Class Functions

        [DllImport("user32.dll")]
        [ return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        #endregion
    }

}
