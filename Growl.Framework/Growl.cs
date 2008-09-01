using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.Framework
{
    /// <summary>
    /// This is the main class used to send notifications from locally-installed 
    /// applications to Growl.
    /// </summary>
    /// <remarks>
    /// Growl will only accept notifications from the same machine when using this
    /// interface. If your application runs on computer other than the one where
    /// Growl is running, you must use the <see cref="NetGrowl"/> class instead.
    /// </remarks>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Growler
    {
        /// <summary>
        /// Default port for local applications to send notifications
        /// </summary>
        public const int DEFAULT_LOCAL_PORT = 9888;

        private const string DEFAULT_LOCAL_ADDRESS = "127.0.0.1";
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
        protected string ipAddress = DEFAULT_LOCAL_ADDRESS;
        /// <summary>
        /// The port to send notifications to
        /// </summary>
        protected int port = DEFAULT_LOCAL_PORT;
        /// <summary>
        /// The name of the application sending the notifications
        /// </summary>
        protected string applicationName;
        /// <summary>
        /// The password used to validate notifications (this is always "" for local apps)
        /// </summary>
        private string password = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>The default constructor is parameterless to enable COM interoperability.</remarks>
        public Growler()
        {
        }

        /// <summary>
        /// Create a new <see cref="Growl"/> instance
        /// </summary>
        /// <param name="applicationName">The name of the application sending the notifications</param>
        public Growler(string applicationName)
        {
            this.applicationName = applicationName;
        }

        /// <summary>
        /// The version of Growl to target
        /// </summary>
        /// <remarks>
        /// Setting this value has no effect on the protocol version used or any other aspect
        /// of how the class operates. It is provided solely as a information source.
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
        /// Registers the application with Growl
        /// </summary>
        /// <param name="notificationTypes">Array of <see cref="NotificationType"/>s that the application will be sending</param>
        /// <remarks>
        /// An application must register before it can send notifications. If notifications are send and the application is not
        /// registered, the notifications will be ignored and discarded. If an application is already registered and calls
        /// <c>Register</c> again, the list of notifications that the application is allowed to send will be updated.
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
        [ComVisible(false)]
        public void Notify(NotificationType notificationType, string title)
        {
            Notify(notificationType, title, "", Priority.Normal, false);
        }

        /// <summary>
        /// Sends a notification, setting all available parameters
        /// </summary>
        /// <param name="notificationType">The <see cref="NotificationType">type</see> of notification</param>
        /// <param name="title">The title of the notification</param>
        /// <param name="description">The description or extended information of the notifications</param>
        /// <param name="priority">The <see cref="Priority"/> of the notification</param>
        /// <param name="sticky"><c>true</c> to request that the notification is sticky, <c>false</c> to request the notification be not sticky</param>
        public void Notify(NotificationType notificationType, string title, string description, Priority priority, bool sticky)
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
            udp.Send(packet.Data, packet.Data.Length);
            udp.Close();
        }
    }
}
