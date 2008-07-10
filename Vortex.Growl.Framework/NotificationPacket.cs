using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Vortex.Growl.Framework
{
    /// <summary>
    /// Represents a Notification message sent as a UDP packet as specified by the Growl protocol
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class NotificationPacket : BasePacket
    {
        /// <summary>
        /// The <see cref="NotificationType"/> of the notification
        /// </summary>
        protected NotificationType notificationType;
        /// <summary>
        /// The title of the notification
        /// </summary>
        protected string title;
        /// <summary>
        /// The description or extended information of the notification
        /// </summary>
        protected string description;
        /// <summary>
        /// The <see cref="Priority"/> of the notification
        /// </summary>
        protected Priority priority;
        /// <summary>
        /// Indicates if the notification is sticky or not
        /// </summary>
        protected bool sticky;

        /// <summary>
        /// Creates a new <see cref="NotificationPacket"/>
        /// </summary>
        /// <param name="protocolVersion">The Growl protocol version</param>
        /// <param name="applicationName">The name of the sending application</param>
        /// <param name="password">The password used to validate the receiving client</param>
        /// <param name="notificationType">The <see cref="NotificationType"/> of the notification</param>
        /// <param name="title">The title of the notification</param>
        /// <param name="description">The description or extended information of the notification</param>
        /// <param name="priority">The <see cref="Priority"/> of the notification</param>
        /// <param name="sticky">Indicates if the notification should be sticky or not</param>
        public NotificationPacket(int protocolVersion, string applicationName, string password, NotificationType notificationType, string title, string description, Priority priority, bool sticky)
        {
            this.packetType = PacketType.Notification;
            this.protocolVersion = protocolVersion;
            this.applicationName = applicationName;
            this.password = password;
            this.notificationType = notificationType;
            this.title = title;
            this.description = description;
            this.priority = priority;
            this.sticky = sticky;
            this.data = PrepareData();
        }

        /// <summary>
        /// The <see cref="NotificationType"/> of the notification
        /// </summary>
        /// <remarks>
        /// The <see cref="NotificationType"/> must already have been registered with the application
        /// or the notification will be discarded.
        /// </remarks>
        public NotificationType NotificationType
        {
            get
            {
                return this.notificationType;
            }
        }

        /// <summary>
        /// The title of the notification
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }
        }

        /// <summary>
        /// The description or extended information of the notification
        /// </summary>
        public string Description
        {
            get
            {
                return this.description;
            }
        }

        /// <summary>
        /// The <see cref="Priority"/> of the notification
        /// </summary>
        /// <remarks>
        /// Although the sending application can specify the priority of the notification,
        /// the end user may override the priority setting. As such, the priority specified
        /// here is not guaranteed.
        /// </remarks>
        public Priority Priority
        {
            get
            {
                return this.priority;
            }
        }

        /// <summary>
        /// Indicates if the notification is sticky or not
        /// </summary>
        /// <value><c>true</c> if the notification should be sticky; <c>false</c> otherwise</value>
        /// <remarks>
        /// Although the sending application can specify the stickiness of the notification,
        /// the end user may override the stickiness setting. As such, the stickiness specified
        /// here is not guaranteed.
        /// </remarks>
        public bool Sticky
        {
            get
            {
                return this.sticky;
            }
        }

        /// <summary>
        /// Converts the notification information into a packet of data to be sent 
        /// to the Growl receiving application.
        /// </summary>
        /// <returns>byte array</returns>
        private byte[] PrepareData()
        {
            //TODO: add code to check/handle different versions

            int flags = ConvertPriorityToFlag(this.priority);
            if (this.sticky) flags = flags | 1;

            ByteBuilder bb = new ByteBuilder();
            bb.Append((byte)this.protocolVersion);
            bb.Append((byte)this.packetType);
            bb.Append(flags);
            bb.Append(this.notificationType.Name.Length);
            bb.Append(this.title.Length);
            bb.Append(this.description.Length);
            bb.Append(this.applicationName.Length);
            bb.Append(this.notificationType.Name);
            bb.Append(this.title);
            bb.Append(this.description);
            bb.Append(this.applicationName);

            // handle the password
            ByteBuilder pb = new ByteBuilder();
            pb.Append(this.password);
            ByteBuilder bpb = new ByteBuilder();
            bpb.Append(bb.GetBytes());
            bpb.Append(pb.GetBytes());

            byte[] checksum = Utility.MD5(bpb.GetBytes());
            ByteBuilder fb = new ByteBuilder();
            fb.Append(bb.GetBytes());
            fb.Append(checksum);
            byte[] data = fb.GetBytes();

            return data;
        }

        /// <summary>
        /// Given the raw packet data, converts the data back into a <see cref="NotificationPacket"/>
        /// </summary>
        /// <param name="bytes">The raw packet data</param>
        /// <param name="password">The client password</param>
        /// <returns><see cref="NotificationPacket"/></returns>
        /// <remarks>
        /// If the client password does not match the password used to validate the sent notification,
        /// or if the packet is malformed in any way, a <c>null</c> object will be returned.
        /// </remarks>
        public static NotificationPacket FromPacket(byte[] bytes, string password)
        {
            NotificationPacket np = null;

            // parse the packet
            if (bytes != null && bytes.Length > 18)
            {
                // check md5 hash first
                bool valid = BasePacket.IsPasswordValid(bytes, password);
                if (!valid)
                    return np;

                int protocolVersion = (int)bytes[0];    //TODO: add code to check/handle different versions
                PacketType packetType = (PacketType)bytes[1];

                if (packetType == PacketType.Notification)
                {
                    short flags = BitConverter.ToInt16(new byte[] { bytes[3], bytes[2] }, 0);
                    bool sticky = ((flags & 1) == 1 ? true : false);
                    Priority priority = ConvertFlagToPriority(flags);

                    short notificationNameLength = BitConverter.ToInt16(new byte[] { bytes[5], bytes[4] }, 0);
                    short titleLength = BitConverter.ToInt16(new byte[] { bytes[7], bytes[6] }, 0);
                    short descriptionLength = BitConverter.ToInt16(new byte[] { bytes[9], bytes[8] }, 0);
                    short applicationNameLength = BitConverter.ToInt16(new byte[] { bytes[11], bytes[10] }, 0);

                    int index = 12;
                    string notificationName = Encoding.UTF8.GetString(bytes, index, notificationNameLength);
                    index += notificationNameLength;
                    string title = Encoding.UTF8.GetString(bytes, index, titleLength);
                    index += titleLength;
                    string description = Encoding.UTF8.GetString(bytes, index, descriptionLength);
                    index += descriptionLength;
                    string applicationName = Encoding.UTF8.GetString(bytes, index, applicationNameLength);

                    NotificationType nt = NotificationType.GetByName(notificationName); //TODO: 
                    np = new NotificationPacket(protocolVersion, applicationName, password, nt, title, description, Priority.Normal, sticky);
                }
            }

            return np;
        }

        private static int ConvertPriorityToFlag(Priority priority)
        {
            int flags = (((int)priority) & 7) * 2;
            if (priority < 0) flags = flags | 8;
            return flags;
        }

        private static Priority ConvertFlagToPriority(int flags)
        {
            bool negative = ((flags & 8) == 8 ? true : false);
            int val = flags >> 1;
            if (negative)
            {
                val = ~(val ^ 7);
            }

            Console.WriteLine(val);
            return (Priority)val;
        }
    }
}
