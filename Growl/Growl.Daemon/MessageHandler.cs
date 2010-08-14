using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Growl.Daemon
{
    /// <summary>
    /// Handles a single request/response/callback message transaction.
    /// </summary>
    /// <remarks>
    /// This class is responsible for most of the heavy lifting when dealing with GNTP messages.
    /// It parses the incoming request, passes off control for displaying the notification,
    /// and then builds and sends the response.
    /// </remarks>
    public class MessageHandler
    {
        /// <summary>
        /// Represents the method that will handle the <see cref="Error"/> event.
        /// </summary>
        public delegate void MessageHandlerErrorEventHandler(Error error);

        /// <summary>
        /// Represents the method that will handle the <see cref="MessageParsed"/> event.
        /// </summary>
        /// <param name="mh">The <see cref="MessageHandler"/> that parsed the message</param>
        public delegate void MessageHandlerMessageParsedEventHandler(MessageHandler mh);

        /// <summary>
        /// Represents the method that will handle the <see cref="SocketUsageComplete"/> event.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> whose usage is complete</param>
        public delegate void MessageHandlerSocketUsageCompleteEventHandler(AsyncSocket socket);

        /// <summary>
        /// Occurs when the MessageHandler is going to return an Error response
        /// </summary>
        public event MessageHandlerErrorEventHandler Error;

        /// <summary>
        /// Occurs when the request has been successfully parsed
        /// </summary>
        public event MessageHandlerMessageParsedEventHandler MessageParsed;

        /// <summary>
        /// Occurs when the socket usage is complete (all response and callbacks have been written)
        /// </summary>
        public event MessageHandlerSocketUsageCompleteEventHandler SocketUsageComplete;

        private const long ACCEPT_TAG = 0;
        private const long USAGECOMPLETE_TAG = 9999;

        private const long RESPONSE_SUCCESS_TAG = 97;
        private const long RESPONSE_ERROR_TAG = 98;
        private const long RESPONSE_TIMEOUT_TAG = 99;

        private const int TIMEOUT_INITIALREAD = -1;
        private const int TIMEOUT_FLASHPOLICYREQUEST = -1;
        private const int TIMEOUT_FLASHPOLICYRESPONSE = -1;
        private const int TIMEOUT_ERROR_RESPONSE = -1;
        private const int TIMEOUT_USAGECOMPLETE = -1;

        /// <summary>
        /// Seperator line for log files
        /// </summary>
        private const string SEPERATOR = "\r\n\r\n-----------------------------------------------------\r\n\r\n";

        /// <summary>
        /// The parsed request
        /// </summary>
        GNTPRequest request;

        /// <summary>
        /// The class responsible for reading and parsing incoming requests
        /// </summary>
        GNTPRequestReader requestReader;

        /// <summary>
        /// The name of the server
        /// </summary>
        private string serverName;

        /// <summary>
        /// The path to the folder where log files are written
        /// </summary>
        private string logFolder;

        /// <summary>
        /// Indicates if logging is enabled or not
        /// </summary>
        private bool loggingEnabled;

        /// <summary>
        /// Indicates if remote notifications are allowed
        /// </summary>
        private bool allowNetworkNotifications = false;

        /// <summary>
        /// Indicates if notifications originating from a browser are allowed
        /// </summary>
        private bool allowBrowerConnections = false;

        /// <summary>
        /// Indicates if client are allowed to subscribe to notifications from this server
        /// </summary>
        private bool allowSubscriptions = false;

        /// <summary>
        /// The list of valid passwords
        /// </summary>
        private PasswordManager passwordManager;

        /// <summary>
        /// Indicates if the request must supply a password
        /// </summary>
        private bool passwordRequired = true;

        /// <summary>
        /// The callback info associated with the request
        /// </summary>
        private CallbackInfo callbackInfo;

        /// <summary>
        /// The request info associated with the request
        /// </summary>
        private RequestInfo requestInfo;

        /// <summary>
        /// The socket used to receive the request and send the response and callback
        /// </summary>
        private AsyncSocket socket;


        /// <summary>
        /// Type initializer for the MessageHandler class
        /// </summary>
        static MessageHandler()
        {
            // we dont do anything here anymore, but lets keep it around for now...
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="passwordManager">A list of valid passwords</param>
        /// <param name="passwordRequired">Indicates if the request must contain a password</param>
        /// <param name="logFolder">The path to the folder where log files are written</param>
        /// <param name="loggingEnabled">Indicates if logging is enabled or not</param>
        /// <param name="allowNetworkNotifications">Indicates if notifications from remote machines are allowed</param>
        /// <param name="allowBrowerConnections">Indicates if notifications from browsers are allowed</param>
        /// <param name="allowSubscriptions">Indicates if clients are allowed to subscribe to notifications from this server</param>
        public MessageHandler(string serverName, PasswordManager passwordManager, bool passwordRequired, string logFolder, bool loggingEnabled, bool allowNetworkNotifications, bool allowBrowerConnections, bool allowSubscriptions)
        {
            this.serverName = serverName;
            this.passwordManager = passwordManager;
            this.passwordRequired = passwordRequired;
            this.logFolder = logFolder;
            this.loggingEnabled = loggingEnabled;
            this.allowNetworkNotifications = allowNetworkNotifications;
            this.allowBrowerConnections = allowBrowerConnections;
            this.allowSubscriptions = allowSubscriptions;

            this.serverName = serverName;
            this.callbackInfo = new CallbackInfo();
            this.requestInfo = new RequestInfo();
        }

        /// <summary>
        /// Gets the <see cref="CallbackInfo"/> associated with the request.
        /// </summary>
        /// <value><see cref="CallbackInfo"/></value>
        public CallbackInfo CallbackInfo
        {
            get
            {
                return this.callbackInfo;
            }
        }

        /// <summary>
        /// Gets the <see cref="RequestInfo"/> associated with the request.
        /// </summary>
        /// <value><see cref="RequestInfo"/></value>
        public RequestInfo RequestInfo
        {
            get
            {
                return this.requestInfo;
            }
        }

        /// <summary>
        /// Gets the socket used for reading the request and writing the response and any callbacks.
        /// </summary>
        /// <value><see cref="AsyncSocket"/></value>
        public AsyncSocket Socket
        {
            get
            {
                return this.socket;
            }
        }

        /// <summary>
        /// The parsed GNTP request.
        /// </summary>
        /// <value><see cref="GNTPRequest"/></value>
        /// <remarks>
        /// This property will only be populated once the request has successfully been parsed.
        /// </remarks>
        public GNTPRequest Request
        {
            get
            {
                return this.request;
            }
        }

        /// <summary>
        /// Performs an initial read of the received data to see if it looks like a
        /// valid request.
        /// </summary>
        /// <param name="socket"><see cref="AsyncSocket"/></param>
        public void InitialRead(AsyncSocket socket)
        {
            this.socket = socket;
            socket.DidReadTimeout += new AsyncSocket.SocketDidReadTimeout(socket_DidReadTimeout);

            // log where this notification came from
            bool isLocal = System.Net.IPAddress.IsLoopback(socket.RemoteAddress);
            bool isLAN = Growl.CoreLibrary.IPUtilities.IsInSameSubnet(socket.LocalAddress, socket.RemoteAddress);
            this.requestInfo.SaveHandlingInfo(String.Format("Notification Origin: {0} [{1}]", socket.RemoteAddress.ToString(), (isLocal ? "LOCAL MACHINE" : (isLAN ? "LAN - same subnet" : "REMOTE NETWORK"))));

            // read the first 4 bytes so we know what kind of request this is (GNTP, GNTP over WebSocket, Flash Policy request, etc)
            socket.DidRead += new AsyncSocket.SocketDidRead(this.SocketDidReadIndicatorBytes);
            socket.Read(4, TIMEOUT_INITIALREAD, ACCEPT_TAG);
        }

        /// <summary>
        /// Handles the socket's DidRead event after reading only the first four bytes of data.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/></param>
        /// <param name="readBytes">Array of <see cref="byte"/>s that were read</param>
        /// <param name="tag">The tag identifying the read operation</param>
        private void SocketDidReadIndicatorBytes(AsyncSocket socket, byte[] readBytes, long tag)
        {
            // remove this event handler since we dont need it any more
            socket.DidRead -= new AsyncSocket.SocketDidRead(this.SocketDidReadIndicatorBytes);

            Data data = new Data(readBytes);
            string s = data.ToString();

            if (tag == ACCEPT_TAG)
            {
                if (s == FlashPolicy.REQUEST_INDICATOR)
                {
                    GNTPFlashSocketReader gfsr = new GNTPFlashSocketReader(socket, allowBrowerConnections);
                    gfsr.Read(readBytes);
                }
                else if (s == WebSocketHandshakeHandler.REQUEST_INDICATOR)
                {
                    // this is a GNTP over WebSocket request, so we have to do the WebSocket handshake first
                    socket.Tag = readBytes;
                    WebSocketHandshakeHandler wshh = new WebSocketHandshakeHandler(socket, "*", "ws://localhost:23053");
                    wshh.DoHandshake(delegate()
                    {
                        // now pass off to the GNTPWebSocketReader (which is just a normal GNTPSocketReader that can deal with the WebSocket framing of packets)
                        GNTPWebSocketReader gwsr = new GNTPWebSocketReader(socket, passwordManager, passwordRequired, allowNetworkNotifications, allowBrowerConnections, allowSubscriptions, this.requestInfo);
                        this.requestReader = gwsr;
                        gwsr.MessageParsed += new GNTPRequestReader.GNTPRequestReaderMessageParsedEventHandler(requestReader_MessageParsed);
                        gwsr.Error += new GNTPRequestReader.GNTPRequestReaderErrorEventHandler(requestReader_Error);
                        gwsr.Read(readBytes);
                    });
                }
                else
                {
                    // this is a normal GNTP/TCP connection, so handle it as such
                    GNTPSocketReader gsr = new GNTPSocketReader(socket, passwordManager, passwordRequired, allowNetworkNotifications, allowBrowerConnections, allowSubscriptions, this.requestInfo);
                    this.requestReader = gsr;
                    gsr.MessageParsed += new GNTPRequestReader.GNTPRequestReaderMessageParsedEventHandler(requestReader_MessageParsed);
                    gsr.Error += new GNTPRequestReader.GNTPRequestReaderErrorEventHandler(requestReader_Error);
                    gsr.Read(readBytes);
                }
            }
            else
            {
                WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.MALFORMED_REQUEST);
            }
        }

        /// <summary>
        /// Handles the requestReader's <see cref="GNTPRequestReader.MessageParsed"/> event
        /// </summary>
        /// <param name="request">The parsed <see cref="GNTPRequest"/></param>
        void requestReader_MessageParsed(GNTPRequest request)
        {
            this.request = request;
            OnMessageParsed(this.socket);
        }

        /// <summary>
        /// Handles the requestReader's <see cref="GNTPRequestReader.Error"/> event
        /// </summary>
        /// <param name="error">The <see cref="Error"/> information</param>
        void requestReader_Error(Error error)
        {
            WriteError(this.Socket, error);
        }

        /// <summary>
        /// Handles the socket's <see cref="AsyncSocket.DidReadTimeout"/> event
        /// </summary>
        /// <param name="sender">The <see cref="AsyncSocket"/></param>
        /// <returns>Always returns <c>true</c></returns>
        bool socket_DidReadTimeout(AsyncSocket sender)
        {
            sender.DidReadTimeout -= new AsyncSocket.SocketDidReadTimeout(socket_DidReadTimeout);
            WriteError(sender, ErrorCode.TIMED_OUT, ErrorDescription.TIMED_OUT);
            return true;
        }

        /// <summary>
        /// Writes the response back to the original sender.
        /// </summary>
        /// <param name="mb">The <see cref="MessageBuilder"/> containing the data to write</param>
        /// <param name="requestComplete">Indicates if this completes the transaction (all responses and callbacks have been written)</param>
        public void WriteResponse(MessageBuilder mb, bool requestComplete)
        {
            Write(socket, mb, TIMEOUT_ERROR_RESPONSE, RESPONSE_SUCCESS_TAG, false, requestComplete);
        }

        /// <summary>
        /// Writes an error response back to the original sender.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="args">Any additional data to include in the error message</param>
        public void WriteError(int errorCode, string errorMessage, params object[] args)
        {
            WriteError(this.socket, errorCode, errorMessage, args);
        }

        /// <summary>
        /// Writes an error response back to the original sender.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> used to write the response</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="args">Any additional data to include in the error message</param>
        private void WriteError(AsyncSocket socket, int errorCode, string errorMessage, params object[] args)
        {
            if (args != null)
            {
                foreach (object arg in args)
                {
                    errorMessage += String.Format(" ({0})", arg);
                }
            }
            Error error = new Error(errorCode, errorMessage);

            WriteError(socket, error);
        }

        /// <summary>
        /// Writes an error response back to the original sender.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> used to write the response</param>
        /// <param name="error">The error</param>
        private void WriteError(AsyncSocket socket, Error error)
        {
            if (this.Error != null)
            {
                this.Error(error);
            }

            HeaderCollection headers = error.ToHeaders();
            MessageBuilder mb = new MessageBuilder(ResponseType.ERROR);
            foreach (Header header in headers)
            {
                mb.AddHeader(header);
            }

            Write(socket, mb, TIMEOUT_ERROR_RESPONSE, RESPONSE_ERROR_TAG, true, true);
        }

        /// <summary>
        /// Writes data to the specified socket.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> to write the data to</param>
        /// <param name="mb">The <see cref="MessageBuilder"/> containing the data to write</param>
        /// <param name="timeout">The socket write timeout value</param>
        /// <param name="tag">The tag that will identify the write operation (can be referenced in the socket's DidWrite event)</param>
        /// <param name="disconnectAfterWriting">Indicates if the server should disconnect the socket after writing the data</param>
        /// <param name="requestComplete">Indicates if the request is complete once the data is written</param>
        protected virtual void Write(AsyncSocket socket, MessageBuilder mb, int timeout, long tag, bool disconnectAfterWriting, bool requestComplete)
        {
            //Console.WriteLine(alreadyReceived.ToString());

            byte[] bytes = mb.GetBytes();
            mb = null;
            FinalWrite(socket, bytes, timeout, tag, disconnectAfterWriting, requestComplete);
        }

        /// <summary>
        /// Performs the actual writing of data to the socket. Used by all other Write* methods.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> to write the data to</param>
        /// <param name="bytes">The bytes to write to the socket</param>
        /// <param name="timeout">The socket write timeout value</param>
        /// <param name="tag">The tag that will identify the write operation (can be referenced in the socket's DidWrite event)</param>
        /// <param name="disconnectAfterWriting">Indicates if the server should disconnect the socket after writing the data</param>
        /// <param name="requestComplete">Indicates if the request is complete once the data is written</param>
        protected void FinalWrite(AsyncSocket socket, byte[] bytes, int timeout, long tag, bool disconnectAfterWriting, bool requestComplete)
        {
            Data data = new Data(bytes);
            Log(data);

            // give any custom readers the change to modify the output before we send it (especially useful for WebSockets that need to frame their data)
            if (this.requestReader != null)
                this.requestReader.BeforeResponse(ref bytes);

            // send the data
            socket.Write(bytes, timeout, tag);

            // if we are the ones disconnecting, do it now.
            // otherwise, we need to know if the request is complete (all callbacks are done)
            // if so, we must trigger another read attempt so we can be notified of the other end's decision to disconnect
            // if not, we can just leave the socket alone until the request is complete
            if (disconnectAfterWriting)
                socket.CloseAfterWriting();
            else if (requestComplete)
            {
                socket.CloseAfterWriting();
                OnSocketUsageComplete(socket);
            }

            /* TODO: as3growl testing ONLY
            InitialRead(socket);
             * */
        }

        /// <summary>
        /// Logs the specified data.
        /// </summary>
        /// <param name="data">The <see cref="Data"/> to log</param>
        public void Log(Data data)
        {
            try
            {
                if (this.loggingEnabled && !String.IsNullOrEmpty(this.logFolder))
                {
                    string filename = String.Format(@"GNTP_{0}.txt", this.requestInfo.RequestID);
                    string filepath = PathUtility.Combine(this.logFolder, filename);
                    bool exists = System.IO.File.Exists(filepath);
                    if (!exists)
                    {
                        // log the initial request/response
                        System.IO.StreamWriter w = System.IO.File.CreateText(filepath);
                        using (w)
                        {
                            w.Write("Timestamp: {0}\r\n", DateTime.Now.ToString());
                            foreach (string item in this.requestInfo.HandlingInfo)
                            {
                                w.Write(String.Format("{0}\r\n", item));
                            }
                            w.Write(SEPERATOR);

                            if (this.requestReader != null)
                            {
                                w.Write(this.requestReader.ReceivedData);

                                if (!String.IsNullOrEmpty(this.requestReader.DecryptedData))
                                {
                                    w.Write(SEPERATOR);
                                    w.Write(this.requestReader.DecryptedData);
                                }
                            }

                            w.Write(SEPERATOR);
                            w.Write(data.ToString());
                            w.Close();
                            this.requestInfo.HandlingInfo.Clear();
                        }
                    }
                    else
                    {
                        // this is for logging callback data
                        System.IO.StreamWriter w = new System.IO.StreamWriter(filepath, true);
                        using (w)
                        {
                            w.Write(SEPERATOR);
                            w.Write("Timestamp: {0}\r\n", DateTime.Now.ToString());
                            foreach (string item in this.requestInfo.HandlingInfo)
                            {
                                w.Write(String.Format("{0}\r\n", item));
                            }
                            w.Write(SEPERATOR);
                            w.Write(data.ToString());
                            w.Close();
                        }
                    }
                }
            }
            catch
            {
                // suppress logging exceptions
            }
        }

        /// <summary>
        /// Called when the request is successfully parsed.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> that the request came in on</param>
        private void OnMessageParsed(AsyncSocket socket)
        {
            // handle request information
            this.requestInfo.ReceivedFrom = socket.RemoteAddress.ToString();
            this.requestInfo.ReceivedBy = String.Format("{0} ({1})", Environment.MachineName, GrowlServer.ServerID);
            this.requestInfo.ReceivedWith = this.serverName;

            // see if this machine has already processed this notification
            // (this prevents endless loops caused by forwarding messages back to the original forwarder)
            bool alreadyProcessed = CheckAlreadyProcessed();
            if (alreadyProcessed)
            {
                WriteError(socket, ErrorCode.ALREADY_PROCESSED, ErrorDescription.ALREADY_PROCESSED);
                return;
            }

            // handle callback information
            this.callbackInfo.Context = this.Request.CallbackContext;
            this.callbackInfo.MessageHandler = this;
            this.callbackInfo.RequestInfo = requestInfo;

            if (this.MessageParsed != null)
            {
                this.MessageParsed(this);
            }
            else
            {
                // no handler - return some kind of error? (this should never really happen)
                WriteError(socket, ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
            }
        }

        /// <summary>
        /// Called when the socket usage is complete (all responses and callbacks have
        /// been written).
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> used in the transaction</param>
        protected void OnSocketUsageComplete(AsyncSocket socket)
        {
            // kick of one final read so we know when the socket closes
            socket.Read(TIMEOUT_USAGECOMPLETE, USAGECOMPLETE_TAG);

            if (this.SocketUsageComplete != null)
            {
                this.SocketUsageComplete(socket);
            }
        }

        /// <summary>
        /// Checks the 'Received' headers to see if this machine has already handled this request
        /// </summary>
        /// <returns>
        /// <c>true</c> if this machine has already handled this request;
        /// <c>false</c> otherwise
        /// </returns>
        /// <remarks>
        /// This check is not 100% perfect. All we can check for is if the machine name is in the Received header value.
        /// It could appear in the Received header even if it did not already process it (similarly name machine, etc), 
        /// producing a false positive. Just be aware.
        /// </remarks>
        private bool CheckAlreadyProcessed()
        {
            if (this.requestInfo.PreviousReceivedHeaders != null && this.requestInfo.PreviousReceivedHeaders.Count > 0)
            {
                foreach (Header header in this.requestInfo.PreviousReceivedHeaders)
                {
                    if (header.Value.IndexOf(this.requestInfo.ReceivedBy) >= 0)
                    {
                        this.requestInfo.SaveHandlingInfo("NOT PROCESSED - NOTIFICATION WAS FORWARDED BACK TO THIS MACHINE");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
