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

        //private static Regex regExMessageHeader = new Regex(@"(GNTP/)(?<Version>(.\..))\s+(?<Directive>(\S+))[\r\n]");

        /// <summary>
        /// Regex used to parse GNTP headers for local requests (dont require password)
        /// </summary>
        private static Regex regExMessageHeader_Local = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))|((?<EncryptionAlgorithm>\S+)))\s*[\r\n]");

        /// <summary>
        /// Regex used to parse GNTP headers for non-local requests (password required)
        /// </summary>
        private static Regex regExMessageHeader_Remote = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))\s+|((?<EncryptionAlgorithm>\S+)\s+))(?<KeyHashAlgorithm>(\S+)):(?<KeyHash>(\S+))\.(?<Salt>(\S+))\s*[\r\n]");


        private const long ACCEPT_TAG = 0;
        private const long FLASH_POLICY_REQUEST_TAG = 1;
        private const long FLASH_POLICY_RESPONSE_TAG = 2;
        private const long GNTP_IDENTIFIER_TAG = 3;
        private const long HEADER_TAG = 4;
        private const long NOTIFICATION_TYPE_TAG = 5;
        private const long RESOURCE_HEADER_TAG = 6;
        private const long RESOURCE_TAG = 7;
        private const long ENCRYPTED_HEADERS_TAG = 8;
        private const long USAGECOMPLETE_TAG = 9;

        private const long RESPONSE_SUCCESS_TAG = 97;
        private const long RESPONSE_ERROR_TAG = 98;
        private const long RESPONSE_TIMEOUT_TAG = 99;

        /*
        private const int TIMEOUT_INITIALREAD = 1 * 1000;
        private const int TIMEOUT_FLASHPOLICYREQUEST = 1 * 1000;
        private const int TIMEOUT_GNTP_HEADER = 1 * 1000;
        private const int TIMEOUT_GNTP_BINARY = 5 * 60 * 1000; // five minutes
        private const int TIMEOUT_ENCRYPTED_HEADERS = 5 * 1000;
        private const int TIMEOUT_FLASHPOLICYRESPONSE = -1;
        private const int TIMEOUT_ERROR_RESPONSE = -1;
         * */

        private const int TIMEOUT_INITIALREAD = -1;
        private const int TIMEOUT_FLASHPOLICYREQUEST = -1;
        private const int TIMEOUT_GNTP_HEADER = -1;
        private const int TIMEOUT_GNTP_BINARY = -1;
        private const int TIMEOUT_ENCRYPTED_HEADERS = -1;
        private const int TIMEOUT_FLASHPOLICYRESPONSE = -1;
        private const int TIMEOUT_ERROR_RESPONSE = -1;
        private const int TIMEOUT_USAGECOMPLETE = -1;

        /// <summary>
        /// Message logged when a request is only partially read before encountering an error
        /// </summary>
        private const string PARTIAL_MESSAGE_NOTICE = "<Additional request data may not have been read after the message was invalidated.>";

        /// <summary>
        /// Seperator line for log files
        /// </summary>
        private const string SEPERATOR = "\r\n\r\n-----------------------------------------------------\r\n\r\n";

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
        /// Indicates if local requests must supply a password
        /// </summary>
        private bool requireLocalPassword;

        /// <summary>
        /// Indicates if remote notifications are allowed
        /// </summary>
        private bool allowNetworkNotifications = false;

        /// <summary>
        /// Indicates if notifications originating from a Flash client are allowed
        /// </summary>
        private bool allowFlash = false;

        /// <summary>
        /// Indicates if client are allowed to subscribe to notifications from this server
        /// </summary>
        private bool allowSubscriptions = false;

        /// <summary>
        /// The key used to validate and encrypt the message
        /// </summary>
        private Key key;

        /// <summary>
        /// The list of valid passwords
        /// </summary>
        private PasswordManager passwordManager;

        /// <summary>
        /// The hex-encoded IV value from the request
        /// </summary>
        private string ivHex;

        /// <summary>
        /// The actual IV bytes from the request
        /// </summary>
        private byte[] iv;

        /// <summary>
        /// Indicates if the request originated on the local machine or not
        /// </summary>
        private bool isLocal = false;

        /// <summary>
        /// Contains all of the data from the request that has already been read
        /// </summary>
        private StringBuilder alreadyReceived;

        /// <summary>
        /// The collection of headers parsed from the current request
        /// </summary>
        private HeaderCollection headers;

        /// <summary>
        /// The version of the GNTP request
        /// </summary>
        private string version;

        /// <summary>
        /// The type of GNTP request
        /// </summary>
        private RequestType directive;

        /// <summary>
        /// The type of hashing algorithm used in the request
        /// </summary>
        private Cryptography.HashAlgorithmType keyHashAlgorithm;

        /// <summary>
        /// The type of encryption used in the request
        /// </summary>
        private Cryptography.SymmetricAlgorithmType encryptionAlgorithm;

        /// <summary>
        /// The name of the application sending the request
        /// </summary>
        private string applicationName;

        /// <summary>
        /// For REGISTER requests, the number of notifications expected to be registered
        /// </summary>
        private int expectedNotifications = 0;

        /// <summary>
        /// For REGISTER requests, the number of notifcations still to be registered
        /// </summary>
        private int expectedNotificationsRemaining = 0;

        /// <summary>
        /// For REGISTER requests, the index of the current notification type
        /// </summary>
        private int currentNotification = 0;

        /// <summary>
        /// A collection of the groups of headers for each notification type to be registered
        /// </summary>
        private List<HeaderCollection> notificationsToBeRegistered;

        /// <summary>
        /// The number of binary pointers in the request
        /// </summary>
        private int pointersExpected = 0;

        /// <summary>
        /// The number of binary pointers still to be found in the request
        /// </summary>
        private int pointersExpectedRemaining = 0;

        /// <summary>
        /// The index of the current binary pointer
        /// </summary>
        private int currentPointer = 0;

        /// <summary>
        /// A collection of all pointers in the request
        /// </summary>
        private List<Pointer> pointers;

        /// <summary>
        /// The callback info associated with the request
        /// </summary>
        private CallbackInfo callbackInfo;

        /// <summary>
        /// The callback data associated with the request
        /// </summary>
        private string callbackData;

        /// <summary>
        /// The callback data type associated with the request
        /// </summary>
        private string callbackDataType;

        /// <summary>
        /// The callback url associated with the request
        /// </summary>
        private string callbackUrl;

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
        /// <param name="isLocal">Indicates if the request originated on the local machine or not</param>
        /// <param name="logFolder">The path to the folder where log files are written</param>
        /// <param name="loggingEnabled">Indicates if logging is enabled or not</param>
        /// <param name="requireLocalPassword">Indicates if local requests must supply a password (normally, local requests are not required to supply a password)</param>
        /// <param name="allowNetworkNotifications">Indicates if notifications from remote machines are allowed</param>
        /// <param name="allowFlash">Indicates if notifications from Flash clients are allowed</param>
        /// <param name="allowSubscriptions">Indicates if clients are allowed to subscribe to notifications from this server</param>
        public MessageHandler(string serverName, PasswordManager passwordManager, bool isLocal, string logFolder, bool loggingEnabled, bool requireLocalPassword, bool allowNetworkNotifications, bool allowFlash, bool allowSubscriptions)
        {
            this.serverName = serverName;
            this.passwordManager = passwordManager;
            this.isLocal = isLocal;
            this.logFolder = logFolder;
            this.loggingEnabled = loggingEnabled;
            this.requireLocalPassword = requireLocalPassword;
            this.allowNetworkNotifications = allowNetworkNotifications;
            this.allowFlash = allowFlash;
            this.allowSubscriptions = allowSubscriptions;
        }

        /// <summary>
        /// Gets the name of the application sending the request
        /// </summary>
        /// <value>string</value>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
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
        /// Gets the type of the request
        /// </summary>
        /// <value><see cref="RequestType"/></value>
        public RequestType Directive
        {
            get
            {
                return directive;
            }
        }

        /// <summary>
        /// Gets the list of headers parsed from the request.
        /// </summary>
        /// <value><see cref="HeaderCollection"/></value>
        public HeaderCollection Headers
        {
            get
            {
                return this.headers;
            }
        }

        /// <summary>
        /// Gets the collection of groups of headers for all notifications to be registered.
        /// </summary>
        /// <value><see cref="List{HeaderCollection}"/></value>
        public List<HeaderCollection> NotificationsToBeRegistered
        {
            get
            {
                return this.notificationsToBeRegistered;
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
        /// Gets the <see cref="Key"/> used to validate and encrypt the request
        /// </summary>
        /// <value><see cref="Key"/></value>
        internal Key Key
        {
            get
            {
                return this.key;
            }
        }

        /// <summary>
        /// Gets the type of hash algorithm used in the request.
        /// </summary>
        /// <value><see cref="Cryptography.HashAlgorithmType"/></value>
        internal Cryptography.HashAlgorithmType KeyHashAlgorithm
        {
            get
            {
                return this.keyHashAlgorithm;
            }
        }

        /// <summary>
        /// Gets the type of encryption algorithm used in the request
        /// </summary>
        /// <value><see cref="Cryptography.SymmetricAlgorithmType"/></value>
        internal Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                return this.encryptionAlgorithm;
            }
        }


        /// <summary>
        /// Performs an initial read of the received data to see if it looks like a
        /// valid request.
        /// </summary>
        /// <param name="socket"><see cref="AsyncSocket"/></param>
        public void InitialRead(AsyncSocket socket)
        {
            this.alreadyReceived = new StringBuilder();
            this.headers = new HeaderCollection();
            this.notificationsToBeRegistered = new List<HeaderCollection>();
            this.pointers = new List<Pointer>();
            this.callbackInfo = new CallbackInfo();
            this.requestInfo = new RequestInfo();

            socket.DidReadTimeout += new AsyncSocket.SocketDidReadTimeout(socket_DidReadTimeout);

            socket.Read(1, TIMEOUT_INITIALREAD, ACCEPT_TAG);
        }

        /// <summary>
        /// Handles the socket's DidReadTimeout event.
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
        /// Handles the socket's DidWrite event.
        /// </summary>
        /// <param name="sender">The <see cref="AsyncSocket"/></param>
        /// <param name="tag">The tag identifying the write operation</param>
        public void SocketDidWrite(AsyncSocket sender, long tag)
        {
            //Console.WriteLine("did write - " + tag.ToString());
        }

        /// <summary>
        /// Handles the socket's DidRead event.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/></param>
        /// <param name="readBytes">Array of <see cref="byte"/>s that were read</param>
        /// <param name="tag">The tag identifying the read operation</param>
        public void SocketDidRead(AsyncSocket socket, byte[] readBytes, long tag)
        {
            try
            {
                Data data = new Data(readBytes);
                string s = data.ToString();
                alreadyReceived.Append(s);

                if (tag == ACCEPT_TAG)
                {
                    if (s == FlashPolicy.REQUEST_INDICATOR)
                    {
                        socket.Read(FlashPolicy.REQUEST.Length - s.Length, TIMEOUT_FLASHPOLICYREQUEST, FLASH_POLICY_REQUEST_TAG);
                    }
                    else
                    {
                        socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, GNTP_IDENTIFIER_TAG);
                    }
                }

                else if (tag == FLASH_POLICY_REQUEST_TAG)
                {
                    string request = alreadyReceived.ToString();
                    if (request == FlashPolicy.REQUEST)
                    {
                        if (this.allowFlash)
                        {
                            socket.Write(FlashPolicy.ResponseBytes, TIMEOUT_FLASHPOLICYRESPONSE, FLASH_POLICY_RESPONSE_TAG);
                            socket.CloseAfterWriting();
                        }
                        else
                        {
                            WriteError(socket, ErrorCode.NOT_AUTHORIZED, ErrorDescription.FLASH_CONNECTIONS_NOT_ALLOWED);
                        }
                    }
                    else
                    {
                        WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.UNRECOGNIZED_REQUEST);
                    }
                }

                else if (tag == GNTP_IDENTIFIER_TAG)
                {
                    string line = alreadyReceived.ToString();
                    Match match = ParseGNTPHeaderLine(line, this.isLocal);

                    if (match.Success)
                    {
                        this.version = match.Groups["Version"].Value;
                        if (version == MessageParser.GNTP_SUPPORTED_VERSION)
                        {
                            string d = match.Groups["Directive"].Value;
                            if (Enum.IsDefined(typeof(RequestType), d))
                            {
                                this.directive = (RequestType) Enum.Parse(typeof(RequestType), d);

                                // check for supported but not allowed requests
                                if (this.directive == RequestType.SUBSCRIBE && !this.allowSubscriptions)
                                {
                                    WriteError(socket, ErrorCode.NOT_AUTHORIZED, ErrorDescription.SUBSCRIPTIONS_NOT_ALLOWED);
                                }
                                else
                                {
                                    this.encryptionAlgorithm = Cryptography.GetEncryptionType(match.Groups["EncryptionAlgorithm"].Value);
                                    this.ivHex = (match.Groups["IV"] != null ? match.Groups["IV"].Value : null);
                                    if (!String.IsNullOrEmpty(this.ivHex)) this.iv = Cryptography.HexUnencode(this.ivHex);
                                    string keyHash = match.Groups["KeyHash"].Value.ToUpper();

                                    bool authorized = false;
                                    // Any of the following criteria require a password:
                                    //    1. the request did not originate on the local machine
                                    //    2. it is a SUBSCRIBE request (all subscriptions require a password)
                                    //    3. the user's preferences require even local requests to supply a password
                                    // Additionally, even if a password is not required, it will be validated if the 
                                    // sending appplication includes one
                                    string errorDescription = ErrorDescription.INVALID_KEY;
                                    if (!this.isLocal || this.directive == RequestType.SUBSCRIBE || this.requireLocalPassword || !String.IsNullOrEmpty(keyHash))
                                    {
                                        if (String.IsNullOrEmpty(keyHash))
                                        {
                                            errorDescription = ErrorDescription.MISSING_KEY;
                                        }
                                        else
                                        {
                                            string keyHashAlgorithmType = match.Groups["KeyHashAlgorithm"].Value;
                                            this.keyHashAlgorithm = Cryptography.GetKeyHashType(keyHashAlgorithmType);
                                            string salt = match.Groups["Salt"].Value.ToUpper();
                                            authorized = this.passwordManager.IsValid(keyHash, salt, this.keyHashAlgorithm, this.encryptionAlgorithm, out this.key);
                                        }
                                    }
                                    else
                                    {
                                        authorized = true;
                                        this.key = Key.None;
                                    }

                                    if (authorized)
                                    {
                                        if (this.encryptionAlgorithm == Cryptography.SymmetricAlgorithmType.PlainText)
                                            socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, HEADER_TAG);
                                        else
                                            socket.Read(AsyncSocket.CRLFCRLFData, TIMEOUT_ENCRYPTED_HEADERS, ENCRYPTED_HEADERS_TAG);
                                    }
                                    else
                                    {
                                        WriteError(socket, ErrorCode.NOT_AUTHORIZED, errorDescription);
                                    }
                                }
                            }
                            else
                            {
                                WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.UNSUPPORTED_DIRECTIVE, directive);
                            }
                        }
                        else
                        {
                            WriteError(socket, ErrorCode.UNKNOWN_PROTOCOL_VERSION, ErrorDescription.UNSUPPORTED_VERSION, version);
                        }
                    }
                    else
                    {
                        WriteError(socket, ErrorCode.UNKNOWN_PROTOCOL, ErrorDescription.MALFORMED_REQUEST);
                    }
                }

                else if (tag == HEADER_TAG)
                {
                    if (s == MessageParser.BLANK_LINE)
                    {
                        // if this is a REGISTER message, check Notifications-Count value
                        // to see how many notification sections to expect
                        if (this.directive == RequestType.REGISTER)
                        {
                            if (this.expectedNotifications > 0)
                            {
                                socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, NOTIFICATION_TYPE_TAG);
                            }
                            else
                            {
                                // a REGISTER request with no notifications is not valid
                                WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.NO_NOTIFICATIONS_REGISTERED);
                            }
                        }
                        else
                        {
                            // otherwise, check the number of resource pointers we got and start reading those
                            this.pointersExpected = GetNumberOfPointers();
                            if (this.pointersExpected > 0)
                            {
                                this.pointersExpectedRemaining = this.pointersExpected;
                                this.currentPointer = 1;
                                socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                            }
                            else
                            {
                                OnMessageParsed(socket);
                            }
                        }
                    }
                    else
                    {
                        Header header = Header.ParseHeader(s);
                        if (header != null)
                        {
                            bool addHeader = true;
                            if (header.Name == Header.APPLICATION_NAME)
                            {
                                this.applicationName = header.Value;
                            }
                            if (header.Name == Header.NOTIFICATIONS_COUNT)
                            {
                                this.expectedNotifications = Convert.ToInt32(header.Value);
                                this.expectedNotificationsRemaining = this.expectedNotifications;
                                this.currentNotification = 1;
                            }
                            if (header.Name == Header.NOTIFICATION_CALLBACK_CONTEXT)
                            {
                                this.callbackData = header.Value;
                            }
                            if (header.Name == Header.NOTIFICATION_CALLBACK_CONTEXT_TYPE)
                            {
                                this.callbackDataType = header.Value;
                            }
                            if (header.Name == Header.NOTIFICATION_CALLBACK_TARGET || header.Name == Header.NOTIFICATION_CALLBACK_CONTEXT_TARGET)   // left in for compatibility
                            {
                                this.callbackUrl = header.Value;
                            }
                            if (header.Name == Header.RECEIVED)
                            {
                                this.requestInfo.PreviousReceivedHeaders.Add(header);
                                addHeader = false;
                            }

                            if(addHeader) this.headers.AddHeader(header);
                        }
                        socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, HEADER_TAG);
                    }
                }

                else if (tag == NOTIFICATION_TYPE_TAG)
                {
                    if (s == MessageParser.BLANK_LINE)
                    {
                        this.expectedNotificationsRemaining--;
                        if (this.expectedNotificationsRemaining > 0)
                        {
                            this.currentNotification++;
                            socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, NOTIFICATION_TYPE_TAG);
                        }
                        else
                        {
                            // otherwise, check the number of resource pointers we got and start reading those
                            this.pointersExpected = GetNumberOfPointers();
                            if (this.pointersExpected > 0)
                            {
                                this.pointersExpectedRemaining = this.pointersExpected;
                                this.currentPointer = 1;
                                socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                            }
                            else
                            {
                                OnMessageParsed(socket);
                            }
                        }
                    }
                    else
                    {
                        if (this.notificationsToBeRegistered.Count < this.currentNotification)
                        {
                            this.notificationsToBeRegistered.Add(new HeaderCollection());
                        }

                        Header header = Header.ParseHeader(s);
                        this.notificationsToBeRegistered[this.currentNotification - 1].AddHeader(header);
                        socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, NOTIFICATION_TYPE_TAG);
                    }
                }

                else if (tag == RESOURCE_HEADER_TAG)
                {
                    if (s == MessageParser.BLANK_LINE)
                    {
                        // we should have found an Identifier header and Length header, or we are just starting a new section
                        Pointer p = this.pointers[this.currentPointer - 1];
                        if (p.Identifier != null && p.Length > 0)
                        {
                            // read #of bytes
                            int length = this.pointers[this.currentPointer - 1].Length;
                            socket.Read(length, TIMEOUT_GNTP_BINARY, RESOURCE_TAG);
                        }
                        else
                        {
                            socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                        }
                    }
                    else
                    {
                        Header header = Header.ParseHeader(s);
                        // should be Identifer or Length
                        if (header != null)
                        {
                            bool validHeader = false;
                            if (header.Name == Header.RESOURCE_IDENTIFIER)
                            {
                                this.pointers[this.currentPointer - 1].Identifier = header.Value;
                                validHeader = true;
                            }
                            else if (header.Name == Header.RESOURCE_LENGTH)
                            {
                                this.pointers[this.currentPointer - 1].Length = Convert.ToInt32(header.Value);
                                validHeader = true;
                            }
                            else
                                WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.UNRECOGNIZED_RESOURCE_HEADER, header.Name);

                            if (validHeader) socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                        }
                        else
                        {
                            WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.UNRECOGNIZED_RESOURCE_HEADER);
                        }
                    }
                }

                else if (tag == RESOURCE_TAG)
                {
                    // deal with data bytes
                    //byte[] encryptionKey = this.key.GetEncryptionKey(this.keyHashAlgorithm);
                    //byte[] bytes = Cryptography.Decrypt(encryptionKey, this.iv, data.ByteArray, this.encryptionAlgorithm);
                    byte[] bytes = this.key.Decrypt(data.ByteArray, this.iv);

                    Pointer pointer = this.pointers[this.currentPointer - 1];
                    pointer.ByteArray = bytes;
                    BinaryData binaryData = new BinaryData(pointer.Identifier, pointer.ByteArray);
                    ResourceCache.Add(this.applicationName, binaryData);

                    this.pointersExpectedRemaining--;
                    if (this.pointersExpectedRemaining > 0)
                    {
                        this.currentPointer++;
                        socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                    }
                    else
                    {
                        OnMessageParsed(socket);
                    }
                }

                else if (tag == ENCRYPTED_HEADERS_TAG)
                {
                    // see if a length was specified (the original spec did not require the main encrypted headers portion to specify a length)
                    if (s.StartsWith(Header.RESOURCE_LENGTH))
                    {
                        Header header = Header.ParseHeader(s);
                        if (header != null)
                        {
                            int len = Convert.ToInt32(header.Value);
                            socket.Read(len, TIMEOUT_ENCRYPTED_HEADERS, ENCRYPTED_HEADERS_TAG);
                            return;
                        }
                    }

                    ParseEncryptedMessage(data.ByteArray);
                    if (this.pointersExpected > 0)
                        socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                    else
                        OnMessageParsed(socket);
                }

                else
                {
                    WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.MALFORMED_REQUEST);
                }
            }
            catch (GrowlException gEx)
            {
                WriteError(socket, gEx.ErrorCode, gEx.Message, gEx.AdditionalInfo);
            }
            catch(Exception ex)
            {
                WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.MALFORMED_REQUEST, ex.Message);
            }
        }

        /// <summary>
        /// Gets the number of pointers that need to be read from the request, taking
        /// into account any pre-cached binary data.
        /// </summary>
        /// <returns>The number of pointers that need to be read</returns>
        private int GetNumberOfPointers()
        {
            /* check to see if we have already cached the data for
             * all pointers. if so, we can skip reading the binary sections.
             * (unfortunately, if we have cached some of the sections but not all,
             * we will probably have to read it all in again, depending on the order
             * they are included)
             * */
            int c = 0;
            foreach (Header header in this.headers.Pointers)
            {
                Pointer pointer = new Pointer(this.headers);
                this.pointers.Add(pointer);

                if (ResourceCache.IsCached(this.applicationName, header.GrowlResourcePointerID))
                {
                    BinaryData data = ResourceCache.Get(this.applicationName, header.GrowlResourcePointerID);
                    pointer.Identifier = header.GrowlResourcePointerID;
                    pointer.ByteArray = data.Data;
                    c++;
                }
            }
            foreach (HeaderCollection notification in this.notificationsToBeRegistered)
            {
                foreach (Header header in notification.Pointers)
                {
                    Pointer pointer = new Pointer(notification);
                    this.pointers.Add(pointer);

                    if (ResourceCache.IsCached(this.applicationName, header.GrowlResourcePointerID))
                    {
                        BinaryData data = ResourceCache.Get(this.applicationName, header.GrowlResourcePointerID);
                        pointer.Identifier = header.GrowlResourcePointerID;
                        pointer.ByteArray = data.Data;
                        c++;
                    }
                }
            }
            int p = this.pointers.Count;

            // check to see if all pointers were already cached
            if (p == c) p = 0;  // if #cached == total#, we dont need to read any
            else
            {
                // not all of the items are cached, so we have to re-read all of them.
                // to do so, we have to clear any pointer data we may have set
                foreach (Pointer pointer in this.pointers)
                {
                    pointer.Clear();
                }
            }

            if (c > 0 && p == 0)
            {
                requestInfo.SaveHandlingInfo("ALL BINARY RESOURCES ALREADY CACHED");
            }

            return p;
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
                            w.Write(this.alreadyReceived.ToString());
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

            // note that we may have only read part of the request
            this.alreadyReceived.Append(PARTIAL_MESSAGE_NOTICE);

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
        /// Called when the request is successfully parsed.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> that the request came in on</param>
        private void OnMessageParsed(AsyncSocket socket)
        {
            this.socket = socket;
            
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
            CallbackContext context = null;
            if (!String.IsNullOrEmpty(this.callbackData) && !String.IsNullOrEmpty(this.callbackDataType) && String.IsNullOrEmpty(this.callbackUrl))
                context = new CallbackContext(this.callbackData, this.callbackDataType);
            else if(!String.IsNullOrEmpty(this.callbackUrl))
                context = new CallbackContext(this.callbackUrl);
            this.callbackInfo.Context = context;
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
        /// Parses the GNTP header line.
        /// </summary>
        /// <param name="line">The GNTP header</param>
        /// <param name="isLocal">Indicates if the request originated on the local machine.</param>
        /// <returns></returns>
        private static Match ParseGNTPHeaderLine(string line, bool isLocal)
        {
            Match match = null;
            if (isLocal)
            {
                // key not required
                match = regExMessageHeader_Local.Match(line);

                // if they *did* pass the key hash anyway, they need to at least have used the correct format
                if (!match.Success)
                    match = regExMessageHeader_Remote.Match(line);
            }
            else
            {
                match = regExMessageHeader_Remote.Match(line);

                // if there is no match, see if it is due to a missing password
                if (!match.Success)
                    match = regExMessageHeader_Local.Match(line);
            }
            return match;
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

        /// <summary>
        /// Parses the encrypted message.
        /// </summary>
        /// <param name="bytes">The encrypted bytes</param>
        private void ParseEncryptedMessage(byte[] bytes)
        {
            byte[] encryptedBytes = null;

            if (bytes[bytes.Length - 4] == ((byte)'\r')
                && bytes[bytes.Length - 3] == ((byte)'\n')
                && bytes[bytes.Length - 2] == ((byte)'\r')
                && bytes[bytes.Length - 1] == ((byte)'\n'))
            {
                encryptedBytes = new byte[bytes.Length - 4];
            }
            else
            {
                encryptedBytes = new byte[bytes.Length];
            }
            Buffer.BlockCopy(bytes, 0, encryptedBytes, 0, encryptedBytes.Length);

            //byte[] encryptionKey = this.key.GetEncryptionKey(this.keyHashAlgorithm);
            //byte[] decryptedBytes = Cryptography.Decrypt(encryptionKey, this.iv, encryptedBytes, this.encryptionAlgorithm);
            byte[] decryptedBytes = this.key.Decrypt(encryptedBytes, this.iv);

            string x = Encoding.UTF8.GetString(decryptedBytes);

            System.IO.MemoryStream stream = new System.IO.MemoryStream(decryptedBytes);
            using (stream)
            {
                GNTPStreamReader reader = new GNTPStreamReader(stream);
                using (reader)
                {
                    // main headers
                    while (!reader.EndOfStream)
                    {
                        string s = reader.ReadLine().Trim();

                        if (String.IsNullOrEmpty(s))
                        {
                            // if this is a REGISTER message, check Notifications-Count value
                            // to see how many notification sections to expect
                            if (this.directive == RequestType.REGISTER)
                            {
                                if (this.expectedNotifications == 0)
                                {
                                    // a REGISTER request with no notifications is not valid
                                    WriteError(socket, ErrorCode.INVALID_REQUEST, ErrorDescription.NO_NOTIFICATIONS_REGISTERED);
                                }
                            }
                            break;
                        }
                        else
                        {
                            Header header = Header.ParseHeader(s);
                            if (header != null)
                            {
                                bool addHeader = true;
                                if (header.Name == Header.APPLICATION_NAME)
                                {
                                    this.applicationName = header.Value;
                                }
                                if (header.Name == Header.NOTIFICATIONS_COUNT)
                                {
                                    this.expectedNotifications = Convert.ToInt32(header.Value);
                                    this.expectedNotificationsRemaining = this.expectedNotifications;
                                    this.currentNotification = 1;
                                }
                                if (header.Name == Header.NOTIFICATION_CALLBACK_CONTEXT)
                                {
                                    this.callbackData = header.Value;
                                }
                                if (header.Name == Header.NOTIFICATION_CALLBACK_CONTEXT_TYPE)
                                {
                                    this.callbackDataType = header.Value;
                                }
                                if (header.Name == Header.NOTIFICATION_CALLBACK_TARGET || header.Name == Header.NOTIFICATION_CALLBACK_CONTEXT_TARGET)   // left in for compatibility
                                {
                                    this.callbackUrl = header.Value;
                                }
                                if (header.Name == Header.RECEIVED)
                                {
                                    this.requestInfo.PreviousReceivedHeaders.Add(header);
                                    addHeader = false;
                                }

                                if (addHeader) this.headers.AddHeader(header);
                            }
                        }
                    }

                    // any notification type headers
                    if (this.expectedNotifications > 0)
                    {
                        while (!reader.EndOfStream)
                        {
                            string s = reader.ReadLine().Trim();

                            if (s == String.Empty)
                            {
                                this.expectedNotificationsRemaining--;
                                if (this.expectedNotificationsRemaining > 0)
                                {
                                    this.currentNotification++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (this.notificationsToBeRegistered.Count < this.currentNotification)
                                {
                                    this.notificationsToBeRegistered.Add(new HeaderCollection());
                                }

                                Header header = Header.ParseHeader(s);
                                this.notificationsToBeRegistered[this.currentNotification - 1].AddHeader(header);
                            }
                        }
                    }

                    // now that we have read the stream, check for any embedded resources
                    this.pointersExpected = GetNumberOfPointers();
                    if (this.pointersExpected > 0)
                    {
                        this.pointersExpectedRemaining = this.pointersExpected;
                        this.currentPointer = 1;
                    }
                }
            }
        }
    }
}
