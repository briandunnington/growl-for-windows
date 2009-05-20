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

        private static string displayStyleDirectory = Growl.CoreLibrary.PathUtility.Combine(Application.StartupPath, @"Displays\");
        private static Dictionary<string, LoadedDisplayStyle> currentlyLoadedDisplayStyles = new Dictionary<string, LoadedDisplayStyle>();
        private static Dictionary<string, SettingsPanelBase> settingsPanels = new Dictionary<string, SettingsPanelBase>();
        private static Dictionary<string, Display> availableDisplays;

		private DisplayStyleManager() {}

		public static void Load()
		{
            // always Unload first
            Unload();

            string[] displayDirectories = Directory.GetDirectories(displayStyleDirectory);
			for(int d=0;d<displayDirectories.Length;d++)
			{
				DirectoryInfo directory = new DirectoryInfo(displayDirectories[d]);
                AppDomain.CurrentDomain.AppendPrivatePath(@"Displays\" + directory.Name);

				AppDomainSetup setup = new AppDomainSetup();
				setup.ApplicationName = directory.Name;
				setup.ApplicationBase = directory.FullName;
                setup.PrivateBinPath = directory.FullName;
				setup.ConfigurationFile = String.Format("{0}\\app.config", setup.ApplicationBase);

				AppDomain appDomain = AppDomain.CreateDomain(setup.ApplicationName, null, setup);
				string assemblyName = Assembly.GetAssembly(typeof(RemoteLoader)).FullName;
				string typeName = typeof(RemoteLoader).FullName;

                RemoteLoader remoteLoader = null;
                try
                {
                    remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Display '{0}' could not be loaded - {1}", directory.Name, ex.Message);
                    continue;
                }
                if (remoteLoader.ContainsValidModule)
                {
                    LoadedDisplayStyle displayStyle = new LoadedDisplayStyle(appDomain, remoteLoader);

                    OnDisplayLoaded(displayStyle.FriendlyName);

                    displayStyle.SetGrowlApplicationPath(Application.StartupPath);
                    displayStyle.SetDisplayStylePath(directory.FullName);
                    displayStyle.Load();
                    currentlyLoadedDisplayStyles.Add(directory.FullName, displayStyle);

                    object x = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(remoteLoader.SettingsPanelAssemblyName, remoteLoader.SettingsPanelTypeName);
                    SettingsPanelBase settingsPanel = x as SettingsPanelBase;
                    if (settingsPanel != null)
                    {
                        settingsPanels.Add(directory.FullName, settingsPanel);
                        settingsPanel.SetDirectories(directory.FullName, Utility.GetDisplayUserSettingsFolder(directory.Name));
                        displayStyle.Display.SettingsCollection = settingsPanel.GetSettings();
                    }
                }
                else
                {
                    AppDomain.Unload(appDomain);
                }
			}

            // now that all displays have been read, load up the wrapper objects
            availableDisplays = new Dictionary<string, Display>();
            foreach (LoadedDisplayStyle loadedDisplayStyle in currentlyLoadedDisplayStyles.Values)
            {
                string[] displays = loadedDisplayStyle.Display.GetListOfAvailableDisplays();
                foreach (string name in displays)
                {
                    Growl.Display display = new Growl.Display(name, loadedDisplayStyle.Display);
                    availableDisplays.Add(name, display);
                }
            }
		}

		public static void Unload()
		{
			foreach(LoadedDisplayStyle loadedDisplayStyle in currentlyLoadedDisplayStyles.Values)
			{
				loadedDisplayStyle.Unload();
				AppDomain.Unload(loadedDisplayStyle.AppDomain);
			}
			currentlyLoadedDisplayStyles.Clear();
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
	}
}
