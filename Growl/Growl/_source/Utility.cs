using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Growl
{
    public static class Utility
    {
        private static string userSettingsFolder;
        private static string userSettingsFolderBeta;
        private static System.Diagnostics.FileVersionInfo fileVersionInfo;
        private static bool debugMode = false;
        private static object debugLock = new object();

        static Utility()
        {
            /* this the path that the built-in LocalUserSettingsProvider uses, but the path gets funny,
             * so we decided not to use it.
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string userConfigFile = config.FilePath;
            userSettingsFolder = userConfigFile.Replace("user.config", "");
             * */

            /* we then were using Application.LocalUserAppDataPath, but that uses the FileVersion, and we dont want to do that
            userSettingsFolder = Application.LocalUserAppDataPath;
             * */

            /* we were then using Environment.SpecialFolder.LocalApplicationData + ProductVersion, but ProductVersion could
             * change with minor releases. 
             * */
            //string folder = String.Format(@"Growl\{0}", fileVersionInfo.ProductVersion);

            /* currently, we are using Environment.SpecialFolder.LocalApplicationData + AssemblyVersion. AssemblyVersion should
             * not change for anything but major releases, in which case we will want a new user folder anyway.
             * */

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);

            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = String.Format(@"Growl\{0}", a.GetName().Version.ToString());
            string folderBeta = @"Growl\2.0b2";
            userSettingsFolder = System.IO.Path.Combine(root, folder);
            if (!userSettingsFolder.EndsWith(@"\")) userSettingsFolder += @"\";
            userSettingsFolderBeta = System.IO.Path.Combine(root, folderBeta);
            if (!userSettingsFolderBeta.EndsWith(@"\")) userSettingsFolderBeta += @"\";

            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(userSettingsFolder);
        }

        public static string UserSettingFolder
        {
            get
            {
                return userSettingsFolder;
            }
        }

        public static string UserSettingFolderBeta
        {
            get
            {
                return userSettingsFolderBeta;
            }
        }

        public static System.Diagnostics.FileVersionInfo FileVersionInfo
        {
            [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
            get
            {
                return fileVersionInfo;
            }
        }

        public static string GetDisplayUserSettingsFolder(string displayName)
        {
            string folder = Growl.CoreLibrary.PathUtility.Combine(UserSettingFolder, String.Format(@"Displays\{0}\", displayName));
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(folder);
            return folder;
        }

        public static bool DebugMode
        {
            get
            {
                return debugMode;
            }
            set
            {
                debugMode = value;
            }
        }

        public static void WriteDebugInfo(string info)
        {
            string debugFile = System.IO.Path.Combine(UserSettingFolder, "debug.txt");
            lock (debugLock)
            {
                System.IO.StreamWriter w = System.IO.File.AppendText(debugFile);
                using (w)
                {
                    w.WriteLine("{0} {1}", DateTime.Now, info);
                }
            }
        }
    }
}
