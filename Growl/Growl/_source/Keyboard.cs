using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace Growl
{
    internal class Keyboard
    {
        public delegate void KeyInterceptedEventHandler(KeyboardHookEventArgs args);
        public static event KeyInterceptedEventHandler KeyIntercepted;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104; 
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static void SetHook()
        {
            _hookID = SetHook(_proc);
        }

        public static void Unhook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            //if(nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // fire event
                OnKeyIntercepted(new KeyboardHookEventArgs(key, Control.ModifierKeys));
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void OnKeyIntercepted(KeyboardHookEventArgs args)
        {
            if (KeyIntercepted != null)
            {
                KeyIntercepted(args);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        public class KeyboardHookEventArgs : EventArgs
        {
            public KeyboardHookEventArgs(Keys key, Keys modifier)
            {
                this.Key = key;
                this.Modifier = modifier;
            }

            public Keys Key;
            public Keys Modifier;
            public Keys KeyData
            {
                get
                {
                    return (Keys)((int)Key + (int)Modifier);
                }
            }
        }
    }
}
