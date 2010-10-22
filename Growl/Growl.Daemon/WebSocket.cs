using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Growl.Daemon
{
    /// <summary>
    /// Handles the initial handshake for WebSocket communication
    /// </summary>
    internal class WebSocketHandshakeHandler
    {
        /// <summary>
        /// String that indicates the request may be a WebSocket request
        /// </summary>
        public const string REQUEST_INDICATOR = "GET ";

        private const long HANDSHAKE_REQUEST_TAG = 6000;
        private const long HANDSHAKE_REQUEST_CHALLENGE_TAG = 6001;
        private const long HANDSHAKE_RESPONSE_TAG = 6010;

        byte[] requestBytes = null;

        /// <summary>
        /// Represents methods that handle the HandshakeComplete event
        /// </summary>
        public delegate void HandshakeCompleteEventHandler();

        /// <summary>
        /// The socket making the connection
        /// </summary>
        private AsyncSocket socket;

        /// <summary>
        /// The allowed origin
        /// </summary>
        private string origin;

        /// <summary>
        /// The location of the WebSocket endpoint
        /// </summary>
        private string location;

        /// <summary>
        /// Called when the handshake is complete
        /// </summary>
        private HandshakeCompleteEventHandler callback;

        /// <summary>
        /// Handshake information
        /// </summary>
        private Handshake handshake;


        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketHandshakeHandler"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> making the connection</param>
        /// <param name="origin">The allowed origin of connections, or *.</param>
        /// <param name="location">The location of the WebSocket endpoint</param>
        public WebSocketHandshakeHandler(AsyncSocket socket, string origin, string location)
        {
            this.socket = socket;
            this.origin = origin;
            this.location = location;
        }

        /// <summary>
        /// Does the handshake and then calls the callback when complete.
        /// </summary>
        /// <param name="callback">The <see cref="HandshakeCompleteEventHandler"/> callback.</param>
        public void DoHandshake(HandshakeCompleteEventHandler callback)
        {
            this.callback = callback;

            socket.DidRead += new AsyncSocket.SocketDidRead(socket_DidRead);
            socket.Read(AsyncSocket.CRLFCRLFData, -1, HANDSHAKE_REQUEST_TAG);
        }

        /// <summary>
        /// Handles the socket's DidRead event.
        /// Reads the HTTP headers and sends the handshake response.
        /// </summary>
        /// <param name="sender">The <see cref="AsyncSocket"/>.</param>
        /// <param name="data">The data read.</param>
        /// <param name="tag">The tag identifying the read request.</param>
        void socket_DidRead(AsyncSocket sender, byte[] data, long tag)
        {
            // handle any data that may already have been read by the MessageHandler
            byte[] previousBytes = (byte[])sender.Tag;
            if (previousBytes != null) this.requestBytes = previousBytes;
            sender.Tag = null;

            // append the new data to any already-read data
            if (this.requestBytes != null)
            {
                byte[] bytes = new byte[this.requestBytes.Length + data.Length];
                Array.Copy(this.requestBytes, bytes, this.requestBytes.Length);
                Array.Copy(data, 0, bytes, this.requestBytes.Length, data.Length);
                this.requestBytes = bytes;
            }
            else
            {
                this.requestBytes = data;
            }

            // check the handshake at this point so we know if we should be looking for the challenge bytes or not
            this.handshake = new Handshake(this.requestBytes, this.requestBytes.Length);
            if (handshake.Protocol == WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00 && tag != HANDSHAKE_REQUEST_CHALLENGE_TAG)
            {
                // read challenge bytes
                socket.Read(8, -1, HANDSHAKE_REQUEST_CHALLENGE_TAG);
            }
            else
            {
                /* if we are here, we have read all of the necessary handshake info, so we can proceed */

                // remove this event handler since we dont need it any more
                sender.DidRead -= new AsyncSocket.SocketDidRead(this.socket_DidRead);

                byte[] bytes = this.requestBytes;
                this.handshake = new Handshake(bytes, bytes.Length);

                string response = "";
                byte[] MD5Answer = null;

                // check if the client handshake is valid
                switch (handshake.Protocol)
                {
                    case WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75:
                        if (handshake.Fields == null ||
                            (this.origin != "*" && handshake.Fields["origin"] != this.origin) || // is the connection comming from the right place
                            handshake.Fields["host"] != this.location.Replace("ws://", "")) // is the connection trying to connect to us
                        {
                            throw new Exception("client handshake was invalid");
                        }
                        else
                        {
                            response = handshake.GetHostResponse()
                                .Replace("{ORIGIN}", handshake.Fields["origin"])
                                .Replace("{LOCATION}", this.location + handshake.Fields["path"]);
                        }
                        break;
                    case WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00:
                        if (handshake.Fields == null ||
                            (this.origin != "*" && handshake.Fields["origin"] != this.origin) || // is the connection comming from the right place
                            handshake.Fields["host"] != this.location.Replace("ws://", "")) // is the connection trying to connect to us
                        {
                            throw new Exception("client handshake was invalid");
                        }
                        else
                        {
                            // calculate the handshake proof
                            // the following code is to conform with the protocol

                            string key1 = handshake.Fields["sec-websocket-key1"];
                            string key2 = handshake.Fields["sec-websocket-key2"];

                            // concat all digits and count the spaces
                            StringBuilder sb1 = new StringBuilder();
                            StringBuilder sb2 = new StringBuilder();
                            int spaces1 = 0;
                            int spaces2 = 0;

                            for (int i = 0; i < key1.Length; i++)
                            {
                                if (Char.IsDigit(key1[i]))
                                    sb1.Append(key1[i]);
                                else if (key1[i] == ' ')
                                    spaces1++;
                            }

                            for (int i = 0; i < key2.Length; i++)
                            {
                                if (Char.IsDigit(key2[i]))
                                    sb2.Append(key2[i]);
                                else if (key2[i] == ' ')
                                    spaces2++;
                            }

                            // divide the digits with the number of spaces
                            Int32 result1 = (Int32)(Int64.Parse(sb1.ToString()) / spaces1);
                            Int32 result2 = (Int32)(Int64.Parse(sb2.ToString()) / spaces2);

                            // get the last 8 byte of the client handshake
                            byte[] challenge = new byte[8];
                            Array.Copy(handshake.Raw, bytes.Length - 8, challenge, 0, 8);

                            // convert the results to 32 bit big endian byte arrays
                            byte[] result1bytes = BitConverter.GetBytes(result1);
                            byte[] result2bytes = BitConverter.GetBytes(result2);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(result1bytes);
                                Array.Reverse(result2bytes);
                            }

                            // concat the two integers and the 8 bytes from the client
                            byte[] answer = new byte[16];
                            Array.Copy(result1bytes, 0, answer, 0, 4);
                            Array.Copy(result2bytes, 0, answer, 4, 4);
                            Array.Copy(challenge, 0, answer, 8, 8);

                            // compute the md5 hash
                            MD5 md5 = System.Security.Cryptography.MD5.Create();
                            MD5Answer = md5.ComputeHash(answer);

                            // put the relevant info into the response (the 
                            response = handshake.GetHostResponse()
                                .Replace("{ORIGIN}", handshake.Fields["origin"])
                                .Replace("{LOCATION}", this.location + handshake.Fields["path"]);

                            // just echo the subprotocol for now. This should be picked up and made avaialbe to the application implementation.
                            if (handshake.Fields.ContainsKey("sec-websocket-protocol"))
                                response = response.Replace("{PROTOCOL}", handshake.Fields["sec-websocket-protocol"]);
                            else
                                response = response.Replace("Sec-WebSocket-Protocol: {PROTOCOL}\r\n", "");
                        }
                        break;
                    case WebSocketProtocolIdentifier.Unknown:
                    default:
                        throw new Exception("client handshake was invalid"); // the client handshake was not valid
                }

                // send the handshake, line by line
                byte[] byteResponse = Encoding.UTF8.GetBytes(response);

                // if this is using the draft_ietf_hybi_thewebsocketprotocol_00 protocol, we need to send to answer to the challenge
                if (handshake.Protocol == WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00)
                {
                    int byteResponseLength = byteResponse.Length;
                    Array.Resize(ref byteResponse, byteResponseLength + MD5Answer.Length);
                    Array.Copy(MD5Answer, 0, byteResponse, byteResponseLength, MD5Answer.Length);
                }

                // send the response
                sender.DidWrite += new AsyncSocket.SocketDidWrite(socket_DidWrite);
                sender.Write(byteResponse, 0, byteResponse.Length, -1, HANDSHAKE_RESPONSE_TAG);
            }
        }

        /// <summary>
        /// Handles the socket's DidWrite event.
        /// Calls the callback.
        /// </summary>
        /// <param name="sender">The <see cref="AsyncSocket"/>.</param>
        /// <param name="tag">The tag identifying the write request.</param>
        void socket_DidWrite(AsyncSocket sender, long tag)
        {
            // remove this since we dont need it any more
            sender.DidWrite -= new AsyncSocket.SocketDidWrite(socket_DidWrite);

            if (tag == HANDSHAKE_RESPONSE_TAG)
            {
                this.callback.BeginInvoke(null, null);
            }
        }
    }

    /// <summary>
    /// Lists the different types of WebSocket implementations that are supported
    /// </summary>
    public enum WebSocketProtocolIdentifier
    {
        /// <summary>
        /// Unknown websocket protocol
        /// </summary>
        Unknown,

        /// <summary>
        /// See http://tools.ietf.org/html/draft-hixie-thewebsocketprotocol-75
        /// As of 7/7/2010, Google Chrome 5 uses this protocol (though v6 is slated to move to the IETF/76 protocol)
        /// As of 7/7/2010, Safari 5 also uses this protocol
        /// </summary>
        draft_hixie_thewebsocketprotocol_75,

        /// <summary>
        /// See http://www.whatwg.org/specs/web-socket-protocol/
        /// </summary>
        draft_ietf_hybi_thewebsocketprotocol_00, // aka draft-hixie-thewebsocketprotocol-76
    }

    /// <summary>
    /// Represents a websocket handshake. The class knows the format of the handshake, both from the client and the host.
    /// </summary>
    class Handshake
    {
        /// <summary>
        /// A list of patterns identifying different WebSocket protocol implementations
        /// </summary>
        public static Dictionary<WebSocketProtocolIdentifier, string> ClientPatterns;

        /// <summary>
        /// A list of host responses corresponding to diffrent WebSocket protocol implementations
        /// </summary>
        public static Dictionary<WebSocketProtocolIdentifier, string> HostResponses;

        /// <summary>
        /// The WebSocket protocol being used
        /// </summary>
        private WebSocketProtocolIdentifier protocol;

        /// <summary>
        /// The handshake-related fields from the request
        /// </summary>
        private Dictionary<string, string> fields;

        /// <summary>
        /// The raw HTTP header information
        /// </summary>
        private byte[] raw;

        /// <summary>
        /// Initializes the ClientPatterns and HostResponses lists
        /// </summary>
        static Handshake()
        {
            ClientPatterns = new Dictionary<WebSocketProtocolIdentifier,string>();
            ClientPatterns.Add(WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75, 
                @"^(?<connect>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\n" +
                @"Upgrade:\sWebSocket\n" +
                @"Connection:\sUpgrade\n" +
                @"Host:\s(?<host>[^\n]+)\n" +
                @"Origin:\s(?<origin>[^\n]+)\n\n$");
            ClientPatterns.Add(WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00,
                @"^(?<connect>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\n" + 
                @"((?<field_name>[^:\s]+):\s(?<field_value>[^\n]+)\n)+");

            HostResponses = new Dictionary<WebSocketProtocolIdentifier,string>();
            HostResponses.Add(WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75, 
                "HTTP/1.1 101 Web Socket Protocol Handshake\r\n"+
                "Upgrade: WebSocket\r\n"+
                "Connection: Upgrade\r\n"+
                "WebSocket-Origin: {ORIGIN}\r\n"+
                "WebSocket-Location: {LOCATION}\r\n"+
                "\r\n");
            HostResponses.Add(WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00,
                "HTTP/1.1 101 Web Socket Protocol Handshake\r\n"+
                "Upgrade: WebSocket\r\n"+
                "Connection: Upgrade\r\n"+
                "Sec-WebSocket-Origin: {ORIGIN}\r\n"+
                "Sec-WebSocket-Location: {LOCATION}\r\n"+
                "Sec-WebSocket-Protocol: {PROTOCOL}\r\n"+
                "\r\n");
        }

        /// <summary>
        /// The web socket protocol the client is using
        /// </summary>
        /// <value>
        /// <see cref="WebSocketProtocolIdentifier"/>
        /// </value>
        public WebSocketProtocolIdentifier Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        /// <summary>
        /// Gets the handshake-related fields from the request
        /// </summary>
        /// <value><see cref="Dictionary{TKey, TVal}"/></value>
        public Dictionary<string, string> Fields
        {
            get
            {
                return this.fields;
            }
        }

        /// <summary>
        /// Gets the raw HTTP header information.
        /// </summary>
        /// <value>byte array</value>
        public byte[] Raw
        {
            get
            {
                return this.raw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Handshake"/> class.
        /// </summary>
        /// <param name="handshakeRaw">The raw HTTP information received from the client</param>
        /// <param name="length">The length of the handshake information</param>
        public Handshake(byte[] handshakeRaw, int length)
        {
            this.raw = handshakeRaw;
            string handshake = Encoding.UTF8.GetString(handshakeRaw, 0, length).Replace("\r\n", "\n");

            foreach (KeyValuePair<WebSocketProtocolIdentifier, string> pattern in ClientPatterns)
            {
                Regex regex = new Regex(pattern.Value);
                Match match = regex.Match(handshake);
                if (match.Success)
                {
                    this.protocol = pattern.Key;
                    SetFields(match.Groups);
                    return;
                }
            }

            this.protocol = WebSocketProtocolIdentifier.Unknown;
        }

        /// <summary>
        /// Get the expected response to the handshake. The string contains placeholders for fields that needs to be filled out, before sending the handshake to the client.
        /// </summary>
        /// <returns>string</returns>
        public string GetHostResponse()
        {
            return HostResponses[Protocol];
        }

        /// <summary>
        /// Sets the handshake-related fields based on the HTTP information.
        /// </summary>
        /// <param name="gc">Regex-matched group collection</param>
        private void SetFields(GroupCollection gc)
        {
            switch (Protocol)
            {
                case WebSocketProtocolIdentifier.draft_hixie_thewebsocketprotocol_75:
                    this.fields = new Dictionary<string, string>();
                    this.fields.Add("connect", gc["connect"].ToString());
                    this.fields.Add("path", gc["path"].ToString());
                    this.fields.Add("host", gc["host"].ToString());
                    this.fields.Add("origin", gc["origin"].ToString());

                    break;
                case WebSocketProtocolIdentifier.draft_ietf_hybi_thewebsocketprotocol_00:
                    this.fields = new Dictionary<string, string>();

                    for (int i = 0; i < gc["field_name"].Captures.Count; i++)
                    {
                        this.fields.Add(gc["field_name"].Captures[i].ToString().ToLower(), gc["field_value"].Captures[i].ToString());
                    }

                    this.fields.Add("connect", gc["connect"].ToString());
                    this.fields.Add("path", gc["path"].ToString());

                    break;
                case WebSocketProtocolIdentifier.Unknown:    
                default:
                    break;
            }
        }
    }
}
