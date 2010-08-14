using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using Growl.CoreLibrary;
using Growl.Connector;

namespace Growl.Daemon
{
    /// <summary>
    /// Represents the Growl server that listens for and receives incoming GNTP requests.
    /// </summary>
    /// <remarks>
    /// The server only handles receiving the request and returning the response - the actual display/handling
    /// of the notification is up to the calling code. Implementors can register for the various server events
    /// (RegisterReceived, NotifyReceived) and handle the notifications appropriately.
    /// 
    /// If Bonjour is available and running on the server machine, the server will also advertise itself with a 
    /// type of "_gntp._tcp" and the port configured with each instance.
    /// </remarks>
    public class GrowlServer : IDisposable
    {
        /// <summary>
        /// The type of service advertised via Bonjour
        /// </summary>
        public const string BONJOUR_SERVICE_TYPE = "_gntp._tcp";

        /// <summary>
        /// The domain used when advertising via Bonjour
        /// </summary>
        public const string BONJOUR_SERVICE_DOMAIN = "";

        /// <summary>
        /// The name of the service advertised via Bonjour
        /// </summary>
        public static readonly string BonjourServiceName = String.Format("Growl on {0}", Environment.MachineName);

        /// <summary>
        /// A unique ID for this server. (This value may change across application restarts, but should not change while the app is running.)
        /// </summary>
        public static readonly string ServerID = System.Guid.NewGuid().ToString();

        /// <summary>
        /// Represents the method that will handle the <see cref="RegisterReceived"/> event.
        /// </summary>
        /// <param name="application">The <see cref="Application"/> that is registering</param>
        /// <param name="notificationTypes">A list of <see cref="NotificationType"/>s being registered</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with the request</param>
        /// <returns><see cref="Response"/></returns>
        public delegate Response RegisterReceivedEventHandler(Application application, List<NotificationType> notificationTypes, RequestInfo requestInfo);

        /// <summary>
        /// Represents the method that will handle the <see cref="NotifyReceived"/> event.
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> that was received</param>
        /// <param name="callbackInfo">The <see cref="CallbackInfo"/> for the notification</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with the request</param>
        /// <returns><see cref="Response"/></returns>
        public delegate Response NotifyReceivedEventHandler(Notification notification, CallbackInfo callbackInfo, RequestInfo requestInfo);

        /// <summary>
        /// Represents the method that will handle the <see cref="SubscribeReceived"/> event.
        /// </summary>
        /// <param name="subscriber">The <see cref="Subscriber"/> information</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with the request</param>
        /// <returns><see cref="SubscriptionResponse"/></returns>
        public delegate SubscriptionResponse SubscribeReceivedEventHandler(Subscriber subscriber, RequestInfo requestInfo);

        /// <summary>
        /// Represents the method that will handle the <see cref="ServerMessage"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="GrowlServer"/> sending the message</param>
        /// <param name="type">The <see cref="LogMessageType">type</see> of message</param>
        /// <param name="message">The message</param>
        public delegate void ServerMessageEventHandler(GrowlServer sender, LogMessageType type, string message);

        /// <summary>
        /// Raised when the server fails to start properly
        /// </summary>
        public event EventHandler FailedToStart;

        /// <summary>
        /// Raised when a REGISTER request is received
        /// </summary>
        public event RegisterReceivedEventHandler RegisterReceived;

        /// <summary>
        /// Raised when a NOTIFY request is received
        /// </summary>
        public event NotifyReceivedEventHandler NotifyReceived;

        /// <summary>
        /// Raised when a SUBSCRIBE request is received
        /// </summary>
        public event SubscribeReceivedEventHandler SubscribeReceived;

        /// <summary>
        /// Raised when the server is outputting an informational message to it implementor
        /// </summary>
        public event ServerMessageEventHandler ServerMessage;

        /// <summary>
        /// The port that the server will listen on
        /// </summary>
        private ushort port = GrowlConnector.TCP_PORT;

        /// <summary>
        /// The default name of the server
        /// </summary>
        private string serverName = "Growl/Win";

        /// <summary>
        /// Indicates if the server is started
        /// </summary>
        private bool isStarted;

        /// <summary>
        /// The socket used to listen for requests
        /// </summary>
        private AsyncSocket listenSocket;

        /// <summary>
        /// A list of connected sockets currently being serviced
        /// </summary>
        private ConnectedSocketCollection connectedSockets;

        /// <summary>
        /// A list of MessageHandlers currently servicing requests
        /// </summary>
        private Dictionary<AsyncSocket, MessageHandler> connectedHandlers;

        /// <summary>
        /// Runs in the background and disconnects orphaned sockets
        /// </summary>
        private System.Timers.Timer socketCleanupTimer;

        /// <summary>
        /// The list of valid passwords to allow
        /// </summary>
        private PasswordManager passwordManager;

        /// <summary>
        /// The full path to the user folder (where logs, data cache, etc are kept)
        /// </summary>
        private string userFolder;

        /// <summary>
        /// The full path to the log folder
        /// </summary>
        private string logFolder;

        /// <summary>
        /// The full path to the resource cache folder
        /// </summary>
        private string resourceFolder;

        /// <summary>
        /// Indicates if logging is enabled or not
        /// </summary>
        private bool loggingEnabled;

        /// <summary>
        /// The Bonjour service that advertises this server
        /// </summary>
        private BonjourService bonjour;

        /// <summary>
        /// Indicates if local applications must supply the password or not
        /// </summary>
        private bool requireLocalPassword;

        /// <summary>
        /// Indicates if LAN applications must supply the password or not
        /// </summary>
        private bool requireLANPassword;

        /// <summary>
        /// Indicates if network (non-local) request are allowed
        /// </summary>
        private bool allowNetworkNotifications = false;

        /// <summary>
        /// Indicates if webpage notifications are allowed
        /// </summary>
        private bool allowWebNotifications = false;

        /// <summary>
        /// Indicates if client subscriptions are allowed
        /// </summary>
        private bool allowSubscriptions = false;


        /// <summary>
        /// Creates a new instance of the Growl server.
        /// </summary>
        /// <param name="port">The port to listen on. The standard GNTP port is <see cref="ConnectorBase.TCP_PORT"/>.</param>
        /// <param name="passwordManager">The <see cref="PasswordManager"/> containing the list of allowed passwords.</param>
        /// <param name="userFolder">The full path to the user folder where logs, resource cache, and other files will be stored.</param>
        public GrowlServer(int port, PasswordManager passwordManager, string userFolder)
        {
            // this will set the server name and version properly
            ServerName = serverName;

            this.port = (ushort) port;

            this.passwordManager = passwordManager;

            this.userFolder = userFolder;

            this.logFolder = PathUtility.Combine(userFolder, @"Log\");
            PathUtility.EnsureDirectoryExists(this.logFolder);

            this.resourceFolder = PathUtility.Combine(userFolder, @"Resources\");
            PathUtility.EnsureDirectoryExists(this.resourceFolder);

            this.listenSocket = new AsyncSocket();
            this.listenSocket.AllowMultithreadedCallbacks = true; // VERY IMPORTANT: if we dont set this, async socket events will silently be swallowed by the AsyncSocket class
            listenSocket.DidAccept += new AsyncSocket.SocketDidAccept(listenSocket_DidAccept);
            //listenSocket.DidClose += new AsyncSocket.SocketDidClose(listenSocket_DidClose);
 
            // Initialize list to hold connected sockets
            // We support multiple concurrent connections
            connectedSockets = new ConnectedSocketCollection();
            connectedHandlers = new Dictionary<AsyncSocket, MessageHandler>();
            socketCleanupTimer = new System.Timers.Timer(30 * 1000);
            socketCleanupTimer.Elapsed += new System.Timers.ElapsedEventHandler(socketCleanupTimer_Elapsed);

            ResourceCache.ResourceFolder = this.resourceFolder;
            ResourceCache.Enabled = true;

            this.bonjour = new BonjourService(BonjourServiceName, BONJOUR_SERVICE_TYPE);
        }

        /// <summary>
        /// The port that the server will listen on
        /// </summary>
        /// <value>int - The standard GNTP port is <see cref="ConnectorBase.TCP_PORT"/>.</value>
        public int Port
        {
            get
            {
                return this.port;
            }
        }

        /// <summary>
        /// The name of the server.
        /// </summary>
        /// <remarks>
        /// This name is returned in the GNTP responses as the Origin-Software-Name node.
        /// </remarks>
        public string ServerName
        {
            get
            {
                return this.serverName;
            }
            set
            {
                System.Diagnostics.FileVersionInfo f = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string version = f.FileVersion;

                this.serverName = value;
                ExtensibleObject.SetSoftwareInformation(this.serverName, version);
            }
        }

        /// <summary>
        /// Indicates if logging is enabled.
        /// </summary>
        /// <remarks>
        /// If logging is enabled, a new text file will be written for each request/response/callback group. The files can
        /// be found in the 'Logs' folder underneath the specified UserFolder.
        /// </remarks>
        public bool LoggingEnabled
        {
            get
            {
                return this.loggingEnabled;
            }
            set
            {
                this.loggingEnabled = value;
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
        /// Indicates if LAN applications must supply a password.
        /// </summary>
        /// <value>
        /// <c>true</c> - LAN applications must supply a password;
        /// <c>false</c> - LAN applications do not need to supply a password
        /// </value>
        /// <remarks>
        /// Network applications normally must always supply a password, but LAN
        /// applications can be exempted.
        /// </remarks>
        public bool RequireLANPassword
        {
            get
            {
                return this.requireLANPassword;
            }
            set
            {
                this.requireLANPassword = value;
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
        /// Indicates if webpage (browser-based) requests are allowed.
        /// </summary>
        /// <value>
        /// <c>true</c> - browser-based requests are allowed;
        /// <c>false</c> - browser-based requests are blocked
        /// </value>
        public bool AllowWebNotifications
        {
            get
            {
                return this.allowWebNotifications;
            }
            set
            {
                this.allowWebNotifications = value;
            }
        }

        /// <summary>
        /// Indicates if client subscriptions (SUBSCRIBE) requests are allowed
        /// </summary>
        /// <value>
        /// <c>true</c> - Subscriptions are allowed;
        /// <c>false</c> - Subscriptions are not allowed
        /// </value>
        public bool AllowSubscriptions
        {
            get
            {
                return this.allowSubscriptions;
            }
            set
            {
                this.allowSubscriptions = value;
            }
        }

        /// <summary>
        /// Contains the list of allowed passwords used to authorize incoming requests
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
        /// Starts the server
        /// </summary>
        /// <returns>
        /// <c>true</c> - if the server started successfully;
        /// <c>false</c> - the server failed to start. The <see cref="OnFailedToStart"/> event will be raised.
        /// </returns>
        public bool Start()
        {
            if (!this.isStarted)
            {
                Exception error;
                if (!listenSocket.Accept(this.port, out error))
                {
                    this.OnFailedToStart(this, EventArgs.Empty);
                    LogError("Error starting server: {0}", error);
                    return false;
                }

                if(BonjourService.IsSupported)
                    this.bonjour.Start(this.port);

                this.socketCleanupTimer.Start();

                LogInfo("Growl server started on port {0}", listenSocket.LocalPort);
                isStarted = true;
            }
            return true;
        }

        /// <summary>
        /// Stops the server and closes any open connections.
        /// </summary>
        public void Stop()
        {
            if (this.isStarted)
            {
                // Stop accepting connections
                listenSocket.Close();

                // Stop trying to clean up sockets
                this.socketCleanupTimer.Stop();

                // Stop any client connections
                int connected = 0;
                while(connectedSockets.Count > 0)
                {
                    // if this happens, then the socket could not be closed properly for some reason.
                    // we have no choice but to abandon the routine and force the stop anyway.
                    // (hopefully, dangling sockets will get cleaned up later by the GC)
                    if (connectedSockets.Count == connected)
                    {
                        this.connectedSockets.Clear();
                        this.connectedHandlers.Clear();
                        break;
                    }
                    connected = connectedSockets.Count;

                    // Call Disconnect on the socket,
                    // which will invoke the DidDisconnect method,
                    // which will remove the socket and handler from the list.

                    // (we have to use some trickery to a single item from the list without knowing the key)
                    AsyncSocket someSocket = connectedSockets[0].Socket;
                    if(someSocket != null) someSocket.CloseImmediately();
                }

                if(this.bonjour != null) this.bonjour.Stop();

                LogInfo("Stopped Growl server");
                isStarted = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="FailedToStart"/> event
        /// </summary>
        /// <param name="sender">The <see cref="GrowlServer"/> instance that failed to start</param>
        /// <param name="args"><see cref="EventArgs.Empty"/></param>
        protected void OnFailedToStart(object sender, EventArgs args)
        {
            if (this.FailedToStart != null)
            {
                this.FailedToStart(sender, args);
            }
        }

        /// <summary>
        /// Handles the <see cref="AsyncSocket.DidAccept"/> event.
        /// </summary>
        /// <param name="sender">The listening <see cref="AsyncSocket"/></param>
        /// <param name="newSocket">The new <see cref="AsyncSocket"/> that was accepted</param>
        private void listenSocket_DidAccept(AsyncSocket sender, AsyncSocket newSocket)
        {
            LogInfo("Accepted client {0}:{1}", newSocket.RemoteAddress, newSocket.RemotePort);

            // check origin
            bool isLocal = IPAddress.IsLoopback(newSocket.RemoteAddress);
            bool isLAN = Growl.CoreLibrary.IPUtilities.IsInSameSubnet(newSocket.LocalAddress, newSocket.RemoteAddress);
            if (!this.allowNetworkNotifications && !isLocal)
            {
                // remote connections not allowed - Should we return a GNTP error response? i think this is better (no reply at all)
                LogInfo("Blocked network request from '{0}'", newSocket.RemoteAddress);
                newSocket.Close();
                return;
            }

            bool passwordRequired = true;
            if (isLocal && !this.RequireLocalPassword) passwordRequired = false;
            else if (isLAN && !this.RequireLANPassword) passwordRequired = false;

            // SUPER IMPORTANT
            newSocket.AllowMultithreadedCallbacks = true;

            MessageHandler mh = new MessageHandler(this.serverName, this.passwordManager, passwordRequired, this.logFolder, this.loggingEnabled, this.allowNetworkNotifications, this.allowWebNotifications, this.allowSubscriptions);
            newSocket.DidClose += new AsyncSocket.SocketDidClose(newSocket_DidClose);
            mh.MessageParsed += new MessageHandler.MessageHandlerMessageParsedEventHandler(mh_MessageParsed);
            mh.Error += new MessageHandler.MessageHandlerErrorEventHandler(mh_Error);
            mh.SocketUsageComplete += new MessageHandler.MessageHandlerSocketUsageCompleteEventHandler(mh_SocketUsageComplete);

            connectedSockets.Add(new ConnectedSocket(newSocket));
            connectedHandlers.Add(newSocket, mh);

            mh.InitialRead(newSocket);
        }

        /*
        void listenSocket_DidDisconnect(AsyncSocket sender)
        {
            // we dont want to get into this function, but if we do, it means that the master listener has
            // been closed (usually due to some kind of exception).
            // if that happens, we want to make sure to restart it or else notifications will just be silently ignored
            Exception error;
            if (!listenSocket.Accept(this.port, out error))
            {
                this.OnFailedToStart(this, EventArgs.Empty);
                LogError("Error restarting server: {0}", error);
            }
        }
         * */

        /// <summary>
        /// Handles the <see cref="MessageHandler.Error"/> event
        /// </summary>
        /// <param name="error">The <see cref="Error"/> that occurred</param>
        void mh_Error(Error error)
        {
            AddServerHeaders(error);
        }

        /// <summary>
        /// Handles the <see cref="MessageHandler.MessageParsed"/> event
        /// </summary>
        /// <param name="mh">The <see cref="MessageHandler"/> that parsed the message</param>
        /// <remarks>
        /// This method starts a new thread to peform the actual message handling
        /// </remarks>
        private void mh_MessageParsed(MessageHandler mh)
        {
            ParameterizedThreadStart pts = new ParameterizedThreadStart(HandleParsedMessage);
            Thread t = new Thread(pts);
            t.Start(mh);
        }

        /// <summary>
        /// Handles the parsed message after it is received
        /// </summary>
        /// <param name="obj">The <see cref="MessageHandler"/> object that parsed the message</param>
        private void HandleParsedMessage(object obj)
        {
            MessageHandler mh = (MessageHandler)obj;
            GNTPRequest request = mh.Request;

            try
            {
                Response response = null;
                switch (request.Directive)
                {
                    case RequestType.REGISTER:
                        Application application = Application.FromHeaders(request.Headers);
                        List<NotificationType> notificationTypes = new List<NotificationType>();
                        for (int i = 0; i < request.NotificationsToBeRegistered.Count; i++)
                        {
                            HeaderCollection headers = request.NotificationsToBeRegistered[i];
                            notificationTypes.Add(NotificationType.FromHeaders(headers));
                        }
                        response = this.OnRegisterReceived(application, notificationTypes, mh.RequestInfo);
                        break;
                    case RequestType.NOTIFY:
                        Notification notification = Notification.FromHeaders(request.Headers);
                        mh.CallbackInfo.NotificationID = notification.ID;
                        response = this.OnNotifyReceived(notification, mh.CallbackInfo, mh.RequestInfo);
                        break;
                    case RequestType.SUBSCRIBE:
                        Subscriber subscriber = Subscriber.FromHeaders(request.Headers);
                        subscriber.IPAddress = mh.Socket.RemoteAddress.ToString();
                        subscriber.Key = new SubscriberKey(request.Key, subscriber.ID, request.Key.HashAlgorithm, request.Key.EncryptionAlgorithm);
                        response = this.OnSubscribeReceived(subscriber, mh.RequestInfo);
                        break;
                }


                ResponseType responseType = ResponseType.ERROR;
                if (response != null && response.IsOK)
                {
                    responseType = ResponseType.OK;
                    response.InResponseTo = request.Directive.ToString();
                }

                // no response
                if (response == null)
                    response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);

                AddServerHeaders(response);
                MessageBuilder mb = new MessageBuilder(responseType);
                HeaderCollection responseHeaders = response.ToHeaders();
                foreach (Header header in responseHeaders)
                {
                    mb.AddHeader(header);
                }
                // return any application-specific data headers that were received
                RequestData rd = RequestData.FromHeaders(request.Headers);
                AddRequestData(mb, rd);

                bool requestComplete = !mh.CallbackInfo.ShouldKeepConnectionOpen();
                mh.WriteResponse(mb, requestComplete);
            }
            catch (GrowlException gEx)
            {
                mh.WriteError(gEx.ErrorCode, gEx.Message, gEx.AdditionalInfo);
            }
            catch(Exception ex)
            {
                mh.WriteError(ErrorCode.INVALID_REQUEST, ex.Message);
            }
        }

        /// <summary>
        /// Writes back the GNTP response to the requesting application
        /// </summary>
        /// <param name="cbInfo">The <see cref="CallbackInfo"/> associated with the response</param>
        /// <param name="response">The <see cref="Response"/> to be written back</param>
        public void WriteResponse(CallbackInfo cbInfo, Response response)
        {
            if (!cbInfo.AlreadyResponded)
            {
                cbInfo.AlreadyResponded = true;
                MessageHandler mh = cbInfo.MessageHandler;
                GNTPRequest request = mh.Request;
                ResponseType responseType = ResponseType.ERROR;
                if (response != null)
                {
                    if (response.IsCallback) responseType = ResponseType.CALLBACK;
                    else if (response.IsOK) responseType = ResponseType.OK;
                }
                else
                {
                    response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
                }

                if (cbInfo.AdditionalInfo != null)
                {
                    foreach (KeyValuePair<string, string> item in cbInfo.AdditionalInfo)
                    {
                        response.CustomTextAttributes.Add(item.Key, item.Value);
                    }
                }

                AddServerHeaders(response);
                MessageBuilder mb = new MessageBuilder(responseType);
                HeaderCollection responseHeaders = response.ToHeaders();
                foreach (Header header in responseHeaders)
                {
                    mb.AddHeader(header);
                }
                // return any application-specific data headers that were received
                RequestData rd = RequestData.FromHeaders(request.Headers);
                AddRequestData(mb, rd);

                mh.WriteResponse(mb, true);
            }
         }

        /// <summary>
        /// Handles the <see cref="RegisterReceived"/> event
        /// </summary>
        /// <param name="application">The <see cref="Application"/> that is registering</param>
        /// <param name="notificationTypes">A list of <see cref="NotificationType"/>s being registered</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with the request</param>
        /// <returns><see cref="Response"/></returns>
        protected virtual Response OnRegisterReceived(Application application, List<NotificationType> notificationTypes, RequestInfo requestInfo)
        {
            LogInfo("REGISTER RECEIVED: {0}", application.Name);

            Response response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
            if (this.RegisterReceived != null)
            {
                response = this.RegisterReceived(application, notificationTypes, requestInfo);
            }
            return response;
        }

        /// <summary>
        /// Handles the <see cref="NotifyReceived"/> event
        /// </summary>
        /// <param name="notification">The <see cref="Notification"/> that was received</param>
        /// <param name="callbackInfo">The <see cref="CallbackInfo"/> for the notification</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with the request</param>
        /// <returns><see cref="Response"/></returns>
        protected virtual Response OnNotifyReceived(Notification notification, CallbackInfo callbackInfo, RequestInfo requestInfo)
        {
            LogInfo("NOTIFICATION RECEIVED: {0}", notification.Name);

            Response response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
            if (this.NotifyReceived != null)
            {
                response = this.NotifyReceived(notification, callbackInfo, requestInfo);
            }
            return response;
        }

        /// <summary>
        /// Handles the <see cref="SubscribeReceived"/> event
        /// </summary>
        /// <param name="subscriber">The <see cref="Subscriber"/> information</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with the request</param>
        /// <returns><see cref="Response"/></returns>
        protected virtual Response OnSubscribeReceived(Subscriber subscriber, RequestInfo requestInfo)
        {
            LogInfo("SUBSCRIBE RECEIVED: {0}", subscriber.Name);
            Response response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
            if (this.SubscribeReceived != null)
            {
                response = this.SubscribeReceived(subscriber, requestInfo);
            }
            return response;
        }

        /// <summary>
        /// Adds some custom server-specific headers to the outgoing response
        /// </summary>
        /// <param name="exObj">The <see cref="Response"/> or <see cref="Error"/> being returned</param>
        private void AddServerHeaders(ExtensibleObject exObj)
        {
            if(!exObj.CustomTextAttributes.ContainsKey("Message-Daemon"))
                exObj.CustomTextAttributes.Add("Message-Daemon", serverName);
            if(!exObj.CustomTextAttributes.ContainsKey("Timestamp"))
                exObj.CustomTextAttributes.Add("Timestamp", DateTime.Now.ToString());
        }


        /// <summary>
        /// Adds any application-specific headers to the message
        /// </summary>
        /// <param name="mb">The <see cref="MessageBuilder"/> used to construct the message</param>
        /// <param name="requestData">The <see cref="RequestData"/> that contains the application-specific data</param>
        private void AddRequestData(MessageBuilder mb, RequestData requestData)
        {
            if (requestData != null)
            {
                HeaderCollection headers = requestData.ToHeaders();
                foreach (Header header in headers)
                {
                    mb.AddHeader(header);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="AsyncSocket.DidClose"/> event
        /// </summary>
        /// <param name="sender">The <see cref="AsyncSocket"/> that disconnected</param>
        void newSocket_DidClose(AsyncSocket sender)
        {
            if (sender != null)
            {
                if (this.connectedHandlers.ContainsKey(sender))
                {
                    MessageHandler mh = this.connectedHandlers[sender];
                    this.connectedHandlers.Remove(sender);

                    if (this.connectedSockets.Contains(sender))
                    {
                        ConnectedSocket cs = this.connectedSockets[sender];
                        this.connectedSockets.Remove(sender);

                        if (cs.Socket != null)
                        {
                            cs.Socket.DidClose -= new AsyncSocket.SocketDidClose(newSocket_DidClose);
                            //cs.Socket.DidRead -= new AsyncSocket.SocketDidRead(mh.SocketDidRead);
                        }

                        cs = null;
                    }

                    if (mh != null)
                    {
                        mh.MessageParsed -= new MessageHandler.MessageHandlerMessageParsedEventHandler(mh_MessageParsed);
                        mh.Error -= new MessageHandler.MessageHandlerErrorEventHandler(mh_Error);
                        mh.SocketUsageComplete -= new MessageHandler.MessageHandlerSocketUsageCompleteEventHandler(mh_SocketUsageComplete);
                    }

                    mh = null;
                }

                sender = null;
            }
        }

        /// <summary>
        /// Checks to see if any open sockets have disconnected and cleans them up.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Event args</param>
        void socketCleanupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.socketCleanupTimer.Stop();
            int count = this.connectedSockets.Count;
            if (count > 0)
            {
                Queue<AsyncSocket> queue = new Queue<AsyncSocket>(this.connectedSockets.Count);
                lock (this.connectedSockets)
                {
                    foreach (ConnectedSocket cs in this.connectedSockets)
                    {
                        queue.Enqueue(cs.Socket);
                    }
                }

                while (queue.Count > 0)
                {
                    AsyncSocket socket = queue.Dequeue();
                    if (socket != null)
                    {
                        if (this.connectedSockets.Contains(socket))
                        {
                            bool safeToDisconnect = this.connectedSockets[socket].SafeToDisconnect;
                            if (safeToDisconnect && !socket.SmartConnected)
                            {
                                // socket is disconnected from the other end
                                socket.Close();
                            }
                        }
                    }
                }
            }
            if(this.isStarted) this.socketCleanupTimer.Start();
        }

        /// <summary>
        /// Called when a socket is done being used (for example, after all responses and callbacks
        /// have been returned to the caller).
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> that has completed</param>
        void mh_SocketUsageComplete(AsyncSocket socket)
        {
            if (this.connectedSockets.Contains(socket))
            {
                this.connectedSockets[socket].SafeToDisconnect = true;
            }
        }


        #region Logging

        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The values to log.</param>
        private void LogInfo(String format, params Object[] args)
        {
            String msg = String.Format(format, args);
            Log(LogMessageType.Information, msg);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The values to log.</param>
        private void LogError(String format, params Object[] args)
        {
            String msg = String.Format(format, args);
            Log(LogMessageType.Error, msg);
        }

        /// <summary>
        /// Log data received or sent
        /// </summary>
        /// <param name="msg">The data to log</param>
        private void LogData(String msg)
        {
            Log(LogMessageType.Data, msg);
        }

        /// <summary>
        /// Fires the <see cref="ServerMessage"/> event so calling code can handle the logging events
        /// </summary>
        /// <param name="type">The type of message being logged.</param>
        /// <param name="message">The log message.</param>
        private void Log(LogMessageType type, string message)
        {
            if (this.ServerMessage != null)
            {
                this.ServerMessage(this, type, message);
            }
        }

        /// <summary>
        /// The type of log message
        /// </summary>
        public enum LogMessageType
        {
            /// <summary>
            /// Informational
            /// </summary>
            Information,

            /// <summary>
            /// Raw data sent or received
            /// </summary>
            Data,

            /// <summary>
            /// Error
            /// </summary>
            Error
        }

        #endregion Logging

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Stop();

                    if (this.listenSocket != null)
                    {
                        this.listenSocket.DidAccept -= new AsyncSocket.SocketDidAccept(listenSocket_DidAccept);
                        this.listenSocket = null;
                    }

                    if (this.socketCleanupTimer != null)
                    {
                        this.socketCleanupTimer.Elapsed -= new System.Timers.ElapsedEventHandler(socketCleanupTimer_Elapsed);
                        this.socketCleanupTimer.Close();
                        this.socketCleanupTimer.Dispose();
                        this.socketCleanupTimer = null;
                    }

                    if (this.bonjour != null)
                    {
                        this.bonjour.Dispose();
                        this.bonjour = null;
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
