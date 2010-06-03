using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Growl.Destinations;


namespace Growl
{
    internal sealed class ForwardDestinationManager
    {
        const string FORWARDER_PLUGIN_INFO_SETTINGS_FILENAME = "forwarderPluginInfo.settings";

        private static List<string> ignoreList = new List<string>();
        private static string userForwarderDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"Forwarders" + Path.DirectorySeparatorChar);
        private static string commonForwarderDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.CommonPluginFolder, @"Forwarders" + Path.DirectorySeparatorChar);

        private static List<IForwardDestinationHandler> loadedHandlersList = new List<IForwardDestinationHandler>();
        private static Dictionary<string, PluginInfo> loadedPlugins = new Dictionary<string, PluginInfo>();
        private static List<PluginInfo> loadedPluginsList = new List<PluginInfo>();
        private static Dictionary<Type, IForwardDestinationHandler> loadedTypes = new Dictionary<Type, IForwardDestinationHandler>();

        private static SettingSaver ssPluginInfo = new SettingSaver(FORWARDER_PLUGIN_INFO_SETTINGS_FILENAME);

        static Type KnownTypeIForwardDestinationHandler = typeof(IForwardDestinationHandler);

		private ForwardDestinationManager() {}

        static ForwardDestinationManager()
        {
            ignoreList.Add("growl.destinations.dll");
            ignoreList.Add("growl.connector.dll");
            ignoreList.Add("growl.corelibrary.dll");
        }

        public static string UserPluginDirectory
        {
            get
            {
                return userForwarderDirectory;
            }
        }

		public static void Load()
		{
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(userForwarderDirectory);
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(commonForwarderDirectory);

            // built-in displays
            LoadBuiltIn(new BonjourForwardDestinationHandler());   // make sure this is the first item in the list
            LoadBuiltIn(new ManualForwardDestinationHandler());
            LoadBuiltIn(new ProwlForwardDestinationHandler());
            LoadBuiltIn(new TwitterForwardDestinationHandler());
            LoadBuiltIn(new EmailForwardDestinationHandler());
            // -- add additional built-in display styles here

            // user-specific plugins
            List<PluginInfo> pis = (List<PluginInfo>) ssPluginInfo.Load();
            if (pis != null)
            {
                foreach (PluginInfo pi in pis)
                {
                    LoadPlugin(pi, null);
                }
                pis.Clear();
                pis = null;
            }

            // kick off an async call to discover new forwarder plugins
            System.Threading.ThreadPool.RegisterWaitForSingleObject(Program.ProgramLoadedResetEvent, new System.Threading.WaitOrTimerCallback(DiscoverNewPlugins), null, -1, true);
		}

        private static void LoadBuiltIn(IForwardDestinationHandler fdh)
        {
            if (fdh != null)
            {
                string path = Path.Combine(userForwarderDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(fdh.Name));
                LoadInternal(fdh, null, path);
            }
        }

        private static void LoadFolder(string folder)
        {
            try
            {
                if (!loadedPlugins.ContainsKey(folder))
                {
                    PluginFinder pf = new PluginFinder();
                    IForwardDestinationHandler plugin = pf.Search<IForwardDestinationHandler>(folder, CheckType, ignoreList);
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
                Utility.WriteDebugInfo(String.Format("Plugin failed to load: '{0}' - {1} - {2}", folder, ex.Message, ex.StackTrace));
            }
        }

        private static void LoadPlugin(PluginInfo pi, IForwardDestinationHandler ifdh)
        {
            try
            {
                // load if not already loaded
                if (ifdh == null)
                {
                    PluginFinder pf = new PluginFinder();
                    ifdh = pf.Load<IForwardDestinationHandler>(pi, ignoreList);
                }

                if (ifdh != null)
                {
                    // for the 'path' value, we still want to use the userprofile directory no matter where the plugin was loaded from
                    // the reason is that the 'path' is where plugin settings will be saved, so it needs to be user writable and user-specific
                    string path = Path.Combine(userForwarderDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(ifdh.Name));

                    // check to make sure this plugin was not loaded from another directory already
                    if (!loadedPlugins.ContainsKey(path))
                    {
                        loadedPlugins.Add(pi.FolderPath, pi);
                        if (!loadedPlugins.ContainsKey(path)) loadedPlugins.Add(path, pi); // link by the settings path as well so we can detect duplicate plugins in other folders
                        loadedPluginsList.Add(pi);

                        LoadInternal(ifdh, pi.FolderPath, path);
                    }
                    else
                    {
                        // plugin was not valid
                        Utility.WriteDebugInfo(String.Format("Forwarder not loaded: '{0}' - Duplicate plugin was already loaded from another folder", pi.FolderPath));
                    }
                }
                else
                {
                    // plugin was not valid
                    Utility.WriteDebugInfo(String.Format("Forwarder not loaded: '{0}' - Does not implement IForwardDestinationHandler interface", pi.FolderPath));
                }
            }
            catch (Exception ex)
            {
                // suppress any per-plugin loading exceptions
                Utility.WriteDebugInfo(String.Format("Forwarder failed to load: '{0}' - {1} - {2}", pi.FolderPath, ex.Message, ex.StackTrace));
            }
        }

        private static void LoadInternal(IForwardDestinationHandler fdh, string installPath, string settingsPath)
        {
            string name = null;
            try
            {
                if (fdh != null)
                {
                    name = fdh.Name;
                    loadedHandlersList.Add(fdh);

                    List<Type> list = fdh.Register();
                    foreach (Type type in list)
                    {
                        if (typeof(ForwardDestination).IsAssignableFrom(type))
                        {
                            if (!loadedTypes.ContainsKey(type))
                            {
                                lock (loadedTypes)
                                {
                                    if (!loadedTypes.ContainsKey(type))
                                    {
                                        loadedTypes.Add(type, fdh);
                                    }
                                }
                            }
                        }
                    }

                    Utility.WriteDebugInfo(String.Format("Forwarder '{0}' was loaded successfully", name));
                }
            }
            catch (Exception ex)
            {
                // suppress any per-plugin loading exceptions
                Utility.WriteDebugInfo(String.Format("Forwarder failed to load: '{0}' - {1} - {2}", name, ex.Message, ex.StackTrace));
            }
        }

        public static IForwardDestinationHandler GetHandler(DestinationBase db)
        {
            Type type = db.GetType();
            return GetHandler(type);
        }

        public static IForwardDestinationHandler GetHandler(Type type)
        {
            if (loadedTypes.ContainsKey(type))
            {
                return loadedTypes[type];
            }
            else
                return null;
        }

        public static Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(Growl.Destinations.ForwardDestination fd)
        {
            IForwardDestinationHandler handler = GetHandler(fd);
            Growl.Destinations.DestinationSettingsPanel panel = handler.GetSettingsPanel(fd);
            return panel;
        }

        public static List<DestinationListItem> GetListItems()
        {
            List<DestinationListItem> list = new List<DestinationListItem>();
            foreach (IForwardDestinationHandler handler in loadedHandlersList)
            {
                list.AddRange(handler.GetListItems());
            }
            return list;
        }

        public static void DiscoverNewPlugins()
        {
            // we want to check both locations
            string[] userFolders = Directory.GetDirectories(userForwarderDirectory);
            string[] commonFolders = Directory.GetDirectories(commonForwarderDirectory);
            string[] folders = new string[userFolders.Length + commonFolders.Length];
            userFolders.CopyTo(folders, 0);
            commonFolders.CopyTo(folders, userFolders.Length);

            foreach (string folder in folders)
            {
                LoadFolder(folder);
            }

            ssPluginInfo.Save(loadedPluginsList);
        }

        private static void DiscoverNewPlugins(object state, bool timedOut)
        {
            try
            {
                DiscoverNewPlugins();
            }
            catch (Exception ex)
            {
                Utility.WriteDebugInfo(String.Format("Exception: ForwardDestinationManager.DiscoverNewPlugins: {0} - {1}", ex.Message, ex.StackTrace));
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
            if (type != null && type != KnownTypeIForwardDestinationHandler && !type.IsAbstract && KnownTypeIForwardDestinationHandler.IsAssignableFrom(type))
                valid = true;
            return valid;
        }
	}
}