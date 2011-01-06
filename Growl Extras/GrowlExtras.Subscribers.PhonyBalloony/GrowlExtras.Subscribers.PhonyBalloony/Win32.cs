using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GrowlExtras.Subscribers.PhonyBalloony
{
    class Win32
    {
        public static IntPtr HWND_BROADCAST = (IntPtr)0xffff;

        public const uint WM_COPYDATA = 0x4A;
        public const uint WM_USER = 0x400;
        public const uint WM_GETTEXTLENGTH = 0x000E;
        public const uint WM_GETTEXT = 0x000D;
        public const uint WM_CLOSE = 0x0010;

        public const int SW_HIDE = 0;

        public const uint TTS_BALLOON = 0x40;
        public const uint TTS_ALWAYSTIP = 0x01;
        public const uint TTM_POP = WM_USER + 28;

        public const int GWL_STYLE = -16;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageSB(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageTimeoutFlags flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, Int32 nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int command);

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0000,
            SMTO_BLOCK = 0x0001,
            SMTO_ABORTIFHUNG = 0x0002,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x0008
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;   // DWORD - Needs to IntPtr and not Int32 for some reason to work on Vista 64. Different versions of Win32 SDK might say different things
            public Int32 cbData;    // DWORD
            public IntPtr lpData;   // PVOID
        }


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

    }
}
