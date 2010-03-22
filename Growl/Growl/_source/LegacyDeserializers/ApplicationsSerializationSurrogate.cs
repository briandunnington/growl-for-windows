using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace Growl.LegacyDeserializers
{
    class ApplicationsSerializationSurrogate : ISerializationSurrogate
    {
        private static Dictionary<string, System.Drawing.Image> images = new Dictionary<string,System.Drawing.Image>();

        private static void AddTemporaryNotificationImage(RegisteredNotification rn, System.Drawing.Image image)
        {
            string id = String.Format("TEMP_{0}", System.Guid.NewGuid().ToString());
            images.Add(id, image);
            rn.IconID = id;
        }

        public static void UpdateTemporaryNotificationImage(RegisteredNotification rn)
        {
            if (rn != null && !String.IsNullOrEmpty(rn.IconID) && rn.IconID.StartsWith("TEMP_") && images.ContainsKey(rn.IconID))
            {
                System.Drawing.Image image = images[rn.IconID];
                using(image)
                {
                    images.Remove(rn.IconID);
                    rn.SetIcon(image);
                }
                image = null;
            }
        }

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

            System.Drawing.Image image = null;
            List<object> dataList = new List<object>();
            SerializationInfoEnumerator sie = info.GetEnumerator();
            while (sie.MoveNext())
            {
                object val = sie.Current.Value;
                if (sie.Name == "icon")
                {
                    image = val as System.Drawing.Image;
                    val = null;
                }
                dataList.Add(val);
            }
            object[] data = dataList.ToArray();
            obj = FormatterServices.PopulateObjectMembers(obj, mi, data);

            // handle the icon
            if (image != null)
            {
                RegisteredApplication ra = obj as RegisteredApplication;
                if (ra != null)
                {
                    ra.IconID = ImageCache.Add(ra.Name, image);
                    image.Dispose();
                    image = null;
                }
                else
                {
                    RegisteredNotification rn = obj as RegisteredNotification;
                    if (rn != null)
                    {
                        AddTemporaryNotificationImage(rn, image);
                        // dont dispose of the image yet, it will get taken care of later
                    }
                }
            }

            return obj;
        }

        #endregion
    }
}
