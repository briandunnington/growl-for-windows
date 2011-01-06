using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace GrowlTray
{
    public class RegHelper
    {
        public Object get(String key)
        {
            RegistryKey reg;
            string[] keysplit = (key).Split(System.IO.Path.DirectorySeparatorChar);
            string root = keysplit[0];
            string name = keysplit[keysplit.Length - 1];
            string path = key.Replace(root + @"\", "").Replace(@"\" + name, "");
            switch (root)
            {
                case "HKCU":
                    reg = Registry.CurrentUser;
                    break;
                case "HKLM":
                    reg = Registry.LocalMachine;
                    break;
                default:
                    return "";
            }

            reg = reg.CreateSubKey(path);
            if (reg == null) return "";
            object regObj = reg.GetValue(name);
            if (regObj == null) goto returnerror;
        returnerror:
            reg.Close();
            reg = null;
            return regObj;

        }

        public bool set(String key, String val, RegistryValueKind type)
        {
            RegistryKey reg;
            int result = 1;
            string[] keysplit = (key).Split(System.IO.Path.DirectorySeparatorChar);
            string root = keysplit[0];
            string name = keysplit[keysplit.Length - 1];
            string path = key.Replace(root + @"\", "").Replace(@"\" + name, "");
            switch (root)
            {
                case "HKCU":
                    reg = Registry.CurrentUser;
                    break;
                case "HKLM":
                    reg = Registry.LocalMachine;
                    break;
                default:
                    return false;
            }

            reg = reg.CreateSubKey(path);
            if (reg == null) result = 0;
            reg.SetValue(name, val, type);
            object regObj = reg.GetValue(name);
            if (regObj == null) result = 0;
            reg.Close();
            reg = null;
            if (result == 0) return false; else return true;

        }

        public bool refreshenv()
        {
            IntPtr HWND_BROADCAST = new IntPtr(0xffff);
            Int32 WM_SETTINGCHANGE = 0x001A;
            UInt32 SMTO_NORMAL = 0x0000;
            //UInt32 SMTO_BLOCK = 0x0001;
            UInt32 SMTO_ABORTIFHUNG = 0x0002;
            //UInt32 SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;
            //IntPtr result = Program.SendMessageTimeout((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, null, SMTO_BLOCK | SMTO_ABORTIFHUNG | SMTO_NOTIMEOUTIFNOTHUNG, 50000, IntPtr.Zero);
            
            IntPtr result = Win32.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "", SMTO_NORMAL, 10000, IntPtr.Zero);

            //IntPtr result = Win32.SendMessageTimeout(HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, "", SMTO_ABORTIFHUNG, 1000, IntPtr.Zero);

            if (result != IntPtr.Zero)
                return true;
            else
                return false;

        }


    }
}
