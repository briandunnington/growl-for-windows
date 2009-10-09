using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace GrowlExtras.FeedMonitor
{
    internal static class SettingsPersister
    {
        private const string FEED_SETTINGS_SECTION_NAME = "feeds";

        private static string configPath;

        static SettingsPersister()
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(a.Location);

            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = String.Format(@"Growl Extras\Feed Monitor\{0}", a.GetName().Version.ToString());
            string configFolder = System.IO.Path.Combine(root, folder);
            if (!configFolder.EndsWith(@"\")) configFolder += @"\";
            if (!System.IO.Directory.Exists(configFolder)) System.IO.Directory.CreateDirectory(configFolder);
            configPath = System.IO.Path.Combine(configFolder, "user.config");
        }

        public static void Persist(List<Feed> feeds)
        {
            // save feed info
            try
            {
                //Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                Configuration configuration = OpenConfig();
                FeedsSection section = (FeedsSection) configuration.GetSection(FEED_SETTINGS_SECTION_NAME);
                if (section == null)
                {
                    section = new FeedsSection();
                    section.LockItem = false;
                    section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                    section.SectionInformation.ForceSave = true;
                    configuration.Sections.Add(FEED_SETTINGS_SECTION_NAME, section);
                }
                section.Feeds.Clear();

                foreach (Feed feed in feeds)
                {
                    FeedElement fe = new FeedElement();
                    fe.Name = feed.ActualName;
                    fe.Url = feed.Url;
                    fe.PollInterval = feed.PollInterval;
                    fe.CustomName = feed.CustomName;
                    fe.Username = feed.Username;
                    fe.Password = feed.Password;
                    section.Feeds.Add(fe);
                }

                configuration.Save();
            }
            catch
            {
            }
        }

        public static List<Feed> Read()
        {
            // read feed info
            List<Feed> feeds = new List<Feed>();
            try
            {
                //Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                Configuration configuration = OpenConfig();
                FeedsSection section = (FeedsSection)configuration.GetSection(FEED_SETTINGS_SECTION_NAME);
                if (section != null)
                {
                    foreach (FeedElement fe in section.Feeds)
                    {
                        Feed feed = Feed.Create(fe.Url, fe.PollInterval, fe.CustomName, fe.Username, fe.Password);
                        feeds.Add(feed);
                    }
                }
            }
            catch
            {
            }
            return feeds;
        }

        private static Configuration OpenConfig()
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = configPath;
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        private class FeedsSection : ConfigurationSection
        {
            private static ConfigurationPropertyCollection s_properties;
            private static ConfigurationProperty s_propFeeds;

            public FeedsSection()
            {
                EnsureStaticPropertyBag();
            }

            [ConfigurationProperty("", IsDefaultCollection=true)]
            public FeedsElement Feeds
            {
                get
                {
                    return (FeedsElement)base[s_propFeeds];
                }
            }

            private static ConfigurationPropertyCollection EnsureStaticPropertyBag()
            {
                if (s_properties == null)
                {
                    s_propFeeds = new ConfigurationProperty(null, typeof(FeedsElement), null, ConfigurationPropertyOptions.IsDefaultCollection);
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(s_propFeeds);
                    s_properties = propertys;
                }
                return s_properties;
            }
        }

        private class FeedsElement : ConfigurationElementCollection
        {
            protected override ConfigurationElement CreateNewElement()
            {
                return new FeedElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((FeedElement)element).Url;
            }

            public void Add(FeedElement feed)
            {
                this.BaseAdd(feed);
            }

            public void Clear()
            {
                this.BaseClear();
            }
        }


        private class FeedElement : ConfigurationElement
        {
            public FeedElement()
            {
            }

            [ConfigurationProperty("name")]
            public string Name
            {
                get
                {
                    return (string)this["name"];
                }
                set
                {
                    this["name"] = value;
                }
            }

            [ConfigurationProperty("url")]
            public string Url
            {
                get
                {
                    return (string)this["url"];
                }
                set
                {
                    this["url"] = value;
                }
            }

            [ConfigurationProperty("pollinterval")]
            public int PollInterval
            {
                get
                {
                    return (int)this["pollinterval"];
                }
                set
                {
                    this["pollinterval"] = value;
                }
            }

            [ConfigurationProperty("customname", IsRequired = false)]
            public string CustomName
            {
                get
                {
                    return (string)this["customname"];
                }
                set
                {
                    this["customname"] = value;
                }
            }

            [ConfigurationProperty("username", IsRequired = false)]
            public string Username
            {
                get
                {
                    return (string)this["username"];
                }
                set
                {
                    this["username"] = value;
                }
            }

            [ConfigurationProperty("password", IsRequired = false)]
            public string Password
            {
                get
                {
                    return (string)this["password"];
                }
                set
                {
                    this["password"] = value;
                }
            }
        }
    }
}
