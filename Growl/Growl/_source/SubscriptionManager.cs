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
    internal sealed class SubscriptionManager
    {
        const string SUBSCRIBER_PLUGIN_INFO_SETTINGS_FILENAME = "subscriberPluginInfo.settings";

        private static List<string> ignoreList = new List<string>();
        private static string userSubscriberDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.UserSettingFolder, @"Subscribers" + Path.DirectorySeparatorChar);
        private static string commonSubscriberDirectory = Growl.CoreLibrary.PathUtility.Combine(Utility.CommonPluginFolder, @"Subscribers" + Path.DirectorySeparatorChar);

        private static List<ISubscriptionHandler> loadedHandlersList = new List<ISubscriptionHandler>();
        private static Dictionary<string, PluginInfo> loadedPlugins = new Dictionary<string, PluginInfo>();
        private static List<PluginInfo> loadedPluginsList = new List<PluginInfo>();
        private static Dictionary<Type, ISubscriptionHandler> loadedTypes = new Dictionary<Type, ISubscriptionHandler>();

        private static SettingSaver ssPluginInfo = new SettingSaver(SUBSCRIBER_PLUGIN_INFO_SETTINGS_FILENAME);

        static Type KnownTypeISubscriptionHandler = typeof(ISubscriptionHandler);

        private SubscriptionManager() { }

        static SubscriptionManager()
        {
            ignoreList.Add("growl.destinations.dll");
            ignoreList.Add("growl.connector.dll");
            ignoreList.Add("growl.corelibrary.dll");
        }

        public static string UserPluginDirectory
        {
            get
            {
                return userSubscriberDirectory;
            }
        }

        public static void Load()
        {
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(userSubscriberDirectory);
            Growl.CoreLibrary.PathUtility.EnsureDirectoryExists(commonSubscriberDirectory);

            // built-in plugins
            LoadBuiltIn(new GNTPSubscriptionHandler());   // make sure this is the first item in the list
            LoadBuiltIn(new NotifyIOSubscriptionHandler());
            // -- add additional built-in plugins here

            // user-specific plugins
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

            // kick off an async call to discover new forwarder plugins
            System.Threading.ThreadPool.RegisterWaitForSingleObject(Program.ProgramLoadedResetEvent, new System.Threading.WaitOrTimerCallback(DiscoverNewPlugins), null, -1, true);
        }

        private static void LoadBuiltIn(ISubscriptionHandler sh)
        {
            if (sh != null)
            {
                string path = Path.Combine(userSubscriberDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(sh.Name));
                LoadInternal(sh, null, path);
            }
        }

        private static void LoadFolder(string folder)
        {
            try
            {
                if (!loadedPlugins.ContainsKey(folder))
                {
                    PluginFinder pf = new PluginFinder();
                    ISubscriptionHandler plugin = pf.Search<ISubscriptionHandler>(folder, CheckType, ignoreList);
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

        private static void LoadPlugin(PluginInfo pi, ISubscriptionHandler ish)
        {
            try
            {
                // load if not already loaded
                if (ish == null)
                {
                    PluginFinder pf = new PluginFinder();
                    ish = pf.Load<ISubscriptionHandler>(pi, ignoreList);
                }

                if (ish != null)
                {
                    // for the 'path' value, we still want to use the userprofile directory no matter where the plugin was loaded from
                    // the reason is that the 'path' is where plugin settings will be saved, so it needs to be user writable and user-specific
                    string path = Path.Combine(userSubscriberDirectory, Growl.CoreLibrary.PathUtility.GetSafeFolderName(ish.Name));

                    // check to make sure this plugin was not loaded from another directory already
                    if (!loadedPlugins.ContainsKey(path))
                    {
                        loadedPlugins.Add(pi.FolderPath, pi);
                        if (!loadedPlugins.ContainsKey(path)) loadedPlugins.Add(path, pi); // link by the settings path as well so we can detect duplicate plugins in other folders
                        loadedPluginsList.Add(pi);

                        LoadInternal(ish, pi.FolderPath, path);
                    }
                    else
                    {
                        // plugin was not valid
                        Utility.WriteDebugInfo(String.Format("Subscriber not loaded: '{0}' - Duplicate plugin was already loaded from another folder", pi.FolderPath));
                    }
                }
                else
                {
                    // plugin was not valid
                    Utility.WriteDebugInfo(String.Format("Subscriber not loaded: '{0}' - Does not implement ISubscriptionHandler interface", pi.FolderPath));
                }
            }
            catch (Exception ex)
            {
                // suppress any per-plugin loading exceptions
                Utility.WriteDebugInfo(String.Format("Subscriber failed to load: '{0}' - {1} - {2}", pi.FolderPath, ex.Message, ex.StackTrace));
            }
        }

        private static void LoadInternal(ISubscriptionHandler ish, string installPath, string settingsPath)
        {
            string name = null;
            try
            {
                if (ish != null)
                {
                    name = ish.Name;

                    loadedHandlersList.Add(ish);

                    List<Type> list = ish.Register();
                    foreach (Type type in list)
                    {
                        if (typeof(Subscription).IsAssignableFrom(type))
                        {
                            if (!loadedTypes.ContainsKey(type))
                            {
                                lock (loadedTypes)
                                {
                                    if (!loadedTypes.ContainsKey(type))
                                    {
                                        loadedTypes.Add(type, ish);
                                    }
                                }
                            }
                        }
                    }

                    Utility.WriteDebugInfo(String.Format("Subscriber '{0}' was loaded successfully", name));
                }
            }
            catch (Exception ex)
            {
                // suppress any per-plugin loading exceptions
                Utility.WriteDebugInfo(String.Format("Subscriber failed to load: '{0}' - {1} - {2}", name, ex.Message, ex.StackTrace));
            }
        }

        public static ISubscriptionHandler GetHandler(DestinationBase db)
        {
            Type type = db.GetType();
            return GetHandler(type);
        }

        public static ISubscriptionHandler GetHandler(Type type)
        {
            if (loadedTypes.ContainsKey(type))
            {
                return loadedTypes[type];
            }
            else
                return null;
        }

        public static Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(Subscription s)
        {
            ISubscriptionHandler handler = GetHandler(s);
            Growl.Destinations.DestinationSettingsPanel panel = handler.GetSettingsPanel(s);
            return panel;
        }

        public static List<DestinationListItem> GetListItems()
        {
            List<DestinationListItem> list = new List<DestinationListItem>();
            foreach (ISubscriptionHandler handler in loadedHandlersList)
            {
                list.AddRange(handler.GetListItems());
            }
            return list;
        }

        public static void Update(Dictionary<string, Subscription> subscriptions, bool enabled)
        {
            foreach (Subscription s in subscriptions.Values)
            {
                Update(s, enabled);
            }
        }

        public static void Update(Subscription subscription, bool enabled)
        {
            if (enabled && subscription.Enabled)
                subscription.Subscribe();
            else
                subscription.Kill();
        }

        public static void DiscoverNewPlugins()
        {
            // we want to check both locations
            string[] userFolders = Directory.GetDirectories(userSubscriberDirectory);
            string[] commonFolders = Directory.GetDirectories(commonSubscriberDirectory);
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
                Utility.WriteDebugInfo(String.Format("Exception: SubscriptionManager.DiscoverNewPlugins: {0} - {1}", ex.Message, ex.StackTrace));
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
            if (type != null && type != KnownTypeISubscriptionHandler && !type.IsAbstract && KnownTypeISubscriptionHandler.IsAssignableFrom(type))
                valid = true;
            return valid;
        }
    }
}