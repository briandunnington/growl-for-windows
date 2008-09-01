using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.Framework
{
    /// <summary>
    /// Represents a client that can listen for and receive Growl-style notifications,
    /// parse the information received, and pass on the events to application code.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComSourceInterfaces(typeof(IMessageReceiver))]
    public class MessageReceiver
    {
        /// <summary>
        /// The default port to listen for messages on
        /// </summary>
        public const int DEFAULT_PORT = 9887;
        /// <summary>
        /// The port currently being used to listen for messages on
        /// </summary>
        protected int port = DEFAULT_PORT;
        /// <summary>
        /// The underlying <see cref="UdpListener"/>
        /// </summary>
        protected UdpListener udp = null;
        /// <summary>
        /// Indicates if the MessageReceiver is currently listening for messages
        /// </summary>
        protected bool isRunning = false;
        /// <summary>
        /// The client password used to validate messages
        /// </summary>
        protected string password;
        /// <summary>
        /// Indicates if messages from remote machines should be allowed or not
        /// </summary>
        protected bool localMessagesOnly = true;
        /// <summary>
        /// Event handler for the <see cref="RegistrationReceived"/> event
        /// </summary>
        /// <param name="rp">The <see cref="RegistrationPacket"/> containing the data received</param>
        /// <param name="receivedFrom">The host that sent the message</param>
        public delegate void RegistrationHandler(RegistrationPacket rp, string receivedFrom);
        /// <summary>
        /// Event handler for the <see cref="NotificationReceived"/> event
        /// </summary>
        /// <param name="np">The <see cref="NotificationPacket"/> containing the data received</param>
        /// <param name="receivedFrom">The host that sent the message</param>
        public delegate void NotificationHandler(NotificationPacket np, string receivedFrom);
        /// <summary>
        /// Fires when a registration message is received
        /// </summary>
        public event RegistrationHandler RegistrationReceived;
        /// <summary>
        /// Fires when a notification message is received
        /// </summary>
        public event NotificationHandler NotificationReceived;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>The default constructor is parameterless to enable COM interoperability.</remarks>
        public MessageReceiver()
        {
            this.localMessagesOnly = false;
        }

        /// <summary>
        /// The port to listen for notifications on.
        /// </summary>
        /// <remarks>The default value is <see cref="DEFAULT_PORT"/></remarks>
        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        /// <summary>
        /// Starts listening for incoming notifications
        /// </summary>
        public virtual void Start()
        {
            if (!this.isRunning)
            {
                // start listening for incoming notifications
                this.udp = new UdpListener(this.port, this.localMessagesOnly);
                this.udp.PacketReceived += new UdpListener.PacketHandler(udp_PacketReceived);
                this.udp.Start();
                this.isRunning = true;
            }
        }

        /// <summary>
        /// Stops listening for incoming notifications
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                if(this.udp != null) this.udp.Stop();
                this.udp = null;
            }
            catch
            {
                // swallow any exceptions (this handles the case when Growl is stopped while still listening for network notifications)
            }
            finally
            {
                this.isRunning = false;
            }
        }

        /// <summary>
        /// Sets the password used to validate incoming messages
        /// </summary>
        /// <param name="password">The password</param>
        public virtual void SetPassword(string password)
        {
            this.password = password;
        }

        /// <summary>
        /// Indicates if the MessageReceiver is currently listening for incoming messages
        /// </summary>
        /// <value><c>true</c> if the MessageReceiver is currently listening, <c>false</c> otherwise</value>
        public virtual bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        /// <summary>
        /// Handles incoming packet data when a message is received
        /// </summary>
        /// <param name="bytes">The raw packet data</param>
        /// <param name="receivedFrom">The host that sent the packet</param>
        protected virtual void udp_PacketReceived(byte[] bytes, string receivedFrom)
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
                    if (np != null) this.OnNotificationPacketReceived(np, receivedFrom);
                }
            }
        }

        /// <summary>
        /// Fired when a <see cref="RegistrationPacket"/> is received
        /// </summary>
        /// <param name="rp">The <see cref="RegistrationPacket"/> containing the information received</param>
        /// <param name="receivedFrom">The host from which the packet was received</param>
        /// <remarks>
        /// </remarks>
        protected virtual void OnRegistrationPacketReceived(RegistrationPacket rp, string receivedFrom)
        {
            if (this.RegistrationReceived != null) this.RegistrationReceived(rp, receivedFrom);
        }

        /// <summary>
        /// Fired when a <see cref="NotificationPacket"/> is received
        /// </summary>
        /// <param name="np">The <see cref="NotificationPacket"/> containing the information received</param>
        /// <param name="receivedFrom">The host from which the packet was received</param>
        protected virtual void OnNotificationPacketReceived(NotificationPacket np, string receivedFrom)
        {
            if (this.NotificationReceived != null) this.NotificationReceived(np, receivedFrom);
        }
    }
}
