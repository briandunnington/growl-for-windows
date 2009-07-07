using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Growl.UDPLegacy
{
    public class MessageSender
    {
        /// <summary>
        /// The default host (the local machine)
        /// </summary>
        public const string LOCALHOST = "127.0.0.1";

        /// <summary>
        /// The default port used by Growl to receive network notifications
        /// </summary>
        public const int DEFAULT_PORT = 9887;

        private const string GROWL_VERSION = "0.6";
        private const int PROTOCOL_VERSION = 1;

        /// <summary>
        /// The current version of Growl to target
        /// </summary>
        protected static string growlVersion = GROWL_VERSION;
        /// <summary>
        /// The Growl protocol version to use to format data packets
        /// </summary>
        protected static int protocolVersion = PROTOCOL_VERSION;
        /// <summary>
        /// The IP address to send notifications to
        /// </summary>
        protected string ipAddress;
        /// <summary>
        /// The port to send notifications to
        /// </summary>
        protected int port = DEFAULT_PORT;
        /// <summary>
        /// The name of the application sending the notifications
        /// </summary>
        protected string applicationName;
        /// <summary>
        /// The password used to validate notifications
        /// </summary>
        protected string password;

        /// <summary>
        /// Create a new <see cref="MessageSender"/> instance using the default host and port
        /// </summary>
        /// <param name="applicationName">The name of the application sending the notifications</param>
        /// <param name="password">The password used to validate the messages</param>
        public MessageSender(string applicationName, string password)
            : this(LOCALHOST, DEFAULT_PORT, applicationName, password)
        {
        }

        /// <summary>
        /// Create a new <see cref="MessageSender"/> instance
        /// </summary>
        /// <param name="ipAddress">The IP Address of the Growl instance to send to</param>
        /// <param name="port">The port that the Growl instance to send to will be listening on</param>
        /// <param name="applicationName">The name of the application sending the notifications</param>
        /// <param name="password">The password used to validate the messages</param>
        public MessageSender(string ipAddress, int port, string applicationName, string password)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.applicationName = applicationName;
            this.password = password;
        }

        /// <summary>
        /// The version of Growl to target
        /// </summary>
        /// <remarks>
        /// Setting this value has no effect on the protocol version used or any other aspect
        /// of how the class operates. It is provided solely as an information source.
        /// </remarks>
        public static string GrowlVersion
        {
            get
            {
                return growlVersion;
            }
            set
            {
                growlVersion = value;
            }
        }

        /// <summary>
        /// The Growl protocol version to use to format data packets
        /// </summary>
        /// <remarks>
        /// Currently, the only supported version is version 1.
        /// </remarks>
        public static int ProtocolVersion
        {
            get
            {
                return protocolVersion;
            }
            set
            {
                protocolVersion = value;
            }
        }

        /// <summary>
        /// The IP Address of the Growl instance to send to
        /// </summary>
        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
            set
            {
                this.ipAddress = value;
            }
        }

        /// <summary>
        /// The port that the Growl instance will be listening for messages on
        /// </summary>
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
        /// The name of the application sending the notifications
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }

        /// <summary>
        /// The password used by the Growl client to validate the messages
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        /// <summary>
        /// Registers the application with Growl
        /// </summary>
        /// <param name="notificationTypes">Array of <see cref="NotificationType"/>s that the application will be sending</param>
        /// <remarks>
        /// An application must register before it can send notifications. If notifications are send and the application is not
        /// registered, the notifications will be ignored and discarded. If an application is already registered and calls
        /// <c>Register</c> again, the list of notifications that the application is allowed to send will be updated. Remote
        /// applications can only send notifications if they register remotely, requiring the user to 'Allow Remote Registrations'.
        /// </remarks>
        public void Register(ref NotificationType[] notificationTypes)
        {
            if (notificationTypes != null && notificationTypes.Length > 0)
            {
                List<NotificationType> ntList = new List<NotificationType>();
                ntList.AddRange(notificationTypes);
                RegistrationPacket packet = new RegistrationPacket(protocolVersion, this.applicationName, this.password, ntList);
                Send(packet);
            }
        }

        /// <summary>
        /// Sends a basic notification, setting only the title
        /// </summary>
        /// <param name="notificationType">The <see cref="NotificationType">type</see> of notification</param>
        /// <param name="title">The title of the notification</param>
        /// <remarks>
        /// This method is <c>ComVisible(false)</c> since some COM languages do not allow method overloading.
        /// </remarks>
        public void Notify(NotificationType notificationType, string title)
        {
            Notify(notificationType, title, "", Growl.Connector.Priority.Normal, false);
        }

        /// <summary>
        /// Sends a notification, setting all available parameters
        /// </summary>
        /// <param name="notificationType">The <see cref="NotificationType">type</see> of notification</param>
        /// <param name="title">The title of the notification</param>
        /// <param name="description">The description or extended information of the notifications</param>
        /// <param name="priority">The <see cref="Priority"/> of the notification</param>
        /// <param name="sticky"><c>true</c> to request that the notification is sticky, <c>false</c> to request the notification be not sticky</param>
        public void Notify(NotificationType notificationType, string title, string description, Growl.Connector.Priority priority, bool sticky)
        {
            NotificationPacket packet = new NotificationPacket(protocolVersion, this.applicationName, this.password, notificationType, title, description, priority, sticky);
            Send(packet);
        }

        /// <summary>
        /// Sends the registration or notification message to the Growl instance
        /// </summary>
        /// <param name="packet">The <see cref="BasePacket"/> representing the message to send</param>
        private void Send(BasePacket packet)
        {
            UdpClient udp = new UdpClient(this.ipAddress, this.port);
            using (udp)
            {
                udp.Send(packet.Data, packet.Data.Length);
                udp.Close();
            }
        }
    }
}
