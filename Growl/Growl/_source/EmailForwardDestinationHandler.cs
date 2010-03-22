using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Growl
{
    public class EmailForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public string Name
        {
            get
            {
                return "Email";
            }
        }

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(EmailForwardDestination));
            return list;
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationBase fd)
        {
            return new Growl.UI.EmailForwardInputs();
        }

        public Growl.Destinations.DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new Growl.UI.EmailForwardInputs();
        }

        public List<DestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem(Properties.Resources.AddComputer_AddEmail, KnownDestinationPlatformType.Email.GetIcon(), this);
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}