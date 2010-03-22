using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Represents an item in the list of choices displayed to the user when adding a new
    /// forwarder.
    /// </summary>
    public class ForwardDestinationListItem : DestinationListItem
    {
        public ForwardDestinationListItem(string text, Image image, IForwardDestinationHandler ifdh)
            : base(text, image, ifdh)
        {
        }
    }
}
