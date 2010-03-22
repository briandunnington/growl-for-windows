using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Growl.LegacyDeserializers
{
    class SubscriptionSurrogateSelector : SurrogateSelector
    {
        public SubscriptionSurrogateSelector()
        {
            DestinationBaseSerializationSurrogate dbss = new DestinationBaseSerializationSurrogate();

             base.AddSurrogate(typeof(Growl.GNTPSubscription),
               new StreamingContext(StreamingContextStates.All),
               dbss);
        }
    }
}
