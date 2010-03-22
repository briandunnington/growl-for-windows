using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using Growl.Destinations;

namespace Growl.LegacyDeserializers
{
    class OldForwardDestinationToNewForwardDestinationBinder : SerializationBinder
    {
        private const string OLDFORWARDDESTINATION = "Growl.ForwardDestination, Growl, Version";
        private const string NEWFORWARDDESTINATION = "Growl.Destinations.ForwardDestination, Growl.Destinations, Version";

        private const string OLDPLATFORMHELPER = "Growl.ForwardDestinationPlatformType+ForwardDestinationPlatformTypeSerializationHelper";
        private const string NEWPLATFORMHELPER = "Growl.KnownDestinationPlatformType+KnownDestinationPlatformTypeSerializationHelper";

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.Contains(OLDFORWARDDESTINATION))
                typeName = typeName.Replace(OLDFORWARDDESTINATION, NEWFORWARDDESTINATION);

            if (typeName == OLDPLATFORMHELPER)
                typeName = NEWPLATFORMHELPER;

            Type type = Type.GetType(typeName);

            if (type == null)
                Console.WriteLine("null type");

            return type;
        }
    }
}
