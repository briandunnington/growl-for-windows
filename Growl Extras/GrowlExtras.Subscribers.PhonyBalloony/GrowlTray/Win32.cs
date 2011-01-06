using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GrowlTray
{
    class Win32
    {
        # region Is64BitOperatingSystem

        /// <summary> 
        /// The function determines whether the current operating system is a  
        /// 64-bit operating system. 
        /// </summary> 
        /// <returns> 
        /// The function returns true if the operating system is 64-bit;  
        /// otherwise, it returns false. 
        /// </returns> 
        public static bool Is64BitOperatingSystem()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64 
            {
                return true;
            }
            else  // 32-bit programs run on both 32-bit and 64-bit Windows 
            {
                // Detect whether the current process is a 32-bit process  
                // running on a 64-bit system. 
                bool flag;
                return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                    IsWow64Process(GetCurrentProcess(), out flag)) && flag);
            }
        }

        /// <summary> 
        /// The function determins whether a method exists in the export  
        /// table of a certain module. 
        /// </summary> 
        /// <param name="moduleName">The name of the module</param> 
        /// <param name="methodName">The name of the method</param> 
        /// <returns> 
        /// The function returns true if the method specified by methodName  
        /// exists in the export table of the module specified by moduleName. 
        /// </returns> 
        static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule,
            [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        # endregion Is64BitOperatingSystem

        public static IntPtr HWND_BROADCAST = (IntPtr)0xffff;

        public const uint WM_COPYDATA = 0x4A;
        public const uint WM_USER = 0x400;
        public const uint WM_GETTEXTLENGTH = 0x000E;
        public const uint WM_GETTEXT = 0x000D;
        public const uint WM_CLOSE = 0x0010;
        public const uint WM_QUIT = 0x0012;

        public const int SW_HIDE = 0;

        public const uint TTS_BALLOON = 0x40;
        public const uint TTS_ALWAYSTIP = 0x01;
        public const uint TTM_POP = WM_USER + 28;

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int command);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, uint fuFlags, uint uTimeout, IntPtr lpdwResult);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("shell32.dll")]
        public static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NOTIFYICONDATA pnid);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
        
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public IntPtr cbData;
            public IntPtr lpData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TRAYINFO
        {
            public UInt32 x;
            public UInt32 y;
            public NOTIFYICONDATA nid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TRAYINFO6
        {
            public UInt32 x;
            public UInt32 y;
            public NOTIFYICONDATA6 nid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATA
        {
            /// <summary>
            /// Size of this structure, in bytes.
            /// </summary>
            public UInt32 cbSize; // DWORD

            /// <summary>
            /// Handle to the window that receives notification messages associated with an icon in the
            /// taskbar status area. The Shell uses hWnd and uID to identify which icon to operate on
            /// when Shell_NotifyIcon is invoked.
            /// </summary>
            public UInt32 hWnd; // HWND
            //public IntPtr hWnd; // HWND

            /// <summary>
            /// Application-defined identifier of the taskbar icon. The Shell uses hWnd and uID to identify
            /// which icon to operate on when Shell_NotifyIcon is invoked. You can have multiple icons
            /// associated with a single hWnd by assigning each a different uID. This feature, however
            /// is currently not used.
            /// </summary>
            public UInt32 uID; // UINT

            /// <summary>
            /// Flags that indicate which of the other members contain valid data. This member can be
            /// a combination of the NIF_XXX constants.
            /// </summary>
            public IconDataMembers Flags; //UINT

            /// <summary>
            /// Application-defined message identifier. The system uses this identifier to send
            /// notifications to the window identified in hWnd.
            /// </summary>
            public UInt32 uCallbackMsg; // UINT

            /// <summary>
            /// A handle to the icon that should be displayed. Just
            /// <see cref="Icon.Handle"/>.
            /// </summary>
            public UInt32 hIcon; // HICON

            /// <summary>
            /// String with the text for a standard ToolTip. It can have a maximum of 64 characters including
            /// the terminating NULL. For Version 5.0 and later, szTip can have a maximum of
            /// 128 characters, including the terminating NULL.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public String szTip; // TCHAR, 128 chars on 2000+

            /// <summary>
            /// State of the icon. Remember to also set the <see cref="StateMask"/>.
            /// </summary>
            public IconState State; // DWORD

            /// <summary>
            /// A value that specifies which bits of the state member are retrieved or modified.
            /// For example, setting this member to <see cref="Interop.IconState.Hidden"/>
            /// causes only the item's hidden
            /// state to be retrieved.
            /// </summary>
            public IconState StateMask; // DWORD

            /// <summary>
            /// String with the text for a balloon ToolTip. It can have a maximum of 255 characters.
            /// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String szInfo; // TCHAR, 256 chars

            /// <summary>
            /// Mainly used to set the version when <see cref="WinApi.Shell_NotifyIcon"/> is invoked
            /// with <see cref="NotifyCommand.SetVersion"/>. However, for legacy operations,
            /// the same member is also used to set timouts for balloon ToolTips.
            /// </summary>
            public UInt32 uTimeoutOrVersion; // UINT

            /// <summary>
            /// String containing a title for a balloon ToolTip. This title appears in boldface
            /// above the text. It can have a maximum of 63 characters.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public String szInfoTitle; // TCHAR, 64 chars

            /// <summary>
            /// Adds an icon to a balloon ToolTip, which is placed to the left of the title. If the
            /// <see cref="BalloonTitle"/> member is zero-length, the icon is not shown.
            /// </summary>
            public BalloonFlags dwInfoFlags; // DWORD

            /// <summary>
            /// Windows XP (Shell32.dll version 6.0) and later.<br/>
            /// - Windows 7 and later: A registered GUID that identifies the icon.
            ///   This value overrides uID and is the recommended method of identifying the icon.<br/>
            /// - Windows XP through Windows Vista: Reserved.
            /// </summary>
            public Guid guidItem; // GUID, Win7 only

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. The handle of a customized
            /// balloon icon provided by the application that should be used independently
            /// of the tray icon. If this member is non-NULL and the <see cref="Interop.BalloonFlags.User"/>
            /// flag is set, this icon is used as the balloon icon.<br/>
            /// If this member is NULL, the legacy behavior is carried out.
            /// </summary>
            public UInt32 CustomBalloonIconHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATA6
        {
            public UInt32 cbSize; // DWORD
            public UInt32 hWnd; // HWND
            public UInt32 uID; // UINT
            public IconDataMembers Flags; //UINT
            public UInt32 uCallbackMsg; // UINT
            public UInt32 hIcon; // HICON
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public String szTip; // TCHAR, 128 chars on 2000+
            public IconState State; // DWORD
            public IconState StateMask; // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public String szInfo; // TCHAR, 256 chars
            public UInt32 uTimeoutOrVersion; // UINT
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public String szInfoTitle; // TCHAR, 64 chars
            public BalloonFlags dwInfoFlags; // DWORD
            public Guid guidItem; // GUID, Win7 only
            public UInt32 CustomBalloonIconHandle;
        }

        [Flags]
        public enum IconDataMembers
        {
            /// <summary>
            /// The message ID is set.
            /// </summary>
            Message = 0x01,
            /// <summary>
            /// The notification icon is set.
            /// </summary>
            Icon = 0x02,
            /// <summary>
            /// The tooltip is set.
            /// </summary>
            Tip = 0x04,
            /// <summary>
            /// State information (<see cref="IconState"/>) is set. This
            /// applies to both <see cref="NotifyIconData.IconState"/> and
            /// <see cref="NotifyIconData.StateMask"/>.
            /// </summary>
            State = 0x08,
            /// <summary>
            /// The ballon ToolTip is set. Accordingly, the following
            /// members are set: <see cref="NotifyIconData.BalloonText"/>,
            /// <see cref="NotifyIconData.BalloonTitle"/>, <see cref="NotifyIconData.BalloonFlags"/>,
            /// and <see cref="NotifyIconData.VersionOrTimeout"/>.
            /// </summary>
            Info = 0x10,
            /// <summary>
            /// Internal identifier is set. Reserved, thus commented out.
            /// </summary>
            //Guid = 0x20,
            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. If the ToolTip
            /// cannot be displayed immediately, discard it.<br/>
            /// Use this flag for ToolTips that represent real-time information which
            /// would be meaningless or misleading if displayed at a later time.
            /// For example, a message that states "Your telephone is ringing."<br/>
            /// This modifies and must be combined with the <see cref="Info"/> flag.
            /// </summary>
            Realtime = 0x40,
            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later.
            /// Use the standard ToolTip. Normally, when uVersion is set
            /// to NOTIFYICON_VERSION_4, the standard ToolTip is replaced
            /// by the application-drawn pop-up user interface (UI).
            /// If the application wants to show the standard tooltip
            /// in that case, regardless of whether the on-hover UI is showing,
            /// it can specify NIF_SHOWTIP to indicate the standard tooltip
            /// should still be shown.<br/>
            /// Note that the NIF_SHOWTIP flag is effective until the next call 
            /// to Shell_NotifyIcon.
            /// </summary>
            UseLegacyToolTips = 0x80
        }

        [Flags]
        public enum IconState
        {
            Visible = 0x00,
            Hidden = 0x01,
            /// <summary>
            /// The icon is shared - currently not supported, thus commented out.
            /// </summary>
            //Shared = 0x02
        }

        [Flags]
        public enum BalloonFlags
        {
            /// <summary>
            /// No icon is displayed.
            /// </summary>
            NONE = 0x00,
            /// <summary>
            /// An information icon is displayed.
            /// </summary>
            INFO = 0x01,
            /// <summary>
            /// A warning icon is displayed.
            /// </summary>
            WARN = 0x02,
            /// <summary>
            /// An error icon is displayed.
            /// </summary>
            CRIT = 0x03,
            /// <summary>
            /// Windows XP Service Pack 2 (SP2) and later.
            /// Use a custom icon as the title icon.
            /// </summary>
            USER = 0x04,
            /// <summary>
            /// Windows XP (Shell32.dll version 6.0) and later.
            /// Do not play the associated sound. Applies only to balloon ToolTips.
            /// </summary>
            NoSound = 0x10,
            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. The large version
            /// of the icon should be used as the balloon icon. This corresponds to the
            /// icon with dimensions SM_CXICON x SM_CYICON. If this flag is not set,
            /// the icon with dimensions XM_CXSMICON x SM_CYSMICON is used.<br/>
            /// - This flag can be used with all stock icons.<br/>
            /// - Applications that use older customized icons (NIIF_USER with hIcon) must
            ///   provide a new SM_CXICON x SM_CYICON version in the tray icon (hIcon). These
            ///   icons are scaled down when they are displayed in the System Tray or
            ///   System Control Area (SCA).<br/>
            /// - New customized icons (NIIF_USER with hBalloonIcon) must supply an
            ///   SM_CXICON x SM_CYICON version in the supplied icon (hBalloonIcon).
            /// </summary>
            LargeIcon = 0x20,
            /// <summary>
            /// Windows 7 and later.
            /// </summary>
            RespectQuietTime = 0x80
        }
    }
}
