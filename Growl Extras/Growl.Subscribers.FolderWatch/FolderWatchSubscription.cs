using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Growl.CoreLibrary;
using Growl.Connector;
using Growl.Destinations;

namespace Growl.Subscribers.FolderWatch
{
    [Serializable]
    public class FolderWatchSubscription : Subscription
    {
        const string APP_NAME = "Folder Watch";
        const string TYPE_CHANGED = "Changed";
        const string TYPE_CREATED = "Created";
        const string TYPE_DELETED = "Deleted";
        const string TYPE_RENAMED = "Renamed";

        string path;
        bool includedSubfolders;
        [NonSerialized]
        FileSystemWatcher watcher;

        public FolderWatchSubscription(bool enabled)
            : base(APP_NAME, enabled)
        {
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        public bool IncludeSubfolders
        {
            get
            {
                return this.includedSubfolders;
            }
            set
            {
                this.includedSubfolders = value;
            }
        }

        public override string AddressDisplay
        {
            get { return path; }
        }

        public override DestinationBase Clone()
        {
            FolderWatchSubscription clone = new FolderWatchSubscription(this.Enabled);
            clone.Path = this.Path;
            clone.IncludeSubfolders = this.IncludeSubfolders;
            return clone;
        }

        public override System.Drawing.Image GetIcon()
        {
            return FolderWatchHandler.Icon;
        }

        public override void Subscribe()
        {
            Kill();

            Application app = new Application(APP_NAME);
            app.Icon = FolderWatchHandler.Icon;
            NotificationType[] types = new NotificationType[4];
            types[0] = new NotificationType(TYPE_CHANGED);
            types[1] = new NotificationType(TYPE_CREATED);
            types[2] = new NotificationType(TYPE_DELETED);
            types[3] = new NotificationType(TYPE_RENAMED);
            Register(app, types);

            if (this.watcher == null)
            {
                this.watcher = new FileSystemWatcher();
                this.watcher.Changed += new FileSystemEventHandler(watcher_Changed);
                this.watcher.Created += new FileSystemEventHandler(watcher_Created);
                this.watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
                this.watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
            }

            this.watcher.Path = this.Path;
            this.watcher.IncludeSubdirectories = this.IncludeSubfolders;
            this.watcher.EnableRaisingEvents = true;
        }

        public override void Kill()
        {
            if (this.watcher != null)
                this.watcher.EnableRaisingEvents = false;
        }

        public override void Remove()
        {
            Kill();
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string type = TYPE_RENAMED;
            string title = "File Renamed";
            string text = String.Format("'{0}' was renamed to '{1}' in '{2}'", e.OldName, e.Name, this.Path);
            Notify(type, title, text);
        }

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string type = TYPE_DELETED;
            string title = "File Deleted";
            string text = String.Format("'{0}' was deleted from '{1}'", e.Name, this.Path);
            Notify(type, title, text);
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            string type = TYPE_CREATED;
            string title = "File Created";
            string text = String.Format("'{0}' was created in '{1}'", e.Name, this.Path);
            Notify(type, title, text);
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string type = TYPE_CHANGED;
            string title = "File Changed";
            string text = String.Format("'{0}' was changed in '{1}'", e.Name, this.Path);
            Notify(type, title, text);
        }

        void Notify(string type, string title, string text)
        {
            Notification n = new Notification(APP_NAME, type, "", title, text);
            Notify(n);
        }
    }
}
