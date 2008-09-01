using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.Framework
{
    /// <summary>
    /// This class is only used to help make COM interop possible.
    /// It should not be used from within any application code.
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMessageReceiver
    {
        /// <summary>
        /// Fired when a registration message is received
        /// </summary>
        /// <param name="rp"><see cref="RegistrationPacket"/> containing the information received</param>
        /// <param name="receivedFrom">The host that sent the message</param>
        [DispId(1)]
        void RegistrationReceived(RegistrationPacket rp, string receivedFrom);

        /// <summary>
        /// Fired when a notification message is received
        /// </summary>
        /// <param name="np"><see cref="NotificationPacket"/> containing the information received</param>
        /// <param name="receivedFrom">The host that sent the message</param>
        [DispId(2)]
        void NotificationReceived(NotificationPacket np, string receivedFrom);
    }
}
