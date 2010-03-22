using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Represents an item in the list of choices displayed to the user when adding a new
    /// subscription.
    /// </summary>
    public class SubscriptionListItem : DestinationListItem
    {
        public SubscriptionListItem(string text, Image image, ISubscriptionHandler ish)
            : base(text, image, ish)
        {
        }
    }
}
