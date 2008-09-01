using System;
using System.Collections.Generic;
using System.Text;
using Growl.Framework;

namespace Growl.AppBridge
{
    class NetworkReceiver : BaseReceiver
    {
        internal NetworkReceiver()
        {
            // this class is like a singleton.
            // instances should only be created by the parent AppBridge
            this.port = NetGrowl.DEFAULT_PORT;
            this.localMessagesOnly = false;
        }

        /*
        protected override void udp_PacketReceived(byte[] bytes, string receivedFrom)
        {
            // parse the packet
            if (bytes != null && bytes.Length > 18)
            {
                int protocolVersion = (int)bytes[0];
                PacketType packetType = (PacketType)bytes[1];

                if (packetType == PacketType.Registration)
                {
                    RegistrationPacket rp = RegistrationPacket.FromPacket(bytes, this.password);
                    if (rp != null) this.OnRegistrationPacketReceived(rp, receivedFrom);
                }

                if (packetType == PacketType.Notification)
                {
                    NotificationPacket np = NotificationPacket.FromPacket(bytes, this.password);
                    if(np != null) this.OnNotificationPacketReceived(np, receivedFrom);
                }
            }
        }
         * */
    }
}
