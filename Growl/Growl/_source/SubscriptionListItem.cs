using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Growl
{
    public class SubscriptionListItem : DestinationListItem
    {
        public SubscriptionListItem(string text, Image image, ISubscriptionHandler ish)
            : base(text, image, ish)
        {
        }
    }
}
