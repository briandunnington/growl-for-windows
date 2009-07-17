using System;
using System.Collections.Generic;
using System.Text;

namespace Growl
{
    public class TwitterForwardDestinationHandler : IForwardDestinationHandler
    {
        #region IForwardDestinationHandler Members

        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(TwitterForwardDestination));
            return list;
        }

        public Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestination fd)
        {
            return new Growl.UI.TwitterForwardInputs();
        }

        public Growl.UI.ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestinationListItem fdli)
        {
            return new Growl.UI.TwitterForwardInputs();
        }

        public List<ForwardDestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem(Properties.Resources.AddComputer_AddTwitter, ForwardDestinationPlatformType.Twitter.Icon, this);
            List<ForwardDestinationListItem> list = new List<ForwardDestinationListItem>();
            list.Add(item);
            return list;
        }

        #endregion
    }
}