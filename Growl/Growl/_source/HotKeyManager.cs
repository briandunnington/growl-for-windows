using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Growl
{
    class HotKeyManager : IDisposable
    {
        public const int WM_HOTKEY = 0x312;

        private int id;
        private IntPtr hWnd;
        private Keys keys;

        public HotKeyManager(IntPtr handle, Keys keys)
        {
            this.hWnd = handle;
            this.id = base.GetHashCode();
            this.keys = keys;
        }

        public int ID
        {
            get
            {
                return this.id;
            }
        }

        public void Register()
        {
            KeyModifier modifiers = KeyModifier.None;

            if ((this.keys & Keys.Alt) == Keys.Alt)
                modifiers = modifiers | KeyModifier.Alt;

            if ((this.keys & Keys.Control) == Keys.Control)
                modifiers = modifiers | KeyModifier.Control;

            if ((this.keys & Keys.Shift) == Keys.Shift)
                modifiers = modifiers | KeyModifier.Shift;

            Keys k = this.keys & ~Keys.Control & ~Keys.Shift & ~Keys.Alt;

            RegisterHotKey(this.hWnd, this.id, modifiers, k);
        }

        public void Unregister()
        {
            UnregisterHotKey(this.hWnd, this.id);
        }

        public enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifier fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.Unregister();
                }
                catch
                {
                    // suppress
                }
            }
        }

        #endregion
    }
}
