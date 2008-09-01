using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Growl.DisplayStyle;

namespace Growl.AppBridge
{
    internal sealed class DisplayStyleManager
    {
		private static string displayStyleDirectory = Application.StartupPath + @"\Displays\";
        private static Dictionary<string, LoadedDisplayStyle> currentlyLoadedDisplayStyles = new Dictionary<string, LoadedDisplayStyle>();
        private static Dictionary<string, SettingsPanelBase> settingsPanels = new Dictionary<string, SettingsPanelBase>();

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
				RemoteLoader remoteLoader = (RemoteLoader)appDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
                if (remoteLoader.ContainsValidModule)
                {
                    LoadedDisplayStyle displayStyle = new LoadedDisplayStyle(appDomain, remoteLoader);
                    displayStyle.GrowlApplicationPath = Application.StartupPath;
                    displayStyle.DisplayStylePath = directory.FullName;
                    displayStyle.Load();
                    currentlyLoadedDisplayStyles.Add(directory.FullName, displayStyle);

                    object x = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(remoteLoader.SettingsPanelAssemblyName, remoteLoader.SettingsPanelTypeName);
                    Console.WriteLine(x.GetType());
                    if (x is SettingsPanelBase)
                    {
                        SettingsPanelBase settingsPanel = (SettingsPanelBase)x;
                        settingsPanels.Add(directory.FullName, settingsPanel);
                        settingsPanel.Directory = Utility.GetDisplayUserSettingsFolder(directory.Name);
                        displayStyle.Display.SettingsCollection = settingsPanel.GetSettings();
                    }
                }
                else
                {
                    AppDomain.Unload(appDomain);
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

        public static Dictionary<string, Growl.AppBridge.Display> GetAvailableDisplayStyles()
		{
            Dictionary<string, Growl.AppBridge.Display> displayStyles = new Dictionary<string, Growl.AppBridge.Display>();
            foreach (LoadedDisplayStyle loadedDisplayStyle in currentlyLoadedDisplayStyles.Values)
            {
                string[] displays = loadedDisplayStyle.Display.GetListOfAvailableDisplays();
                foreach (string name in displays)
                {
                    Growl.AppBridge.Display display = new Growl.AppBridge.Display(name, loadedDisplayStyle.Display);
                    displayStyles.Add(name, display);
                }
            }
			return displayStyles;
		}

		internal static LoadedDisplayStyle FindDisplayStyle(string directory)
		{
			if(currentlyLoadedDisplayStyles.ContainsKey(directory))
			{
				LoadedDisplayStyle loadedDisplayStyle = (LoadedDisplayStyle) currentlyLoadedDisplayStyles[directory];
				return loadedDisplayStyle;
			}
			return null;
		}

        internal static SettingsPanelBase GetSettingsPanel(string directory)
        {
            if (settingsPanels.ContainsKey(directory))
                return settingsPanels[directory];
            else return null;
        }
	}
}
