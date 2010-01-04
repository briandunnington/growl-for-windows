using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Growl
{
    public class CometClient
    {
        public delegate void ResponseReceivedEventHandler(string response);

        public event ResponseReceivedEventHandler ResponseReceived;
        public event EventHandler Disconnected;

        private string host;
        private int port;
        private string path;

        /// <summary>
        /// A collection of active ConnectState objects awaiting the EndConnect callback
        /// </summary>
        private Dictionary<string, ConnectState> connecting = new Dictionary<string, ConnectState>();

        /// <summary>
        /// A collection of active TcpState objects awaiting the EndWrite callback
        /// </summary>
        private Dictionary<string, TcpState> writing = new Dictionary<string, TcpState>();

        /// <summary>
        /// A collection of active TcpState objects awaiting the EndRead callback
        /// </summary>
        private Dictionary<string, TcpState> reading = new Dictionary<string, TcpState>();


        public CometClient(string url)
        {
            Uri uri = new Uri(url);
            this.host = uri.Host;
            this.port = uri.Port;
            this.path = uri.PathAndQuery;
        }

        public void Start()
        {
            ConnectState state = null;
            try
            {
                // build HTTP request
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("GET {0} HTTP/1.1\r\n", this.path);
                sb.AppendFormat("Host: {0}\r\n", this.host);
                sb.Append("User-Agent: Growl for Windows 2.0\r\n");
                sb.Append("\r\n");

                // send HTTP request
                string http = sb.ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(http);

                TcpClient client = new TcpClient();
                AsyncCallback callback = new AsyncCallback(ConnectCallback);
                state = new ConnectState(client, bytes);
                connecting.Add(state.GUID, state);
                client.BeginConnect(this.host, this.port, callback, state);
            }
            catch
            {
                // suppress
                CleanUpSocket(state);
                OnDisconnected();
            }
        }

        /// <summary>
        /// Called once the connection has connected. Begins writing the request.
        /// </summary>
        /// <param name="iar">The result of the asynchronous operation.</param>
        private void ConnectCallback(IAsyncResult iar)
        {
            ConnectState connectState = null;
            TcpState tcpState = null;
            try
            {
                connectState = (ConnectState)iar.AsyncState;
                TcpClient client = connectState.Client;
                byte[] bytes = connectState.Bytes;
                client.EndConnect(iar);

                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];
                AsyncCallback callback = new AsyncCallback(WriteCallback);
                tcpState = TcpState.FromConnectState(connectState, stream, buffer);
                writing.Add(tcpState.GUID, tcpState);
                stream.BeginWrite(bytes, 0, bytes.Length, callback, tcpState);
            }
            catch
            {
                CleanUpSocket(connectState);
                CleanUpSocket(tcpState);
                OnDisconnected();
            }
            finally
            {
                if (connectState != null) connecting.Remove(connectState.GUID);
            }
        }

        /// <summary>
        /// Called once the request has been written. Begins reading the response.
        /// </summary>
        /// <param name="iar">The result of the asynchronous operation.</param>
        private void WriteCallback(IAsyncResult iar)
        {
            TcpState state = null;
            try
            {
                state = (TcpState)iar.AsyncState;
                state.Stream.EndWrite(iar);

                AsyncCallback callback = new AsyncCallback(ReadCallback);
                state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, callback, state);
                reading.Add(state.GUID, state);
            }
            catch
            {
                CleanUpSocket(state);
                OnDisconnected();
            }
            finally
            {
                if (state != null) writing.Remove(state.GUID);
            }
        }

        /// <summary>
        /// Called once the response has been read. If the transaction is complete, the connection is closed.
        /// If there is an outstanding callback, the connection waits to read it before closing.
        /// </summary>
        /// <param name="iar">The result of the asynchronous operation.</param>
        private void ReadCallback(IAsyncResult iar)
        {
            TcpState state = null;

            try
            {
                state = (TcpState)iar.AsyncState;
                int length = state.Stream.EndRead(iar);

                if (length > 0)
                {
                    string response = System.Text.Encoding.UTF8.GetString(state.Buffer, 0, length);

                    // if this is the first response, we need to ignore the HTTP headers
                    if (!state.HasReceivedData)
                    {
                        int index = response.IndexOf("\r\n\r\n");
                        response = response.Substring(index);
                        state.HasReceivedData = true;
                    }

                    state.Response += response.Trim(); ;

                    OnResponseReceived(state.Response);
                    state.Response = null;  // reset

                    // keep listening for more
                    AsyncCallback callback = new AsyncCallback(ReadCallback);
                    state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, callback, state);
                }
            }
            catch
            {
                CleanUpSocket(state);
                OnDisconnected();
            }
            finally
            {
            }
        }

        /// <summary>
        /// Cleans up any connection-related objects when the connection is no longer needed
        /// (either closed intentionally or encounters an exception)
        /// </summary>
        /// <param name="state">The <see cref="ConnectState"/> state information</param>
        private void CleanUpSocket(ConnectState state)
        {
            try
            {
                if (state != null)
                {
                    if (connecting.ContainsKey(state.GUID)) connecting.Remove(state.GUID);
                    if (writing.ContainsKey(state.GUID)) writing.Remove(state.GUID);
                    if (reading.ContainsKey(state.GUID)) reading.Remove(state.GUID);

                    if (state.Client != null && state.Client.Client != null)
                    {
                        state.Client.Client.Blocking = true;
                        state.Client.Client.Shutdown(SocketShutdown.Both);
                        state.Client.Client.Close();
                        state.Client.Close();
                    }

                    TcpState tcpState = state as TcpState;
                    if (tcpState != null)
                    {
                        if (tcpState.Stream != null)
                        {
                            tcpState.Stream.Close();
                            tcpState.Stream.Dispose();
                        }
                        tcpState.Stream = null;
                    }

                    state.Client = null;
                }
            }
            catch
            {
            }
            state = null;
        }

        protected void OnResponseReceived(string response)
        {
            if (this.ResponseReceived != null)
            {
                this.ResponseReceived(response);
            }
        }

        protected void OnDisconnected()
        {
            if (this.Disconnected != null)
            {
                this.Disconnected(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Contains state information for a connection that is in the process of connecting.
        /// </summary>
        private class ConnectState
        {
            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            protected ConnectState() { }

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            /// <param name="client">The <see cref="TcpClient"/> used to make the connection</param>
            /// <param name="bytes">The request bytes to be written</param>
            public ConnectState(TcpClient client, byte[] bytes)
            {
                this.Client = client;
                this.Bytes = bytes;
            }

            /// <summary>
            /// Uniquely identifies this instance
            /// </summary>
            public readonly string GUID = System.Guid.NewGuid().ToString();

            /// <summary>
            /// The <see cref="TcpClient"/> used to make the connection
            /// </summary>
            public TcpClient Client;

            /// <summary>
            /// The request bytes to be written
            /// </summary>
            public byte[] Bytes;
        }

        /// <summary>
        /// Contains state information for a connection that has already connected and is either writing
        /// or reading data.
        /// </summary>
        private class TcpState : ConnectState
        {
            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            private TcpState()
            {
            }

            /// <summary>
            /// Creates a new instance of the TcpState class from an exisiting <see cref="ConnectState"/>.
            /// </summary>
            /// <param name="cs">The <see cref="ConnectState"/></param>
            /// <param name="stream">The <see cref="NetworkStream"/> of the connection</param>
            /// <param name="buffer">The buffer to hold the response</param>
            /// <returns><see cref="TcpState"/></returns>
            public static TcpState FromConnectState(ConnectState cs, NetworkStream stream, byte[] buffer)
            {
                TcpState state = new TcpState();
                state.Client = cs.Client;
                state.Bytes = cs.Bytes;
                state.Stream = stream;
                state.Buffer = buffer;
                return state;
            }

            /// <summary>
            /// The <see cref="NetworkStream"/> of the connection
            /// </summary>
            public NetworkStream Stream;

            /// <summary>
            /// The buffer to hold the response
            /// </summary>
            public byte[] Buffer;

            /// <summary>
            /// Holds the response text
            /// </summary>
            public string Response;

            public bool HasReceivedData;
        }
    }
}
