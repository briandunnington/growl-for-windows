using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    public class GNTPSubscriptionHandler : ISubscriptionHandler
    {
        #region ISubscriptionHandler Members

        public string Name
        {
            get
            {
                return "GNTP Subscription";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(GNTPSubscription));
            return list;
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public List<DestinationListItem> GetListItems()
        {
            SubscriptionListItem item = new SubscriptionListItem(Growl.Properties.Resources.AddSubscription_AddGNTP, KnownDestinationPlatformType.Other.GetIcon(), this);
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}
