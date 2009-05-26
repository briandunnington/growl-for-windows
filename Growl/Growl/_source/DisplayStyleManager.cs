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
        internal delegate void DisplayLoadedEventHandler(string displayName);
        internal static event DisplayLoadedEventHandler DisplayLoaded;

        private static string appDisplayStyleDirectory = Growl.CoreLibrary.PathUtility.Combine(Application.StartupPath, @"Displays\");
        private static string userDisplayStyleDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"Displays\");
        private static Dictionary<string, LoadedDisplayStyle> currentlyLoadedDisplayStyles = new Dictionary<string, LoadedDisplayStyle>();
        private static Dictionary<string, SettingsPanelBase> settingsPanels = new Dictionary<string, SettingsPanelBase>();
        private static Dictionary<string, Display> availableDisplays;

		private DisplayStyleManager() {}

		public static void Load()
		{
            // always Unload first
            Unload();

            availableDisplays = new Dictionary<string, Display>();
            string[] displayDirectories = null;

            // built-in displays
            displayDirectories = Directory.GetDirectories(appDisplayStyleDirectory);
			for(int d=0;d<displayDirectories.Length;d++)
			{
                string directory = displayDirectories[d];
                Load(directory);
			}

            // user-specific displays
            displayDirectories = Directory.GetDirectories(userDisplayStyleDirectory);
            for (int d = 0; d < displayDirectories.Length; d++)
            {
                string directory = displayDirectories[d];
                Load(directory);
            }
		}

        public static void Load(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            DisplayLoader displayLoader = new DisplayLoader(directory.FullName);
            if (displayLoader.ContainsValidModule)
            {
                LoadedDisplayStyle loadedDisplayStyle = new LoadedDisplayStyle(displayLoader);

                OnDisplayLoaded(loadedDisplayStyle.FriendlyName);

                loadedDisplayStyle.SetGrowlApplicationPath(Application.StartupPath);
                loadedDisplayStyle.SetDisplayStylePath(directory.FullName);
                loadedDisplayStyle.Load();
                currentlyLoadedDisplayStyles.Add(directory.FullName, loadedDisplayStyle);

                Assembly a = Assembly.LoadFrom(displayLoader.SettingsPanelAssemblyLocation);
                object x = a.CreateInstance(displayLoader.SettingsPanelTypeName);

                SettingsPanelBase settingsPanel = x as SettingsPanelBase;
                if (settingsPanel != null)
                {
                    settingsPanels.Add(directory.FullName, settingsPanel);
                    settingsPanel.SetDirectories(directory.FullName, Utility.GetDisplayUserSettingsFolder(directory.Name));
                    loadedDisplayStyle.Display.SettingsCollection = settingsPanel.GetSettings();
                }

                // now that the display has been loaded, add it (and any subdisplays) the the list of available displays
                string[] displays = loadedDisplayStyle.Display.GetListOfAvailableDisplays();
                foreach (string name in displays)
                {
                    Growl.Display display = new Growl.Display(name, loadedDisplayStyle.Display);
                    availableDisplays.Add(name, display);
                }
            }
            else
            {
                // display plugin was not valid
            }
        }

		public static void Unload()
		{
			foreach(LoadedDisplayStyle loadedDisplayStyle in currentlyLoadedDisplayStyles.Values)
			{
				loadedDisplayStyle.Unload();
			}
			if(currentlyLoadedDisplayStyles != null) currentlyLoadedDisplayStyles.Clear();
            if(settingsPanels != null) settingsPanels.Clear();
            if(availableDisplays != null) availableDisplays.Clear();
		}

        public static Dictionary<string, Growl.Display> GetAvailableDisplayStyles()
		{
            return availableDisplays;
		}

        internal static Display FindDisplayStyle(string name)
        {
            if (availableDisplays.ContainsKey(name))
            {
                return availableDisplays[name];
            }
            return null;
        }

        internal static SettingsPanelBase GetSettingsPanel(string directory)
        {
            if (settingsPanels.ContainsKey(directory))
                return settingsPanels[directory];
            else return null;
        }

        private static void OnDisplayLoaded(string displayName)
        {
            if (DisplayLoaded != null)
            {
                DisplayLoaded(displayName);
            }
        }

        public static string AppDisplayStyleDirectory
        {
            get
            {
                return appDisplayStyleDirectory;
            }
        }

        public static string UserDisplayStyleDirectory
        {
            get
            {
                return userDisplayStyleDirectory;
            }
        }
	}
}
