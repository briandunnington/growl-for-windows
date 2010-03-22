using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Growl.LegacyDeserializers
{
    class ApplicationsSurrogateSelector : SurrogateSelector
    {
        public ApplicationsSurrogateSelector()
        {
            ApplicationsSerializationSurrogate ass = new ApplicationsSerializationSurrogate();

             base.AddSurrogate(typeof(Growl.RegisteredApplication),
               new StreamingContext(StreamingContextStates.All),
               ass);

             base.AddSurrogate(typeof(Growl.RegisteredNotification),
               new StreamingContext(StreamingContextStates.All),
               ass);
        }
    }
}
