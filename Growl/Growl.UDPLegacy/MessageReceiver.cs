using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;
using Growl.Connector;

namespace Growl.UDPLegacy
{
    /// <summary>
    /// Represents a client that can listen for and receive Growl-style notifications,
    /// parse the information received, and pass on the events to application code.
    /// </summary>
    public class MessageReceiver : IDisposable
    {
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
        /// The default port to listen for local messages on
        /// </summary>
        public const int LOCAL_PORT = 9888;

        /// <summary>
        /// The default port to listen for remote messages on
        /// </summary>
        public const int NETWORK_PORT = 9887;

        /// <summary>
        /// The port currently being used to listen for messages on
        /// </summary>
        protected int port = NETWORK_PORT;

        /// <summary>
        /// The message logged when a request is successfully processed.
        /// </summary>
        private const string RESULT_OK = "Message was valid. Request was processed.";

        /// <summary>
        /// The message logged when a request is invalid for any reason.
        /// </summary>
        private const string RESULT_ERROR = "Invalid request. Either the message was malformed or the password was incorrect.";

        /// <summary>
        /// Used to seperated sections in a log file
        /// </summary>
        private const string SEPERATOR = "\r\n\r\n-----------------------------------------------------\r\n\r\n";

        /// <summary>
        /// The underlying <see cref="UdpListener"/>
        /// </summary>
        protected UdpListener udp = null;

        /// <summary>
        /// Indicates if the MessageReceiver is currently listening for messages
        /// </summary>
        protected bool isRunning = false;

        /// <summary>
        /// Contains a list of the user's valid passwords.
        /// </summary>
        protected PasswordManager passwordManager;

        /// <summary>
        /// Indicates if local requests must supply a valid password
        /// </summary>
        protected bool requireLocalPassword;

        /// <summary>
        /// Indicates if messages from remote machines should be allowed or not
        /// </summary>
        protected bool allowNetworkNotifications;

        /// <summary>
        /// Indicates if requests should be logged
        /// </summary>
        protected bool loggingEnabled;

        /// <summary>
        /// The path where log files are written
        /// </summary>
        protected string logFolder;


        /// <summary>
        /// Default constructor.
        /// </summary>
        public MessageReceiver(int port, PasswordManager passwordManager, bool loggingEnabled, string logFolder)
        {
            this.port = port;
            this.passwordManager = passwordManager;
            this.loggingEnabled = loggingEnabled;
            this.logFolder = logFolder;
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
        }

        /// <summary>
        /// Starts listening for incoming notifications
        /// </summary>
        public virtual bool Start()
        {
            if (!this.isRunning)
            {
                try
                {
                    // start listening for incoming notifications
                    this.udp = new UdpListener(this.port, false);
                    this.udp.PacketReceived += new UdpListener.PacketHandler(udp_PacketReceived);
                    this.udp.Start();
                    this.isRunning = true;
                }
                catch
                {
                    this.isRunning = false;
                }
            }
            return this.isRunning;
        }

        /// <summary>
        /// Stops listening for incoming notifications
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                if (this.udp != null)
                {
                    this.udp.PacketReceived -= new UdpListener.PacketHandler(udp_PacketReceived);
                    this.udp.Stop();
                    this.udp.Dispose();
                }
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
        /// Contains a list of the user's passwords used to authorize incoming requests
        /// </summary>
        /// <value>
        /// <see cref="PasswordManager"/>
        /// </value>
        public PasswordManager PasswordManager
        {
            get
            {
                return this.passwordManager;
            }
        }

        /// <summary>
        /// Indicates if local applications must supply a password.
        /// </summary>
        /// <value>
        /// <c>true</c> - local applications must supply a password;
        /// <c>false</c> - local applications do not need to supply a password
        /// </value>
        /// <remarks>
        /// Network applications must always supply a password.
        /// Not all local applications supply the password, so some notifications may be blocked if enabled.
        /// </remarks>
        public bool RequireLocalPassword
        {
            get
            {
                return this.requireLocalPassword;
            }
            set
            {
                this.requireLocalPassword = value;
            }
        }

        /// <summary>
        /// Indicates if network (non-local) requests are allowed.
        /// </summary>
        /// <value>
        /// <c>true</c> - network requests are allowed;
        /// <c>false</c> - network requests are blocked
        /// </value>
        public bool AllowNetworkNotifications
        {
            get
            {
                return this.allowNetworkNotifications;
            }
            set
            {
                this.allowNetworkNotifications = value;
            }
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
        /// <param name="isLocal">Indicates if the request came from the local machine</param>
        protected virtual void udp_PacketReceived(byte[] bytes, string receivedFrom, bool isLocal)
        {
            // if this is a network request and we dont allow them, stop here
            if (!isLocal && !this.AllowNetworkNotifications)
                return;

            StringBuilder sb = new StringBuilder();
            bool processed = false;

            // parse the packet
            if (bytes != null && bytes.Length > 18)
            {
                int protocolVersion = (int)bytes[0];
                PacketType packetType = (PacketType)bytes[1];

                if (packetType == PacketType.Registration)
                {
                    RegistrationPacket rp = RegistrationPacket.FromPacket(bytes, this.passwordManager, isLocal, this.requireLocalPassword);
                    if (rp != null)
                    {
                        this.OnRegistrationPacketReceived(rp, receivedFrom);
                        processed = true;

                        sb.AppendFormat("Protocol Version:         {0}\r\n", rp.ProtocolVersion);
                        sb.AppendFormat("Packet Type:              {0}\r\n", rp.PacketType);
                        sb.AppendFormat("Application Name:         {0}\r\n", rp.ApplicationName);
                        sb.AppendFormat("Notifications Registered: {0}\r\n\r\n", rp.NotificationTypes.Length);
                        foreach (NotificationType nt in rp.NotificationTypes)
                        {
                            sb.AppendFormat("  Notification Type: {0}\r\n", nt.Name);
                            sb.AppendFormat("  Enabled:           {0}\r\n\r\n", nt.Enabled);
                        }
                    }
                    else
                    {
                        sb.Append("Invalid message - either the message format was incorrect or the password was incorrect");
                    }
                }
                else if (packetType == PacketType.Notification)
                {
                    NotificationPacket np = NotificationPacket.FromPacket(bytes, this.passwordManager, isLocal, this.requireLocalPassword);
                    if (np != null)
                    {
                        this.OnNotificationPacketReceived(np, receivedFrom);
                        processed = true;

                        sb.AppendFormat("Protocol Version:  {0}\r\n", np.ProtocolVersion);
                        sb.AppendFormat("Packet Type:       {0}\r\n", np.PacketType);
                        sb.AppendFormat("Application Name:  {0}\r\n", np.ApplicationName);
                        sb.AppendFormat("Notification Type: {0}\r\n", np.NotificationType.Name);
                        sb.AppendFormat("Title:             {0}\r\n", np.Title);
                        sb.AppendFormat("Description:       {0}\r\n", np.Description);
                        sb.AppendFormat("Sticky:            {0}\r\n", np.Sticky);
                        sb.AppendFormat("Priority:          {0}\r\n", np.Priority);
                    }
                    else
                    {
                        sb.Append("Invalid message - either the message format was incorrect or the password was incorrect");
                    }
                }
                else
                {
                    sb.Append("Malformed packet - unrecognized data");
                }
            }
            else
            {
                sb.Append("Malformed packet - not enough bytes");
            }
            Log(sb.ToString(), bytes, receivedFrom, processed);
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

        /// <summary>
        /// Logs information about the request
        /// </summary>
        /// <param name="packetInfo">Information about the data received</param>
        /// <param name="bytes">The actual bytes received</param>
        /// <param name="receivedFrom">The address of the sender</param>
        /// <param name="processed">Indicates if the request was valid and successfully processed</param>
        private void Log(string packetInfo, byte[] bytes, string receivedFrom, bool processed)
        {
            try
            {
                if (this.loggingEnabled && !String.IsNullOrEmpty(this.logFolder))
                {
                    string requestID = System.Guid.NewGuid().ToString();
                    string filename = String.Format(@"UDP_{0}.txt", requestID);
                    string filepath = PathUtility.Combine(this.logFolder, filename);
                    bool exists = System.IO.File.Exists(filepath);

                    System.IO.StreamWriter w = (exists ? new System.IO.StreamWriter(filepath, true) : System.IO.File.CreateText(filepath));
                    using (w)
                    {
                        w.Write("Timestamp: {0}\r\n", DateTime.Now.ToString());
                        w.Write("Received From: {0}\r\n", receivedFrom);
                        w.Write(SEPERATOR);
                        w.Write(packetInfo);
                        w.Write(SEPERATOR);
                        w.Flush();
                        w.BaseStream.Write(bytes, 0, bytes.Length);
                        w.Write(SEPERATOR);
                        w.Write("Result: {0}\r\n", (processed ? RESULT_OK : RESULT_ERROR));
                        w.Close();
                    }
                }
            }
            catch
            {
                // suppress logging exceptions
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (this.udp != null)
                    {
                        this.udp.PacketReceived -= new UdpListener.PacketHandler(udp_PacketReceived);
                        this.udp.Dispose();
                        this.udp = null;
                    }
                }
                catch
                {
                    // suppress
                }
            }
        }

        #endregion
    }
}
