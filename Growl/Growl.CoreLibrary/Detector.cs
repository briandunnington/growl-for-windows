using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Detects whether or not Growl is installed on the local machine, and if so, returns information
    /// about the installation.
    /// </summary>
    /// <remarks>
    /// This class is useful in custom installation routines for displays or plug-ins that need to know 
    /// where Growl is installed.
    /// </remarks>
    public class Detector
    {
        /// <summary>
        /// The name of the mutex obtained by a running instance of Growl
        /// </summary>
        public const string MUTEX_NAME = "GrowlForWindows_Running";

        /// <summary>
        /// The registry key that holds installation information.
        /// The key is: HKEY_CURRENT_USER\SOFTWARE\Growl
        /// (We can't use HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\ because of UAC restrictions)
        /// </summary>
        public const string REGISTRY_KEY = @"SOFTWARE\Growl";

        /// <summary>
        /// Indicates if Growl is installed on the machine
        /// </summary>
        private bool isInstalled = false;

        /// <summary>
        /// The folder where Growl is installed
        /// </summary>
        private string installationFolder;

        /// <summary>
        /// The folder where Displays are located
        /// </summary>
        private string displaysFolder;

        /// <summary>
        /// The assembly version of Growl that is installed
        /// </summary>
        private Version assemblyVersion;

        /// <summary>
        /// The file version of Growl that is installed
        /// </summary>
        private System.Diagnostics.FileVersionInfo fileVersion;

        /// <summary>
        /// Creates a new instance of the Detector class.
        /// </summary>
        /// <remarks>
        /// When this class is created, it automatically tries to detect if Growl is installed
        /// and if so, the other relevant installation information. After the class is created,
        /// you can check the <see cref="IsInstalled"/> property to see if Growl is installed.
        /// If Growl is not installed, the other properties will be <c>null</c>.
        /// </remarks>
        public Detector()
        {
            try
            {
                string exePath = null;
                RegistryKey hkcu = Registry.CurrentUser;
                RegistryKey growlKey = hkcu.OpenSubKey(REGISTRY_KEY);
                if (growlKey != null)
                {
                    object obj = growlKey.GetValue("");
                    if (obj != null)
                    {
                        exePath = obj.ToString();
                    }
                }

                if (exePath != null)
                {
                    if (File.Exists(exePath))
                    {
                        this.isInstalled = true;

                        this.installationFolder = Path.GetDirectoryName(exePath);
                        this.displaysFolder = Path.Combine(this.installationFolder, "Displays");

                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(exePath);
                        this.assemblyVersion = assemblyName.Version;
                        this.fileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath);

                        this.isInstalled = true;
                    }
                }
            }
            catch
            {
                this.isInstalled = false;
            }
        }

        /// <summary>
        /// Indicates if Growl is installed on the machine
        /// </summary>
        /// <value>
        /// <c>true</c> - Growl is installed on the machine;
        /// <c>false</c> - Growl is not installed on the machine
        /// Note that a <c>true</c> response here simply means that Growl is installed,
        /// and does not pertain to if Growl is currently running or not.
        /// </value>
        [Obsolete("detector.IsAvailable has been replaced with detector.IsInstalled. Please update any references.")]
        public bool IsAvailable
        {
            get
            {
                return this.IsInstalled;
            }
        }

        /// <summary>
        /// Indicates if Growl is installed on the machine
        /// </summary>
        /// <value>
        /// <c>true</c> - Growl is installed on the machine;
        /// <c>false</c> - Growl is not installed on the machine
        /// Note that a <c>true</c> response here simply means that Growl is installed,
        /// and does not pertain to if Growl is currently running or not.
        /// </value>
        public bool IsInstalled
        {
            get
            {
                return this.isInstalled;
            }
        }

        /// <summary>
        /// Detects if Growl is currently running on the local machine.
        /// </summary>
        /// <value>
        /// <c>true</c> if Growl is running;
        /// <c>false</c> if Growl is not running;
        /// </value>
        public bool IsGrowlRunning
        {
            get
            {
                return Detector.DetectIfGrowlIsRunning();
            }
        }

        /// <summary>
        /// Detects if Growl is currently running on the local machine.
        /// </summary>
        /// <returns>
        /// <c>true</c> if Growl is running;
        /// <c>false</c> if Growl is not running;
        /// </returns>
        public static bool DetectIfGrowlIsRunning()
        {
            bool createdNew = true;
            Mutex mutex = new Mutex(true, String.Format(@"Global\{0}", Growl.CoreLibrary.Detector.MUTEX_NAME), out createdNew);
            using (mutex)
            {
                // do nothing (DONT call mutex.ReleaseMutex() - it can cause and exception
            }
            return !createdNew;
        }

        /// <summary>
        /// Returns the full path to where Growl is installed
        /// </summary>
        public string InstallationFolder
        {
            get
            {
                return this.installationFolder;
            }
        }

        /// <summary>
        /// Returns the full path to where Growl's displays are installed
        /// </summary>
        /// <remarks>
        /// If installing a custom display module, it should be installed into a 
        /// subfolder of this directory.
        /// </remarks>
        public string DisplaysFolder
        {
            get
            {
                return this.displaysFolder;
            }
        }

        /// <summary>
        /// The assembly version of Growl that is installed
        /// </summary>
        /// <value>
        /// <see cref="Version"/>
        /// </value>
        /// <remarks>This is the Assembly version, so it should not change amongst minor or point releases.</remarks>
        public Version AssemblyVersion
        {
            get
            {
                return this.assemblyVersion;
            }
        }

        /// <summary>
        /// The file version of Growl that is installed
        /// </summary>
        /// <value>
        /// <see cref="System.Diagnostics.FileVersionInfo"/>
        /// </value>
        /// <remarks>This is the File version, so it will be updated whenever the Growl .exe changes (including minor and point releases)</remarks>
        public System.Diagnostics.FileVersionInfo FileVersion
        {
            get
            {
                return this.fileVersion;
            }
        }
    }
}
