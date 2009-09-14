using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel.Syndication;
using Growl.Connector;
using Growl.CoreLibrary;

namespace GrowlExtras.FeedMonitor
{
    public class MainComponent
    {
        private delegate void SimpleMethodDelegate();

        private MainForm mainForm;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;

        private GrowlConnector growl;
        private Growl.Connector.Application app;
        private NotificationType ntNewFeedItem;
        private NotificationType ntFeedError;
        private List<Feed> feeds;
        private bool loaded = false;

        public MainComponent()
        {
            System.Windows.Forms.Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            this.settingsToolStripMenuItem = new ToolStripMenuItem();
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new EventHandler(settingsToolStripMenuItem_Click);

            this.toolStripSeparator1 = new ToolStripSeparator();
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(129, 6);

            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new EventHandler(exitToolStripMenuItem_Click);

            this.contextMenu = new ContextMenuStrip();
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.settingsToolStripMenuItem,
                this.toolStripSeparator1,
                this.exitToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.ShowImageMargin = false;
            this.contextMenu.Size = new System.Drawing.Size(149, 142);

            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.Icon = global::GrowlExtras.FeedMonitor.Properties.Resources.feed;
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Text = "Feed Monitor";
            this.notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            this.notifyIcon.Visible = true;

            this.mainForm = new MainForm();
            this.mainForm.Load += new EventHandler(mainForm_Load);
            this.mainForm.SetComponent(this);

            InitializeGrowl();

            InitializeFeeds();
        }

        void mainForm_Load(object sender, EventArgs e)
        {
            this.loaded = true;
            foreach (Feed f in this.feeds)
            {
                f.CheckForUpdates();
            }
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            ExitApp();
        }

        private void InitializeGrowl()
        {
            this.app = new Growl.Connector.Application("Feed Monitor");
            this.app.Icon = new BinaryData(ImageConverter.ImageToBytes(global::GrowlExtras.FeedMonitor.Properties.Resources.feed.ToBitmap()));

            this.ntNewFeedItem = new NotificationType("New Feed Item", "New Feed Item");
            this.ntFeedError = new NotificationType("Feed Error", "Feed Error");

            this.growl = new GrowlConnector();
            this.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;

            this.growl.Register(this.app, new NotificationType[] { this.ntNewFeedItem, this.ntFeedError });
        }

        private void InitializeFeeds()
        {
            this.feeds = new List<Feed>();

            List<Feed> savedFeeds = SettingsPersister.Read();
            foreach (Feed feed in savedFeeds)
            {
                AddFeed(feed.Url, feed.PollInterval);
            }

            if (this.feeds.Count == 0)
            {
                //AddFeed("http://rss.news.yahoo.com/rss/topstories", Properties.Settings.Default.DefaultInterval);
                AddFeed("http://feedproxy.google.com/TechCrunch", Properties.Settings.Default.DefaultInterval);
                AddFeed("http://feeds2.feedburner.com/readwriteweb", Properties.Settings.Default.DefaultInterval);
                AddFeed("http://groups.google.com/group/growl-for-windows/feed/atom_v1_0_msgs.xml", Properties.Settings.Default.DefaultInterval);
            }
        }

        public void AddFeed(string url, int pollInterval)
        {
            try
            {
                Feed feed = Feed.Create(url, pollInterval);
                feed.FeedRetrieved += new EventHandler<FeedRetrievedEventArgs>(feed_FeedRetrieved);
                feed.FeedUpdated += new EventHandler<FeedUpdatedEventArgs>(feed_FeedUpdated);
                feed.FeedError += new EventHandler<FeedErrorEventArgs>(feed_FeedError);
                this.feeds.Add(feed);
                feed.CheckForUpdates();
            }
            catch
            {
                // do nothing
            }
        }

        public void RemoveFeed(Feed feed)
        {
            feed.FeedRetrieved -= feed_FeedRetrieved;
            feed.FeedUpdated -= feed_FeedUpdated;
            this.feeds.Remove(feed);
            feed.Dispose();
            feed = null;
        }

        public List<Feed> Feeds
        {
            get
            {
                return this.feeds;
            }
        }

        void feed_FeedError(object sender, FeedErrorEventArgs e)
        {
            Feed feed = (Feed)sender;
            string name = (feed.Name == Feed.LOADING ? feed.Url : feed.Name);
            string text = String.Format("The feed '{0}' was not able to be parsed.", name);
            Notification n = new Notification(this.app.Name, this.ntFeedError.Name, "", "Feed Error", text);
            this.growl.Notify(n);
        }

        void feed_FeedRetrieved(object sender, FeedRetrievedEventArgs e)
        {
            if (this.loaded)
            {
                SimpleMethodDelegate del = new SimpleMethodDelegate(this.mainForm.Refresh);
                this.mainForm.Invoke(del);
            }
        }

        void feed_FeedUpdated(object sender, FeedUpdatedEventArgs e)
        {
            if (e != null && e.NewItems != null)
            {
                foreach (FeedItem item in e.NewItems)
                {
                    //this.notifyIcon.ShowBalloonTip(1000, item.SourceFeed.Title.Text, item.Title.Text, ToolTipIcon.Info);

                    Notification n = new Notification(this.app.Name, this.ntNewFeedItem.Name, String.Empty, item.SourceFeed.Title, item.Title);

                    CallbackContext c = null;
                    if (!String.IsNullOrEmpty(item.Link))
                    {
                        c = new CallbackContext(item.Link);
                    }

                    this.growl.Notify(n, c);
                }
            }
        }

        private void ExitApp()
        {
            this.notifyIcon.Dispose();
            SettingsPersister.Persist(this.Feeds);
        }

        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void ShowForm()
        {
            this.mainForm.Show();
            this.mainForm.Activate();
        }
    }
}
