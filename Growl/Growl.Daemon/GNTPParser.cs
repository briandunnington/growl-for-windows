using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Growl.Daemon
{
    /// <summary>
    /// This class handles parsing GNTP requests. Data is fed in and parsed in real-time.
    /// If the request is malformed or invalid, an error event will be triggered. Otherwise,
    /// when an entire valid request has been read, a 'message parsed' event will be triggered.
    /// 
    /// NOTE that currently, the parser is geared for use with the GNTPSocketReader and thus
    /// expects data to be fed in in a certain way. This limitation will be removed eventually,
    /// but it works for now.
    /// </summary>
    class GNTPParser
    {
        private const long ACCEPT_TAG = 0;
        private const long GNTP_IDENTIFIER_TAG = 1;
        private const long HEADER_TAG = 2;
        private const long NOTIFICATION_TYPE_TAG = 3;
        private const long RESOURCE_HEADER_TAG = 4;
        private const long RESOURCE_TAG = 5;
        private const long ENCRYPTED_HEADERS_TAG = 6;

        /// <summary>
        /// Contains data already read and parsed.
        /// </summary>
        StringBuilder alreadyReceived;

        /// <summary>
        /// Tag used to indicate where in the parsing process the parser is at and what to expect next
        /// </summary>
        long tag;

        /// <summary>
        /// Represents the method that will handle the <see cref="Error"/> event.
        /// </summary>
        public delegate void GNTPParserErrorEventHandler(Error error);

        /// <summary>
        /// Represents the method that will handle the <see cref="MessageParsed"/> event.
        /// </summary>
        /// <param name="request">The <see cref="GNTPRequest"/> representing the parsed message</param>
        public delegate void GNTPParserMessageParsedEventHandler(GNTPRequest request);

        /// <summary>
        /// Occurs when the MessageHandler is going to return an Error response
        /// </summary>
        public event GNTPParserErrorEventHandler Error;

        /// <summary>
        /// Occurs when the request has been successfully parsed
        /// </summary>
        public event GNTPParserMessageParsedEventHandler MessageParsed;

        /// <summary>
        /// Regex used to parse GNTP headers for local requests (dont require password)
        /// </summary>
        private static Regex regExMessageHeader_Local = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))|((?<EncryptionAlgorithm>\S+)))\s*[\r\n]");

        /// <summary>
        /// Regex used to parse GNTP headers for non-local requests (password required)
        /// </summary>
        private static Regex regExMessageHeader_Remote = new Regex(@"GNTP/(?<Version>.\..)\s+(?<Directive>\S+)\s+(((?<EncryptionAlgorithm>\S+):(?<IV>\S+))\s+|((?<EncryptionAlgorithm>\S+)\s+))(?<KeyHashAlgorithm>(\S+)):(?<KeyHash>(\S+))\.(?<Salt>(\S+))\s*[\r\n]");

        /// <summary>
        /// Message logged when a request is only partially read before encountering an error
        /// </summary>
        private const string PARTIAL_MESSAGE_NOTICE = "<Additional request data may not have been read after the message was invalidated.>";

        /// <summary>
        /// Indicates if remote notifications are allowed
        /// </summary>
        private bool allowNetworkNotifications = false;

        /// <summary>
        /// Indicates if notifications originating from a browser are allowed
        /// </summary>
        private bool allowBrowserConnections = false;

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
        /// Indicates if the request must supply a password
        /// </summary>
        private bool passwordRequired = true;

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
        /// The text of the request after decryption (null if the request was not originally encrypted)
        /// </summary>
        private string decryptedRequest = null;


        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPParser"/> class.
        /// </summary>
        /// <param name="passwordManager">The <see cref="PasswordManager"/> containing a list of allowed passwords</param>
        /// <param name="passwordRequired">Indicates if a password is required</param>
        /// <param name="allowNetworkNotifications">Indicates if network requests are allowed</param>
        /// <param name="allowBrowserConnections">Indicates if browser requests are allowed</param>
        /// <param name="allowSubscriptions">Indicates if SUBSCRIPTION requests are allowed</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with this request</param>
        public GNTPParser(PasswordManager passwordManager, bool passwordRequired, bool allowNetworkNotifications, bool allowBrowserConnections, bool allowSubscriptions, RequestInfo requestInfo)
        {
            this.passwordManager = passwordManager;
            this.passwordRequired = passwordRequired;
            this.allowNetworkNotifications = allowNetworkNotifications;
            this.allowBrowserConnections = allowBrowserConnections;
            this.allowSubscriptions = allowSubscriptions;
            this.requestInfo = requestInfo;

            this.alreadyReceived = new StringBuilder();
            this.headers = new HeaderCollection();
            this.notificationsToBeRegistered = new List<HeaderCollection>();
            this.pointers = new List<Pointer>();
            this.callbackInfo = new CallbackInfo();
        }

        /// <summary>
        /// Gets the value indicating where in the parsing process the parser is at and what to expect next.
        /// </summary>
        /// <value>long</value>
        public long Tag
        {
            get
            {
                return this.tag;
            }
        }

        /// <summary>
        /// Gets the decrypted request.
        /// </summary>
        /// <value>
        /// This value will only be set once the <see cref="MessageParsed"/> event has been fired.
        /// For unencrypted requests, this value will always be null.
        /// For encrypted requests, this will contain the full decrypted data.
        /// </value>
        internal string DecryptedRequest
        {
            get
            {
                return this.decryptedRequest;
            }
        }

        /// <summary>
        /// Parses the specified input bytes and returns information on what is expected next.
        /// </summary>
        /// <param name="inputBytes">The input bytes to parse.</param>
        /// <returns>
        /// A <see cref="NextIndicator"/> instance that can be used by the reading class to 
        /// figure out what is expected next.
        /// </returns>
        /// <remarks>
        /// NOTE that currently, the parser is geared for use with the GNTPSocketReader and thus
        /// expects data to be fed in in a certain way. This limitation will be removed eventually,
        /// but it works for now.
        /// </remarks>
        public NextIndicator Parse(byte[] inputBytes)
        {
            try
            {
                Data data = new Data(inputBytes);
                string s = data.ToString();
                alreadyReceived.Append(s);

                if (tag == ACCEPT_TAG)
                {
                    // do nothing here but wait for more data
                    tag = GNTP_IDENTIFIER_TAG;
                    return NextIndicator.CRLF;
                }

                else if (tag == GNTP_IDENTIFIER_TAG)
                {
                    string line = alreadyReceived.ToString();
                    Match match = ParseGNTPHeaderLine(line, this.passwordRequired);

                    if (match.Success)
                    {
                        this.version = match.Groups["Version"].Value;
                        if (version == MessageParser.GNTP_SUPPORTED_VERSION)
                        {
                            string d = match.Groups["Directive"].Value;
                            if (Enum.IsDefined(typeof(RequestType), d))
                            {
                                this.directive = (RequestType)Enum.Parse(typeof(RequestType), d);

                                // check for supported but not allowed requests
                                if (this.directive == RequestType.SUBSCRIBE && !this.allowSubscriptions)
                                {
                                    OnError(ErrorCode.NOT_AUTHORIZED, ErrorDescription.SUBSCRIPTIONS_NOT_ALLOWED);
                                    return NextIndicator.None;
                                }
                                else
                                {
                                    this.encryptionAlgorithm = Cryptography.GetEncryptionType(match.Groups["EncryptionAlgorithm"].Value);
                                    this.ivHex = (match.Groups["IV"] != null ? match.Groups["IV"].Value : null);
                                    if (!String.IsNullOrEmpty(this.ivHex)) this.iv = Cryptography.HexUnencode(this.ivHex);
                                    string keyHash = match.Groups["KeyHash"].Value.ToUpper();

                                    bool authorized = false;
                                    // Any of the following criteria require a password:
                                    //    1. the request did not originate on the local machine or LAN
                                    //    2. the request came from the LAN, but LAN passwords are required
                                    //    2. it is a SUBSCRIBE request (all subscriptions require a password)
                                    //    3. the user's preferences require even local requests to supply a password
                                    // Additionally, even if a password is not required, it will be validated if the 
                                    // sending appplication includes one
                                    string errorDescription = ErrorDescription.INVALID_KEY;
                                    if (this.passwordRequired || this.directive == RequestType.SUBSCRIBE || !String.IsNullOrEmpty(keyHash))
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
                                        {
                                            tag = HEADER_TAG;
                                            return NextIndicator.CRLF;
                                            //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, HEADER_TAG);
                                        }
                                        else
                                        {
                                            tag = ENCRYPTED_HEADERS_TAG;
                                            return NextIndicator.CRLFCRLF;
                                            //socket.Read(AsyncSocket.CRLFCRLFData, TIMEOUT_ENCRYPTED_HEADERS, ENCRYPTED_HEADERS_TAG);
                                        }
                                    }
                                    else
                                    {
                                        OnError(ErrorCode.NOT_AUTHORIZED, errorDescription);
                                        return NextIndicator.None;
                                    }
                                }
                            }
                            else
                            {
                                OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.UNSUPPORTED_DIRECTIVE, d);
                                return NextIndicator.None;
                            }
                        }
                        else
                        {
                            OnError(ErrorCode.UNKNOWN_PROTOCOL_VERSION, ErrorDescription.UNSUPPORTED_VERSION, version);
                            return NextIndicator.None;
                        }
                    }
                    else
                    {
                        OnError(ErrorCode.UNKNOWN_PROTOCOL, ErrorDescription.MALFORMED_REQUEST);
                        return NextIndicator.None;
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
                                tag = NOTIFICATION_TYPE_TAG;
                                return NextIndicator.CRLF;
                                //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, NOTIFICATION_TYPE_TAG);
                            }
                            else
                            {
                                // a REGISTER request with no notifications is not valid
                                OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.NO_NOTIFICATIONS_REGISTERED);
                                return NextIndicator.None;
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
                                tag = RESOURCE_HEADER_TAG;
                                return NextIndicator.CRLF;
                                //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                            }
                            else
                            {
                                OnMessageParsed();
                                return NextIndicator.None;
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

                            if (addHeader) this.headers.AddHeader(header);
                        }
                        tag = HEADER_TAG;
                        return NextIndicator.CRLF;
                        //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, HEADER_TAG);
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
                            tag = NOTIFICATION_TYPE_TAG;
                            return NextIndicator.CRLF;
                            //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, NOTIFICATION_TYPE_TAG);
                        }
                        else
                        {
                            // otherwise, check the number of resource pointers we got and start reading those
                            this.pointersExpected = GetNumberOfPointers();
                            if (this.pointersExpected > 0)
                            {
                                this.pointersExpectedRemaining = this.pointersExpected;
                                this.currentPointer = 1;
                                tag = RESOURCE_HEADER_TAG;
                                return NextIndicator.CRLF;
                                //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                            }
                            else
                            {
                                OnMessageParsed();
                                return NextIndicator.None;
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
                        tag = NOTIFICATION_TYPE_TAG;
                        return NextIndicator.CRLF;
                        //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, NOTIFICATION_TYPE_TAG);
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
                            tag = RESOURCE_TAG;
                            return new NextIndicator(length);
                            //socket.Read(length, TIMEOUT_GNTP_BINARY, RESOURCE_TAG);
                        }
                        else
                        {
                            tag = RESOURCE_HEADER_TAG;
                            return NextIndicator.CRLF;
                            //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
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
                            {
                                OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.UNRECOGNIZED_RESOURCE_HEADER, header.Name);
                                return NextIndicator.None;
                            }

                            if (validHeader)
                            {
                                tag = RESOURCE_HEADER_TAG;
                                return NextIndicator.CRLF;
                                //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                            }
                        }
                        else
                        {
                            OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.UNRECOGNIZED_RESOURCE_HEADER);
                            return NextIndicator.None;
                        }
                    }
                }

                else if (tag == RESOURCE_TAG)
                {
                    // deal with data bytes
                    byte[] bytes = this.key.Decrypt(data.ByteArray, this.iv);

                    Pointer pointer = this.pointers[this.currentPointer - 1];
                    pointer.ByteArray = bytes;
                    BinaryData binaryData = new BinaryData(pointer.Identifier, pointer.ByteArray);
                    ResourceCache.Add(this.applicationName, binaryData);

                    this.pointersExpectedRemaining--;
                    if (this.pointersExpectedRemaining > 0)
                    {
                        this.currentPointer++;
                        tag = RESOURCE_HEADER_TAG;
                        return NextIndicator.CRLF;
                        //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                    }
                    else
                    {
                        OnMessageParsed();
                        return NextIndicator.None;
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
                            tag = ENCRYPTED_HEADERS_TAG;
                            return new NextIndicator(len);
                            //socket.Read(len, TIMEOUT_ENCRYPTED_HEADERS, ENCRYPTED_HEADERS_TAG);
                        }
                    }

                    ParseEncryptedMessage(data.ByteArray);
                    if (this.pointersExpected > 0)
                    {
                        tag = RESOURCE_HEADER_TAG;
                        return NextIndicator.CRLF;
                        //socket.Read(AsyncSocket.CRLFData, TIMEOUT_GNTP_HEADER, RESOURCE_HEADER_TAG);
                    }
                    else
                    {
                        OnMessageParsed();
                        return NextIndicator.None;
                    }
                }

                else
                {
                    OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.MALFORMED_REQUEST);
                    return NextIndicator.None;
                }
            }
            catch (GrowlException gEx)
            {
                OnError(gEx.ErrorCode, gEx.Message, gEx.AdditionalInfo);
                return NextIndicator.None;
            }
            catch (Exception ex)
            {
                OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.MALFORMED_REQUEST, ex.Message);
                return NextIndicator.None;
            }

            return NextIndicator.None;
        }

        /// <summary>
        /// Called when the request is successfully parsed.
        /// </summary>
        private void OnMessageParsed()
        {
            // handle callback context
            CallbackContext context = null;
            if (!String.IsNullOrEmpty(this.callbackData) && !String.IsNullOrEmpty(this.callbackDataType) && String.IsNullOrEmpty(this.callbackUrl))
                context = new CallbackContext(this.callbackData, this.callbackDataType);
            else if (!String.IsNullOrEmpty(this.callbackUrl))
                context = new CallbackContext(this.callbackUrl);

            if (this.MessageParsed != null)
            {
                GNTPRequest request = new GNTPRequest(this.version, this.directive, this.key, this.headers, this.applicationName, this.notificationsToBeRegistered, context);
                this.MessageParsed(request);
            }
            else
            {
                // no handler - return some kind of error? (this should never really happen)
                OnError(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
            }
        }

        /// <summary>
        /// Triggers the Error event
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="args">Any additional data to include in the error message</param>
        private void OnError(int errorCode, string errorMessage, params object[] args)
        {
            // note that we may have only read part of the request
            this.alreadyReceived.Append(PARTIAL_MESSAGE_NOTICE);

            // handle additional arguments
            if (args != null)
            {
                foreach (object arg in args)
                {
                    errorMessage += String.Format(" ({0})", arg);
                }
            }

            // fire event
            Error error = new Error(errorCode, errorMessage);
            if (this.Error != null)
            {
                this.Error(error);
            }
        }

        /// <summary>
        /// Parses the GNTP header line.
        /// </summary>
        /// <param name="line">The GNTP header</param>
        /// <param name="passwordRequired">Indicates if the request must contain a password</param>
        /// <returns></returns>
        private static Match ParseGNTPHeaderLine(string line, bool passwordRequired)
        {
            Match match = null;
            if (!passwordRequired)
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

            byte[] decryptedBytes = this.key.Decrypt(encryptedBytes, this.iv);

            // log the decrypted data
#if DEBUG
            this.decryptedRequest = Encoding.UTF8.GetString(decryptedBytes);
#endif

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
                                    OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.NO_NOTIFICATIONS_REGISTERED);
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
    }
}
