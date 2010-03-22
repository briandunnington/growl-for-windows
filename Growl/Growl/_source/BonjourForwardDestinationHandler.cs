using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    public class BonjourForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public string Name
        {
            get
            {
                return "Bonjour Forwarder";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(BonjourForwardDestination));
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
            List<DestinationListItem> list = new List<DestinationListItem>();
            Controller controller = Controller.GetController();
            if (controller != null)
            {
                Dictionary<string, DetectedService> availableServices = controller.DetectedServices;
                foreach (DetectedService ds in availableServices.Values)
                {
                    BonjourListItem bli = new BonjourListItem(ds, this);
                    list.Add(bli);
                }
            }
            return list;
        }

        #endregion
    }
}
