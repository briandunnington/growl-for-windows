using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Growl.Connector
{
    /// <summary>
    /// The base class for any objects that want to send requests to a GNTP server.
    /// </summary>
    /// <remarks>
    /// Along with applications sending notifications, this class serves as the basis
    /// for objects that do things like forward notifications from one server to another
    /// or subscribe to notifications from a remote client.
    /// </remarks>
    public abstract class ConnectorBase
    {
        /// <summary>
        /// The standard TCP port that GNTP uses
        /// </summary>
        public const int TCP_PORT = 23053;

        /// <summary>
        /// Represents methods that handle the ResponseReceived events
        /// </summary>
        /// <param name="response"></param>
        protected delegate void ResponseReceivedEventHandler(string response);

        /// <summary>
        /// The password used for message authentication and/or encryption
        /// </summary>
        private string password;

        /// <summary>
        /// The hostname of the Growl instance to connect to [defaults to "localhost"]
        /// </summary>
        private string hostname = "localhost";

        /// <summary>
        /// The port of the Growl instance to connect to [defaults to the GNTP standard]
        /// </summary>
        private int port = TCP_PORT;

        /// <summary>
        /// The algorithm to use when generating hashes
        /// </summary>
        private Cryptography.HashAlgorithmType keyHashAlgorithm = Cryptography.HashAlgorithmType.MD5;

        /// <summary>
        /// The algorithm to use when doing encryption
        /// </summary>
        private Cryptography.SymmetricAlgorithmType encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.AES;

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

        /// <summary>
        /// Creates a new instance of the class, using the default hostname and port,
        /// with no password.
        /// </summary>
        public ConnectorBase()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a new instance of the class, using the default hostname and port,
        /// using the supplied password.
        /// </summary>
        /// <param name="password">The password used for message authentication and/or encryption</param>
        public ConnectorBase(string password)
        {
            this.Password = password;
        }

        /// <summary>
        /// Creates a new instance of the class using the supplied hostname, port and password.
        /// </summary>
        /// <param name="password">The password used for message authentication and/or encryption</param>
        /// <param name="hostname">The hostname of the Growl instance to connect to</param>
        /// <param name="port">The port of the Growl instance to connect to</param>
        public ConnectorBase(string password, string hostname, int port)
            : this(password)
        {
            this.hostname = hostname;
            this.port = port;
        }

        /// <summary>
        /// Gets or sets the password used for message authentication and/or encryption
        /// </summary>
        /// <value>string</value>
        /// <remarks>
        /// See the <see cref="Key"/> class for details on how the password is
        /// expanded into an encryption key.
        /// </remarks>
        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                string password = value;
                if (password != null) password = password.Trim();
                if (password != String.Empty)
                    this.password = password;
                else
                    this.password = null;
            }
        }

        /// <summary>
        /// Gets or sets the algorithm used when hashing values
        /// </summary>
        /// <value>
        /// <see cref="Cryptography.HashAlgorithmType"/>
        /// </value>
        public Cryptography.HashAlgorithmType KeyHashAlgorithm
        {
            get
            {
                return this.keyHashAlgorithm;
            }
            set
            {
                this.keyHashAlgorithm = value;
            }
        }

        /// <summary>
        /// Gets or sets the algorithm used when encrypting values
        /// </summary>
        /// <value>
        /// <see cref="Cryptography.SymmetricAlgorithmType"/>
        /// </value>
        public Cryptography.SymmetricAlgorithmType EncryptionAlgorithm
        {
            get
            {
                return this.encryptionAlgorithm;
            }
            set
            {
                this.encryptionAlgorithm = value;
            }
        }

        /// <summary>
        /// Generates a unique <see cref="Key"/> using the supplied password.
        /// </summary>
        /// <returns><see cref="Key"/></returns>
        /// <remarks>
        /// See the <see cref="Key"/> class for details on how the password
        /// is expanded into a key.
        /// </remarks>
        protected Key GetKey()
        {
            Key key = Key.GenerateKey(this.password, this.keyHashAlgorithm, this.encryptionAlgorithm);
            return key;
        }

        /// <summary>
        /// Parses the response and raises the appropriate event
        /// </summary>
        /// <param name="responseText">The raw GNTP response</param>
        protected abstract void OnResponseReceived(string responseText);

        /// <summary>
        /// Occurs when any of the following network conditions occur:
        ///     1. Unable to connect to target host for any reason
        ///     2. Write request fails
        ///     3. Read request fails
        /// </summary>
        /// <param name="response">The <see cref="Response"/> that contains information about the failure</param>
        protected abstract void OnCommunicationFailure(Response response);

        /// <summary>
        /// Fired immediately before the message is constructed and set.
        /// Allows adding any additional headers to the outgoing request or
        /// to cancel the request.
        /// </summary>
        /// <param name="mb">The <see cref="MessageBuilder"/> used to construct the message</param>
        /// <returns>
        /// <c>true</c> to allow the request to be sent;
        /// <c>false</c> to cancel the request
        /// </returns>
        protected virtual bool OnBeforeSend(MessageBuilder mb)
        {
            return true;
        }

        /// <summary>
        /// Constructs the actual message and sends it to Growl
        /// </summary>
        /// <param name="mb">The <see cref="MessageBuilder"/> used to construct the message</param>
        /// <param name="del">The <see cref="ResponseReceivedEventHandler"/> that represents the method that will handle the response</param>
        /// <param name="waitForCallback"><c>true</c> if the connection should wait for a callback;<c>false</c> otherwise</param>
        protected void Send(MessageBuilder mb, ResponseReceivedEventHandler del, bool waitForCallback)
        {
            ConnectState state = null;
            try
            {
                bool doSend = this.OnBeforeSend(mb);

                if (doSend)
                {
                    TcpClient client = new TcpClient();
                    byte[] bytes = mb.GetBytes();
                    mb = null;
                    AsyncCallback callback = new AsyncCallback(ConnectCallback);
                    state = new ConnectState(client, bytes, del, waitForCallback);
                    connecting.Add(state.GUID, state);
                    client.BeginConnect(this.hostname, this.port, callback, state);
                }
            }
            catch
            {
                // suppress
                // could mean growl is not installed, not running, wrong address, etc
                CleanUpSocket(state);
                OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.CONNECTION_FAILURE));
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
                OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.CONNECTION_FAILURE));
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
                OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.WRITE_FAILURE));
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
            bool waitForCallback = false;
            try
            {
                state = (TcpState)iar.AsyncState;
                int length = state.Stream.EndRead(iar);

                if (length > 0)
                {
                    string response = System.Text.Encoding.UTF8.GetString(state.Buffer, 0, length);

                    if (state.Delegate != null)
                        state.Delegate(response);

                    // wait for more data
                    if (state.WaitForCallback)
                    {
                        waitForCallback = true;
                        state.WaitForCallback = false;
                        state.Buffer = new byte[4096];
                        AsyncCallback callback = new AsyncCallback(ReadCallback);
                        state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, callback, state);
                        return;
                    }
                }
            }
            catch
            {
                CleanUpSocket(state);
                OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.READ_FAILURE));
            }
            finally
            {
                if (state != null && !waitForCallback)
                {
                    CleanUpSocket(state);
                }
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

                    TcpState tcpState = state as TcpState;
                    if (tcpState != null)
                    {
                        if (tcpState.Stream != null) tcpState.Stream.Close();
                        tcpState.Stream = null;
                    }
                    if (state.Client != null) state.Client.Close();
                    state.Client = null;
                }
            }
            catch
            {
            }
            state = null;
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
            /// <param name="del">The <see cref="ResponseReceivedEventHandler"/> method to call to handle the response</param>
            /// <param name="waitForCallback"><c>true</c> if the connection should wait for a callback;<c>false</c> otherwise</param>
            public ConnectState(TcpClient client, byte[] bytes, ResponseReceivedEventHandler del, bool waitForCallback)
            {
                this.Client = client;
                this.Bytes = bytes;
                this.Delegate = del;
                this.WaitForCallback = waitForCallback;
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

            /// <summary>
            /// The <see cref="ResponseReceivedEventHandler"/> method to call to handle the response
            /// </summary>
            public ResponseReceivedEventHandler Delegate;

            /// <summary>
            /// Indicates if the connection should wait for a callback after receiving the initial response.
            /// </summary>
            public bool WaitForCallback = false;
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
                state.Delegate = cs.Delegate;
                state.WaitForCallback = cs.WaitForCallback;
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
        }
    }
}
