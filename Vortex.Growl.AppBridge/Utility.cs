using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Vortex.Growl.AppBridge
{
    public class Utility
    {
        private static string userSettingsFolder;

        static Utility()
        {
            /* this the path that the built-in LocalUserSettingsProvider uses, but the path gets funny,
             * so we decided not to use it.
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string userConfigFile = config.FilePath;
            userSettingsFolder = userConfigFile.Replace("user.config", "");
             * */

            userSettingsFolder = Application.LocalUserAppDataPath;
            if (!userSettingsFolder.EndsWith(@"\")) userSettingsFolder += @"\";
            if (!System.IO.Directory.Exists(userSettingsFolder)) System.IO.Directory.CreateDirectory(userSettingsFolder);
        }

        public static string UserSettingFolder
        {
            get
            {
                return userSettingsFolder;
            }
        }

        public static string GetDisplayUserSettingsFolder(string displayName)
        {
            string folder = String.Format(@"{0}Displays\{1}\", UserSettingFolder, displayName);
            if(!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
