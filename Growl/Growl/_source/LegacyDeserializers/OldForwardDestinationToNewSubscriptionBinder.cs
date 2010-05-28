using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Growl.LegacyDeserializers
{
    class OldForwardDestinationToNewSubscriptionBinder : SerializationBinder
    {
        private const string OLDFORWARDDESTINATION = "Growl.ForwardDestination, Growl, Version";
        private const string NEWFORWARDDESTINATION = "Growl.Destinations.Subscription, Growl.Destinations, Version";

        private const string OLDSUBSCRIPTION = "Growl.Subscription";
        //private const string NEWSUBSCRIPTION = "Growl.Destinations.Subscription, Growl.Destinations, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string NEWSUBSCRIPTION = "Growl.GNTPSubscription";

        private const string OLDPLATFORMHELPER = "Growl.ForwardDestinationPlatformType+ForwardDestinationPlatformTypeSerializationHelper";
        private const string NEWPLATFORMHELPER = "Growl.KnownDestinationPlatformType+KnownDestinationPlatformTypeSerializationHelper";

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.Contains(OLDFORWARDDESTINATION))
                typeName = typeName.Replace(OLDFORWARDDESTINATION, NEWFORWARDDESTINATION);

            if (typeName == OLDSUBSCRIPTION)
                typeName = NEWSUBSCRIPTION;

            if (typeName == OLDPLATFORMHELPER)
                typeName = NEWPLATFORMHELPER;

            Type type = Type.GetType(typeName);

            //if (type == null)
            //    Console.WriteLine("null type");

            return type;
        }
    }
}