using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Growl
{
    /// <summary>
    /// Provides access to commonly used properties and methods
    /// </summary>
    public static class Utility
    {
        private static string userSettingsFolder;
        private static string commonPluginFolder;
        private static System.Diagnostics.FileVersionInfo fileVersionInfo;
        private static bool debugMode;
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

            string userroot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string commonroot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string subfolder = String.Format(@"Growl" + System.IO.Path.DirectorySeparatorChar + "{0}", a.GetName().Version.ToString());

            userSettingsFolder = System.IO.Path.Combine(userroot, subfolder);
            if (!userSettingsFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) userSettingsFolder += System.IO.Path.DirectorySeparatorChar;
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(userSettingsFolder);

            commonPluginFolder = System.IO.Path.Combine(commonroot, subfolder);
            if (!commonPluginFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) commonPluginFolder += System.IO.Path.DirectorySeparatorChar;
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(commonPluginFolder);

            // this allows plugins and other external code to write to the debug file
            Growl.CoreLibrary.DebugInfo.Write += new Growl.CoreLibrary.DebugInfo.WriteEventHandler(DebugInfo_Write);
        }

        /// <summary>
        /// Gets the full path to folder where all user settings are saved.
        /// </summary>
        /// <value>Full folder path</value>
        public static string UserSettingFolder
        {
            get
            {
                return userSettingsFolder;
            }
        }

        /// <summary>
        /// Gets the full path to folder where shared plugins are loaded from.
        /// </summary>
        /// <value>Full folder path</value>
        public static string CommonPluginFolder
        {
            get
            {
                return commonPluginFolder;
            }
        }

        /// <summary>
        /// Gets the file version info.
        /// </summary>
        /// <value>The file version info.</value>
        public static System.Diagnostics.FileVersionInfo FileVersionInfo
        {
            [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
            get
            {
                return fileVersionInfo;
            }
        }

        /// <summary>
        /// Gets the full path to the folder where a displays user settings are saved.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns>Full folder path</returns>
        public static string GetDisplayUserSettingsFolder(string displayName)
        {
            string folder = Growl.CoreLibrary.PathUtility.Combine(UserSettingFolder, String.Format(@"Displays\{0}\", displayName));
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(folder);
            return folder;
        }

        /// <summary>
        /// Handles properly formatting strings, especially those from resource strings.
        /// </summary>
        /// <param name="resourceString">The resource string.</param>
        /// <returns>Properly formatted string</returns>
        /// <remarks>The main use for this method is to convert literal \n characters to newlines (since newlines are a pain to deal with in the resource xml files)</remarks>
        public static string GetResourceString(string resourceString)
        {
            resourceString = resourceString.Replace("\\n", "\n");
            return resourceString;
        }

        /// <summary>
        /// Gets an ID that can be used to uniquely identify this machine.
        /// </summary>
        /// <value>The machine ID.</value>
        public static string MachineID
        {
            get
            {
                string machineID = Properties.Settings.Default.MachineID;
                if (String.IsNullOrEmpty(machineID))
                {
                    machineID = System.Guid.NewGuid().ToString();
                    Properties.Settings.Default.MachineID = machineID;
                    Properties.Settings.Default.Save();
                }
                return machineID;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the app is running in debug mode
        /// </summary>
        /// <value><c>true</c> if in debug mode; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Writes debug info to a log file
        /// </summary>
        /// <param name="info">The information to log</param>
        public static void WriteDebugInfo(string info)
        {
            bool ok = DebugMode;

// always write debug info in debug builds
#if (DEBUG)
            ok = true;
#endif

            if (ok)
            {
                string debugFile = System.IO.Path.Combine(UserSettingFolder, "debug.txt");
                lock (debugLock)
                {
                    System.IO.StreamWriter w = System.IO.File.AppendText(debugFile);
                    using (w)
                    {
                        w.WriteLine("{0}\t{1}", DateTime.Now, info);
                    }
                }
                //WriteLine(info);
            }
        }

        /// <summary>
        /// Writes debug info to a log file
        /// </summary>
        /// <param name="info">The information to log</param>
        public static void WriteDebugInfo(string format, params object[] parameters)
        {
            bool ok = DebugMode;

            // always write debug info in debug builds
#if (DEBUG)
            ok = true;
#endif

            if (ok)
            {
                if (parameters == null || parameters.Length == 0)
                    WriteDebugInfo(format);
                else
                    WriteDebugInfo(String.Format(format, parameters));
            }
        }

        /// <summary>
        /// Handles the DebugInfo.Write event
        /// </summary>
        /// <param name="info">The info to write to the debug file</param>
        static void DebugInfo_Write(string info)
        {
            WriteDebugInfo(info);
        }

        /// <summary>
        /// Just like Console.WriteLine, but only does anything when in debug mode
        /// </summary>
        /// <param name="info">The information to log</param>
        private static void WriteLine(string info)
        {
#if (DEBUG)
            if(info != null)
                System.Diagnostics.Debug.WriteLine(info);
#endif
        }
    }
}
