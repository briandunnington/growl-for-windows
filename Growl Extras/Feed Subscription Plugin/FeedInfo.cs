using System;
using System.Collections.Generic;
using System.Text;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    public class FeedInfo
    {
        string title;
        string url;
        string icon;
        string language;
        List<FeedItem> items;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        public string Language
        {
            get
            {
                return this.language;
            }
            set
            {
                this.language = value;
            }
        }

        public List<FeedItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
            }
        }
    }
}
