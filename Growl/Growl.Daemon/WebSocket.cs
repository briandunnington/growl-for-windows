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

        private const string WEBSOCKET_GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private const string HEADER_ORIGIN = "origin";
        private const string HEADER_SEC_WEBSOCKET_KEY = "sec-websocket-key";
        private const string HEADER_SEC_WEBSOCKET_PROTOCOL = "sec-websocket-protocol";
        private const string HEADER_SEC_WEBSOCKET_VERSION = "sec-websocket-version";

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
            // remove this event handler since we dont need it any more
            sender.DidRead -= new AsyncSocket.SocketDidRead(this.socket_DidRead);

            // handle any data that may already have been read by the MessageHandler
            byte[] previousBytes = (byte[])sender.Tag;
            if (previousBytes != null) this.requestBytes = previousBytes;
            sender.Tag = null;

            // append the new data to any already-read data (this cant really happen now that we updated to the 07 spec, but it doenst hurt anything)
            if (this.requestBytes != null)
            {
                byte[] tempBytes = new byte[this.requestBytes.Length + data.Length];
                Array.Copy(this.requestBytes, tempBytes, this.requestBytes.Length);
                Array.Copy(data, 0, tempBytes, this.requestBytes.Length, data.Length);
                this.requestBytes = tempBytes;
            }
            else
            {
                this.requestBytes = data;
            }

            // check the handshake at this point so we know if we should be looking for the challenge bytes or not
            this.handshake = new Handshake(this.requestBytes, this.requestBytes.Length);

            // we could check the Sec-WebSocket-Origin here, but we really dont care where they are connecting from

            // calculate the handshake proof
            string key = handshake.Fields[HEADER_SEC_WEBSOCKET_KEY];
            string concat = key + WEBSOCKET_GUID;
            byte[] keyBytes = Encoding.UTF8.GetBytes(concat);
            SHA1 sha1 = SHA1.Create();
            byte[] sha1Bytes = sha1.ComputeHash(keyBytes);
            string accept = Convert.ToBase64String(sha1Bytes);

            // construct the handshake response
            string version = handshake.Fields[HEADER_SEC_WEBSOCKET_VERSION];
            string protocol = handshake.Fields.ContainsKey(HEADER_SEC_WEBSOCKET_PROTOCOL) ? handshake.Fields[HEADER_SEC_WEBSOCKET_PROTOCOL] : null;
            string response = handshake.GetHostResponse(accept, version, protocol);
            byte[] byteResponse = Encoding.UTF8.GetBytes(response);

            // send the response
            sender.DidWrite += new AsyncSocket.SocketDidWrite(socket_DidWrite);
            sender.Write(byteResponse, 0, byteResponse.Length, -1, HANDSHAKE_RESPONSE_TAG);
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
    /// Represents a websocket handshake. The class knows the format of the handshake, both from the client and the host.
    /// </summary>
    class Handshake
    {
        const string CLIENT_PATTERN =
                @"^(?<connect>[^\s]+)\s(?<path>[^\s]+)\sHTTP\/1\.1\n" +
                @"((?<field_name>[^:\s]+):\s(?<field_value>[^\n]+)\n)+";

        /// <summary>
        /// The handshake-related fields from the request
        /// </summary>
        private Dictionary<string, string> fields;

        /// <summary>
        /// The raw HTTP header information
        /// </summary>
        private byte[] raw;

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

            Regex regex = new Regex(CLIENT_PATTERN);
            Match match = regex.Match(handshake);
            if (match.Success)
            {
                SetFields(match.Groups);
                return;
            }
        }

        /// <summary>
        /// Get the expected response to the handshake.
        /// </summary>
        /// <returns>string</returns>
        public string GetHostResponse(string accept, string version, string protocol)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 101 Switching Protocols\r\n");
            sb.Append("Upgrade: WebSocket\r\n");
            sb.Append("Connection: Upgrade\r\n");
            sb.AppendFormat("Sec-WebSocket-Accept: {0}\r\n", accept);
            sb.AppendFormat("Sec-WebSocket-Version: {0}\r\n", version);

            if(!String.IsNullOrEmpty(protocol))
                sb.AppendFormat("Sec-WebSocket-Protocol: {0}\r\n", protocol);

            sb.Append("\r\n");

            return sb.ToString();

            /*
            HostResponse = 
                "HTTP/1.1 101 Switching Protocols\r\n"+
                "Upgrade: WebSocket\r\n"+
                "Connection: Upgrade\r\n"+
                "Sec-WebSocket-Accept: {ACCEPT}\r\n"+
                "Sec-WebSocket-Protocol: {PROTOCOL}\r\n" +
                "Sec-WebSocket-Version: {VERSION}\r\n" +
                "\r\n";
             * */
        }

        /// <summary>
        /// Sets the handshake-related fields based on the HTTP information.
        /// </summary>
        /// <param name="gc">Regex-matched group collection</param>
        private void SetFields(GroupCollection gc)
        {
            this.fields = new Dictionary<string, string>();

            for (int i = 0; i < gc["field_name"].Captures.Count; i++)
            {
                this.fields.Add(gc["field_name"].Captures[i].ToString().ToLower(), gc["field_value"].Captures[i].ToString());
            }

            this.fields.Add("connect", gc["connect"].ToString());
            this.fields.Add("path", gc["path"].ToString());
        }
    }
}
