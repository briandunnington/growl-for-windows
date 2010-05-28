using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    class NotifyIOSubscriptionHandler : ISubscriptionHandler
    {
        #region ISubscriptionHandler Members

        public string Name
        {
            get
            {
                return "Notify.io";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(NotifyIOSubscription));
            return list;
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new Growl.UI.NotifyIOSubscriptionInputs();
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new Growl.UI.NotifyIOSubscriptionInputs();
        }

        public List<DestinationListItem> GetListItems()
        {
            SubscriptionListItem item = new SubscriptionListItem(Utility.GetResourceString(Properties.Resources.AddSubscription_AddNotifyIO), KnownDestinationPlatformType.NotifyIO.GetIcon(), this);
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}
