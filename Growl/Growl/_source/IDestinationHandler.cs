using System;
using System.Collections.Generic;
using System.Text;
using Growl.UI;

namespace Growl
{
    public interface IDestinationHandler
    {
        List<Type> Register();
        DestinationSettingsPanel GetSettingsPanel(DestinationBase fd);
        DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli);
        List<DestinationListItem> GetListItems();
    }
}