using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace GrowlExtras.Subscribers.PhonyBalloony
{
    public class PhonyBalloonyHandler : ISubscriptionHandler
    {
        public static PhonyBalloonySubscription Singleton = null;

        public static System.Drawing.Image Icon = Properties.Resources.windows;

        #region IDestinationHandler Members

        public string Name
        {
            get
            {
                return "Windows Notifications";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(PhonyBalloonySubscription));
            return list;
        }

        public List<DestinationListItem> GetListItems()
        {
            List<DestinationListItem> list = new List<DestinationListItem>();
            if (Singleton == null)
            {
                SubscriptionListItem item = new SubscriptionListItem("Route Windows system balloons\nthrough Growl", Icon, this);  // TODO: LOCAL: LOCALIZE:
                list.Add(item);
            }
            return list;
        }

        public DestinationSettingsPanel GetSettingsPanel(DestinationListItem dbli)
        {
            return new PhonyBalloonySettings();
        }

        public DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new PhonyBalloonySettings();
        }

        #endregion
    }
}
