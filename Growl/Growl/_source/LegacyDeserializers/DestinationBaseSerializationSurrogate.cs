using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace Growl.LegacyDeserializers
{
    class DestinationBaseSerializationSurrogate : ISerializationSurrogate
    {
        #region ISerializationSurrogate Members

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            // do nothing
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            // Get the set of serializable members for our class and base classes
            Type thisType = obj.GetType();
            MemberInfo[] mi = FormatterServices.GetSerializableMembers(thisType, context);

            List<object> dataList = new List<object>();
            SerializationInfoEnumerator sie = info.GetEnumerator();
            while (sie.MoveNext())
            {
                dataList.Add(sie.Current.Value);
            }
            object[] data = dataList.ToArray();
            obj = FormatterServices.PopulateObjectMembers(obj, mi, data);

            /*
            // Deserialize fields from the info object
            for (Int32 i = 0; i < mi.Length; i++)
            {
                // To ease coding, treat the member as a FieldInfo object
                FieldInfo fi = (FieldInfo)mi[i];

                string name = fi.Name;
                if (name.StartsWith("DestinationBase+")) name = name.Replace("DestinationBase+", "ForwardDestination+");
                if (name.StartsWith("GNTPSubscription+")) name = name.Replace("GNTPSubscription+", "");

                object val = info.GetValue(name, fi.FieldType);

                // Set the field to the deserialized value
                fi.SetValue(obj, val);

                
            }
             * */

            

            return obj;
        }

        #endregion
    }
}
