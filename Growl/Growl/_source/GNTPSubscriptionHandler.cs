using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class GNTPSubscriptionHandler : ISubscriptionHandler
    {
        #region ISubscriptionHandler Members

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(GNTPSubscription));
            return list;
        }

        public Growl.UI.DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public Growl.UI.DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public List<DestinationListItem> GetListItems()
        {
            SubscriptionListItem item = new SubscriptionListItem(@"Subscribe to Growl notifications on another machine", ForwardDestinationPlatformType.Other.Icon, this);  // TODO: LOCAL: LOCALIZE:
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}
