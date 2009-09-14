using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrowlExtras.FeedMonitor
{
    public class FeedInfo
    {
        string title;
        string url;
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
