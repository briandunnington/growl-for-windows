using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.UDPLegacy
{
    /// <summary>
    /// Identifies the type of information that a packet contains
    /// </summary>
    public enum PacketType
    {
        /// <summary>
        /// Used when sending registration packets
        /// </summary>
        Registration = 0,

        /// <summary>
        /// Used when sending notification packets
        /// </summary>
        Notification = 1  /*,

        Registration_SHA256 = 3,

        Notification_SHA256 = 4,

        Registration_NOAUTH = 5,

        Notification_NOAUTH = 6,

        Registration_GrowlCast = 7,

        Notification_GrowlCast = 8 */
    }
}
