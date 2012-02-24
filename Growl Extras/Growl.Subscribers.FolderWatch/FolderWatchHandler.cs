using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl.Subscribers.FolderWatch
{
    public class FolderWatchHandler : ISubscriptionHandler
    {
        public static System.Drawing.Image Icon = Properties.Resources.folderwatch;

        #region IDestinationHandler Members

        public string Name
        {
            get
            {
                return "Folder Watch";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(FolderWatchSubscription));
            return list;
        }

        public List<DestinationListItem> GetListItems()
        {
            List<DestinationListItem> list = new List<DestinationListItem>();
            SubscriptionListItem item = new SubscriptionListItem("Get notified of changes to local folders", Icon, this);  // TODO: LOCAL: LOCALIZE:
            list.Add(item);
            return list;
        }

        public DestinationSettingsPanel GetSettingsPanel(DestinationListItem dbli)
        {
            return new FolderWatchSettings();
        }

        public DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new FolderWatchSettings();
        }

        #endregion
    }
}
