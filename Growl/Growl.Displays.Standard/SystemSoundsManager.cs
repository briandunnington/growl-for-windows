using System;
using System.Collections.Generic;
using System.Media;
using System.Text;
using Microsoft.Win32;

namespace Growl.Displays.Standard
{
    public static class SystemSoundsManager
    {
        private const string REGISTRY_ROOT = @"AppEvents\Schemes\Apps\.Default";

        public static SoundPlayer GetSound(string systemSoundAlias)
        {
            string path = GetSystemSoundPath(systemSoundAlias);
            if (String.IsNullOrEmpty(path))
                return null;
            else
                return new SoundPlayer(path);
        }


        private static string GetSystemSoundPath(string systemSoundAlias)
        {
            string path = null;
            string keyName = String.Format("{0}\\{1}\\.Current", REGISTRY_ROOT, systemSoundAlias);
            RegistryKey key = null;

            try
            {
                key = Registry.CurrentUser.OpenSubKey(keyName, false);

                // get filename, if any
                if (key != null)
                {
                    object defaultVal = key.GetValue(null);
                    if (defaultVal != null)
                    {
                        path = defaultVal.ToString();
                        if (!System.IO.Path.IsPathRooted(path))
                        {
                            string root = System.IO.Path.Combine(Environment.SystemDirectory, @".." + System.IO.Path.DirectorySeparatorChar + "Media");
                            path = System.IO.Path.Combine(root, path);
                        }
                    }
                }
            }
            catch
            {
                path = null;
            }
            finally
            {
                // close keys
                key.Close();
            }

            return path;
        }
    }
}
