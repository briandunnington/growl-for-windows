using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Growl.LegacyDeserializers
{
    class ForwardDestinationSurrogateSelector : SurrogateSelector
    {
        public ForwardDestinationSurrogateSelector()
        {
            DestinationBaseSerializationSurrogate dbss = new DestinationBaseSerializationSurrogate();

            base.AddSurrogate(typeof(Growl.GNTPForwardDestination),
               new StreamingContext(StreamingContextStates.All),
               dbss);

            base.AddSurrogate(typeof(Growl.BonjourForwardDestination),
               new StreamingContext(StreamingContextStates.All),
               dbss);

            base.AddSurrogate(typeof(Growl.ProwlForwardDestination),
               new StreamingContext(StreamingContextStates.All),
               dbss);

            base.AddSurrogate(typeof(Growl.TwitterForwardDestination),
               new StreamingContext(StreamingContextStates.All),
               dbss);

            base.AddSurrogate(typeof(Growl.EmailForwardDestination),
               new StreamingContext(StreamingContextStates.All),
               dbss);

            base.AddSurrogate(typeof(Growl.UDPForwardDestination),
                new StreamingContext(StreamingContextStates.All),
                dbss);
        }
    }
}
