using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    public class ManualForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public string Name
        {
            get
            {
                return "Manual Forwarder";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(GNTPForwardDestination));
            list.Add(typeof(UDPForwardDestination));
            //list.Add(typeof(SubscribedForwardDestination));
            return list;
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationBase fd)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new Growl.UI.ForwardDestinationInputs();
        }

        public List<DestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem(Properties.Resources.AddComputer_ManualAdd, KnownDestinationPlatformType.Other.GetIcon(), this);
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}
