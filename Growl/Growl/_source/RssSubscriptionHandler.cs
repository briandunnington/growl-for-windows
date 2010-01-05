using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class RssSubscriptionHandler : ISubscriptionHandler
    {
        #region IForwardDestinationHandler Members

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(RssSubscription));
            return list;
        }

        public Growl.UI.DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new Growl.UI.RssSubscriptionInputs();
        }

        public Growl.UI.DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new Growl.UI.RssSubscriptionInputs();
        }

        public List<DestinationListItem> GetListItems()
        {
            SubscriptionListItem item = new SubscriptionListItem("Subscribe to RSS or Atom feed", ForwardDestinationPlatformType.Email.Icon, this); // TODO: LOCAL: LOCALIZE:
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}
