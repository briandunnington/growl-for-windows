using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class ManualForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(GNTPForwardDestination));
            list.Add(typeof(UDPForwardDestination));
            list.Add(typeof(SubscribedForwardDestination));
            list.Add(typeof(Subscription));
            return list;
        }

        public Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestination fd)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestinationListItem fdli)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public List<ForwardDestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem(Properties.Resources.AddComputer_ManualAdd, ForwardDestinationPlatformType.Other.Icon, this);
            List<ForwardDestinationListItem> list = new List<ForwardDestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}
