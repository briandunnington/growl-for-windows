using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class BonjourForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(BonjourForwardDestination));
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
            List<ForwardDestinationListItem> list = new List<ForwardDestinationListItem>();
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
