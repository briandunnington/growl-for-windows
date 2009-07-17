using System;
using System.Collections.Generic;
using System.Text;
using Growl.UI;

namespace Growl
{
    public interface IForwardDestinationHandler
    {
        List<Type> Register();
        ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestination fd);
        ForwardDestinationSettingsPanel GetSettingsPanel(ForwardDestinationListItem fdli);
        List<ForwardDestinationListItem> GetListItems();
    }
}
