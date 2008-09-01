using System;
using System.Collections.Generic;
using System.Text;
using Growl.Framework;
using LitJson;

namespace Growl.GrowlProtocolHandler
{
    internal class JsonConverter
    {
        public static NotificationType[] ToNotificationTypeArray(string json)
        {
            List<NotificationType> list = new List<NotificationType>();
            JsonReader reader = new JsonReader(json);

            while (reader.Read())
            {
                string type = reader.Value != null ? reader.Value.GetType().ToString() : "";
                Console.WriteLine("{0} {1} {2}", reader.Token, reader.Value, type);
            }

            NotificationType[] notificationTypes = list.ToArray();
            return notificationTypes;
        }

        public static NotificationType ToNotificationType(string json)
        {
            return null;
        }
    }
}
