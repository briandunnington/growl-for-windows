using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.Framework
{
    /// <summary>
    /// Represents a Registration message sent as a UDP packet as specified by the Growl protocol
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class RegistrationPacket : BasePacket
    {
        /// <summary>
        /// A <see cref="List{NotificationType}">list</see> of <see cref="NotificationType"/>s that 
        /// the application will be sending
        /// </summary>
        protected List<NotificationType> notificationTypes;

        /// <summary>
        /// Creates a new <see cref="RegistrationPacket"/>
        /// </summary>
        /// <param name="protocolVersion">The Growl protocol version</param>
        /// <param name="applicationName">The name of the sending application</param>
        /// <param name="password">The password used to validate the receiving client</param>
        /// <param name="notificationTypes">A list of <see cref="NotificationType"/>s that this application plans to send</param>
        public RegistrationPacket(int protocolVersion, string applicationName, string password, List<NotificationType> notificationTypes)
        {
            this.packetType = PacketType.Registration;
            this.protocolVersion = protocolVersion;
            this.applicationName = applicationName;
            this.password = password;
            this.notificationTypes = notificationTypes;
            this.data = PrepareData();
        }

        /// <summary>
        /// All of the <see cref="NotificationType"/>s that this application can send
        /// </summary>
        public NotificationType[] NotificationTypes
        {
            get
            {
                return this.notificationTypes.ToArray();
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

            ByteBuilder nb = new ByteBuilder();
            ByteBuilder db = new ByteBuilder();
            int index = 0;
            int notificationTypeCount = 0;
            int notificationTypeEnabledCount = 0;
            foreach (NotificationType notificationType in this.notificationTypes)
            {
                nb.Append(notificationType.Name.Length);
                nb.Append(notificationType.Name);

                notificationTypeCount++;
                if (notificationType.Enabled)
                {
                    notificationTypeEnabledCount++;
                    db.Append((byte)index);
                }
                index++;
            }

            ByteBuilder bb = new ByteBuilder();
            bb.Append((byte)this.protocolVersion);
            bb.Append((byte)this.packetType);
            bb.Append(ByteBuilder.GetStringLength(this.applicationName));
            bb.Append((byte)notificationTypeCount);
            bb.Append((byte)notificationTypeEnabledCount);
            bb.Append(this.applicationName);
            bb.Append(nb.GetBytes());
            bb.Append(db.GetBytes());

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
        /// Given the raw packet data, converts the data back into a <see cref="RegistrationPacket"/>
        /// </summary>
        /// <param name="bytes">The raw packet data</param>
        /// <param name="password">The client password</param>
        /// <returns><see cref="RegistrationPacket"/></returns>
        /// <remarks>
        /// If the client password does not match the password used to validate the sent notification,
        /// or if the packet is malformed in any way, a <c>null</c> object will be returned.
        /// </remarks>
        public static RegistrationPacket FromPacket(byte[] bytes, string password)
        {
            RegistrationPacket rp = null;

            // parse the packet
            if (bytes != null && bytes.Length > 18)
            {
                // check md5 hash first
                bool valid = BasePacket.IsPasswordValid(bytes, password);
                if (!valid)
                    return rp;

                int protocolVersion = (int)bytes[0];    //TODO: add code to check/handle different versions
                PacketType packetType = (PacketType)bytes[1];

                if (packetType == PacketType.Registration)
                {
                    int index = 6;
                    List<NotificationType> notificationTypes = new List<NotificationType>();
                    short applicationNameLength = BitConverter.ToInt16(new byte[] { bytes[3], bytes[2] }, 0);
                    int notificationCount = (int)bytes[4];
                    int defaultNotificationCount = (int)bytes[5];
                    string applicationName = Encoding.UTF8.GetString(bytes, index, applicationNameLength);
                    index += applicationNameLength;
                    for (int n = 0; n < notificationCount; n++ )
                    {
                        short notificationNameLength = BitConverter.ToInt16(new byte[] { bytes[index + 1], bytes[index] }, 0);
                        string notificationName = Encoding.UTF8.GetString(bytes, index + 2, notificationNameLength);
                        index += 2 + notificationNameLength;
                        Console.WriteLine(notificationName);
                        NotificationType nt = new NotificationType();
                        nt.Name = notificationName;
                        nt.Enabled = false;
                        notificationTypes.Add(nt);
                    }
                    for (int d = 0; d < defaultNotificationCount; d++)
                    {
                        int notificationIndex = (int) bytes[index++];
                        notificationTypes[notificationIndex].Enabled = true;
                        Console.WriteLine(notificationIndex);
                    }

                    rp = new RegistrationPacket(protocolVersion, applicationName, password, notificationTypes);
                }
            }

            return rp;
        }
    }
}
