using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Growl.DisplayStyle;


namespace Growl
{
    internal sealed class DisplayStyleManager
    {
        const string DISPLAY_PLUGIN_INFO_SETTINGS_FILENAME = "displayPluginInfo.settings";

        internal delegate void DisplayLoadedEventHandler(Display display);
        internal static event DisplayLoadedEventHandler DisplayLoaded;

        private static List<string> ignoreList = new List<string>();
        private static string userDisplayStyleDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"Displays" + Path.DirectorySeparatorChar);
        private static string commonDisplayStyleDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.CommonPluginFolder, @"Displays" + Path.DirectorySeparatorChar);

        private static Dictionary<string, LoadedDisplayStyle> currentlyLoadedDisplayStyles = new Dictionary<string, LoadedDisplayStyle>();
        private static Dictionary<string, SettingsPanelBase> settingsPanels = new Dictionary<string, SettingsPanelBase>();
        private static Dictionary<string, Display> availableDisplays = new Dictionary<string, Display>();
        private static Dictionary<string, PluginInfo> loadedPlugins = new Dictionary<string, PluginInfo>();
        private static List<PluginInfo> loadedPluginsList = new List<PluginInfo>();

        private static SettingSaver ssPluginInfo = new SettingSaver(DISPLAY_PLUGIN_INFO_SETTINGS_FILENAME);

        static Type KnownTypeIDisplay = typeof(IDisplay);
        static Type KnownTypeDisplay = typeof(Display);
        static Type KnownTypeVisualDisplay = typeof(VisualDisplay);

		private DisplayStyleManager() {}

        static DisplayStyleManager()
        {
            ignoreList.Add("growl.displaystyle.dll");
            ignoreList.Add("growl.connector.dll");
            ignoreList.Add("growl.corelibrary.dll");
        }

		public static void Load()
		{
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(userDisplayStyleDirectory);
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(commonDisplayStyleDirectory);

            // built-in displays
            LoadBuiltIn(new Growl.Displays.Standard.StandardDisplay());
            LoadBuiltIn(new Growl.Displays.Smokestack.SmokestackDisplay());
            LoadBuiltIn(new Growl.Displays.Plain.PlainDisplay());
            LoadBuiltIn(new Growl.Displays.Toast.ToastDisplay());
            LoadBuiltIn(new Growl.Displays.Visor.VisorDisplay());
            // -- add additional built-in display styles here

            // user-specific displays
            // -- read list of known additional displays from .config file or similar
            List<PluginInfo> pis = (List<PluginInfo>)ssPluginInfo.Load();
            if (pis != null)
            {
                foreach (PluginInfo pi in pis)
                {
                    LoadPlugin(pi, null);
                }
                pis.Clear();
                pis = null;
            }

            // kick off an async call to discover new display plugins
            System.Threading.ThreadPool.RegisterWaitForSingleObject(Program.ProgramLoadedResetEvent, new System.Threading.WaitOrTimerCallback(DiscoverNewDisplayPlugins), null, -1, true);
		}

        private static void LoadBuiltIn(IDisplay display)
        {
            if (display != null)
            {
                // for the 'path' value, we still want to use the userprofile directory, even though these are built-in displays
                // the reason is that the 'path' is where display settings will be saved, so it needs to be user writable
                string path = Path.Combine(userDisplayStyleDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(display.Name));
                LoadInternal(display, null, path);
            }
        }

        private static void LoadFolder(string folder)
        {
            try
            {
                if (!loadedPlugins.ContainsKey(folder))
                {
                    PluginFinder pf = new PluginFinder();
                    IDisplay plugin = pf.Search<IDisplay>(folder, CheckType, ignoreList);
                    if (plugin != null)
                    {
                        Type type = plugin.GetType();
                        PluginInfo pi = new PluginInfo(folder, type);

                        LoadPlugin(pi, plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                // suppress any per-plugin loading exceptions
                Utility.WriteDebugInfo(String.Format("Display failed to load: '{0}' - {1} - {2}", folder, ex.Message, ex.StackTrace));
            }
        }

        private static void LoadPlugin(PluginInfo pi, IDisplay display)
        {
            try
            {
                // load if not already loaded
                if (display == null)
                {
                    PluginFinder pf = new PluginFinder();
                    display = pf.Load<IDisplay>(pi, ignoreList);
                }

                if (display != null)
                {
                    // for the 'path' value, we still want to use the userprofile directory no matter where the display was loaded from
                    // the reason is that the 'path' is where display settings will be saved, so it needs to be user writable and user-specific
                    string path = Path.Combine(userDisplayStyleDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(display.Name));

                    // check to make sure this display was not loaded from another directory already
                    if (!loadedPlugins.ContainsKey(path))
                    {
                        loadedPlugins.Add(pi.FolderPath, pi);
                        if (!loadedPlugins.ContainsKey(path)) loadedPlugins.Add(path, pi); // link by the settings path as well so we can detect duplicate plugins in other folders
                        loadedPluginsList.Add(pi);

                        LoadInternal(display, pi.FolderPath, path);
                    }
                    else
                    {
                        // plugin was not valid
                        Utility.WriteDebugInfo(String.Format("Display not loaded: '{0}' - Duplicate display was already loaded from another folder", pi.FolderPath));
                    }
                }
                else
                {
                    // plugin was not valid
                    Utility.WriteDebugInfo(String.Format("Display not loaded: '{0}' - Does not implement IDisplay interface", pi.FolderPath));
                }
            }
            catch (Exception ex)
            {
                // suppress any per-plugin loading exceptions
                Utility.WriteDebugInfo(String.Format("Display failed to load: '{0}' - {1} - {2}", pi.FolderPath, ex.Message, ex.StackTrace));
            }
        }

        private static void LoadInternal(IDisplay display, string installPath, string settingsPath)
        {
            string name = null;
            try
            {
                if (display != null)
                {
                    name = display.Name;
                    string typeName = display.GetType().FullName;
                    LoadedDisplayStyle loadedDisplayStyle = new LoadedDisplayStyle(display);

                    loadedDisplayStyle.SetGrowlApplicationPath(Application.StartupPath);
                    loadedDisplayStyle.SetDisplayStylePath(installPath);

                    if (loadedDisplayStyle.Display.SettingsPanel != null)
                    {
                        settingsPanels.Add(typeName, loadedDisplayStyle.Display.SettingsPanel);
                        loadedDisplayStyle.Display.SettingsPanel.SetDirectories(installPath, settingsPath);
                        loadedDisplayStyle.Display.SettingsCollection = loadedDisplayStyle.Display.SettingsPanel.GetSettings();
                    }

                    loadedDisplayStyle.Load();
                    currentlyLoadedDisplayStyles.Add(typeName, loadedDisplayStyle);

                    Utility.WriteDebugInfo(String.Format("Display '{0}' was loaded successfully", name));

                    // now that the display has been loaded, add it (and any subdisplays) the the list of available displays
                    string[] displays = loadedDisplayStyle.Display.GetListOfAvailableDisplays();
                    foreach (string displayName in displays)
                    {
                        Growl.Display d = new Growl.Display(displayName, loadedDisplayStyle.Display);
                        availableDisplays.Add(displayName, d);

                        OnDisplayLoaded(d);

                        Utility.WriteDebugInfo(String.Format("Display '{0}' handles the '{1}' display style", name, displayName));
                    }
                }
            }
            catch (Exception ex)
            {
                // suppress any per-display loading exceptions
                Utility.WriteDebugInfo(String.Format("Display failed to load: '{0}' - {1} - {2}", name, ex.Message, ex.StackTrace));
            }
        }

        public static Dictionary<string, Growl.Display> AvailableDisplayStyles
		{
            get
            {
                return availableDisplays;
            }
		}

        internal static Display FindDisplayStyle(string name)
        {
            if(!String.IsNullOrEmpty(name) && availableDisplays.ContainsKey(name))
                return availableDisplays[name];

            return Display.Default;
        }

        internal static SettingsPanelBase GetSettingsPanel(string typeName)
        {
            if (settingsPanels.ContainsKey(typeName))
                return settingsPanels[typeName];
            else return null;
        }

        private static void OnDisplayLoaded(Display display)
        {
            if (DisplayLoaded != null)
            {
                DisplayLoaded(display);
            }
        }

        public static string UserDisplayStyleDirectory
        {
            get
            {
                return userDisplayStyleDirectory;
            }
        }

        public static void DiscoverNewDisplayPlugins()
        {
            // we want to check both locations
            string[] userFolders = Directory.GetDirectories(userDisplayStyleDirectory);
            string[] commonFolders = Directory.GetDirectories(commonDisplayStyleDirectory);
            string[] folders = new string[userFolders.Length + commonFolders.Length];
            userFolders.CopyTo(folders, 0);
            commonFolders.CopyTo(folders, userFolders.Length);

            foreach (string folder in folders)
            {
                LoadFolder(folder);
            }

            ssPluginInfo.Save(loadedPluginsList);
        }

        private static void DiscoverNewDisplayPlugins(object state, bool timedOut)
        {
            // explantion: this method waits for Program.ProgramLoadedResetEvent to trigger it.
            // however, if it starts up immediatly, the app has just barely loaded and is probably
            // still churning away at a few other async tasks as well, so we dont want to add to the
            // load right away. this gives the app a chance to catch up a bit since this operation
            // can be rather processor intensive.
            // (specifically, this fixes an issue with the 'Growl is running' notification fading 
            // in very slowly/crudely due to processor spike)
            System.Threading.Thread.Sleep(500);

            try
            {
                DiscoverNewDisplayPlugins();
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(String.Format("Exception: DiscoverNewDisplayPlugins: {0} - {1}", ex.Message, ex.StackTrace));
                //throw;
            }
            finally
            {
                // signal any other threads that they may proceed now
                Program.ProgramLoadedResetEvent.Set();
            }
        }

        private static bool CheckType(Type type)
        {
            bool valid = false;
            if (type != null && type != KnownTypeIDisplay && type != KnownTypeDisplay && type != KnownTypeVisualDisplay && !type.IsAbstract && KnownTypeIDisplay.IsAssignableFrom(type))
                valid = true;
            return valid;
        }
	}
}
