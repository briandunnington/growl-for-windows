using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Growl.LegacyDeserializers
{
    public class LegacyDeserializationHelper
    {
        public static LegacyDeserializationHelper OldApplicationsHelper = new LegacyDeserializationHelper(null, new ApplicationsSurrogateSelector());
        public static LegacyDeserializationHelper OldForwardDestinationHelper = new LegacyDeserializationHelper(new OldForwardDestinationToNewForwardDestinationBinder(), new ForwardDestinationSurrogateSelector());
        public static LegacyDeserializationHelper OldSubscriptionHelper = new LegacyDeserializationHelper(new OldForwardDestinationToNewSubscriptionBinder(), new SubscriptionSurrogateSelector());

        SerializationBinder binder;
        ISurrogateSelector surrogate;

        public LegacyDeserializationHelper(SerializationBinder binder, ISurrogateSelector surrogate)
        {
            this.binder = binder;
            this.surrogate = surrogate;


        }

        public SerializationBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        public ISurrogateSelector Surrogate
        {
            get
            {
                return this.surrogate;
            }
        }
    }
}
