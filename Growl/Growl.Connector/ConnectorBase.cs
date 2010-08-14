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
    /// 
    /// NOTE: This class' ability to parse and receive GNTP responses is not currently able
    /// to handle responses with inline binary data.
    /// </remarks>
    public abstract class ConnectorBase
    {
        /// <summary>
        /// The standard TCP port that GNTP uses
        /// </summary>
        public const int TCP_PORT = 23053;

        // End-of-message indicator
        private const string EOM = "\r\n\r\n";

        /// <summary>
        /// Represents methods that handle the ResponseReceived events
        /// </summary>
        /// <param name="response"></param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected delegate void ResponseReceivedEventHandler(string response, object state);

        /// <summary>
        /// The password used for message authentication and/or encryption
        /// </summary>
        private string password;

        /// <summary>
        /// The hostname of the Growl instance to connect to [defaults to "127.0.0.1"]
        /// </summary>
        private string hostname = "127.0.0.1";

        /// <summary>
        /// The port of the Growl instance to connect to [defaults to the GNTP standard]
        /// </summary>
        private int port = TCP_PORT;

        /// <summary>
        /// The algorithm to use when generating hashes [defaults to MD5]
        /// </summary>
        private Cryptography.HashAlgorithmType keyHashAlgorithm = Cryptography.HashAlgorithmType.MD5;

        /// <summary>
        /// The algorithm to use when doing encryption [defaults to PlainText]
        /// </summary>
        private Cryptography.SymmetricAlgorithmType encryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText;

        /// <summary>
        /// Creates a new instance of the class, using the default hostname and port,
        /// with no password.
        /// </summary>
        protected ConnectorBase()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a new instance of the class, using the default hostname and port,
        /// using the supplied password.
        /// </summary>
        /// <param name="password">The password used for message authentication and/or encryption</param>
        protected ConnectorBase(string password)
        {
            this.Password = password;
        }

        /// <summary>
        /// Creates a new instance of the class using the supplied hostname, port and password.
        /// </summary>
        /// <param name="password">The password used for message authentication and/or encryption</param>
        /// <param name="hostname">The hostname of the Growl instance to connect to</param>
        /// <param name="port">The port of the Growl instance to connect to</param>
        protected ConnectorBase(string password, string hostname, int port)
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
        /// <see cref="Cryptography.HashAlgorithmType"/> - Defaults to <see cref="Cryptography.HashAlgorithmType.MD5"/>
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
        /// <see cref="Cryptography.SymmetricAlgorithmType"/> - Defaults to <see cref="Cryptography.SymmetricAlgorithmType.PlainText"/>
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
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected abstract void OnResponseReceived(string responseText, object state);

        /// <summary>
        /// Occurs when any of the following network conditions occur:
        ///     1. Unable to connect to target host for any reason
        ///     2. Write request fails
        ///     3. Read request fails
        /// </summary>
        /// <param name="response">The <see cref="Response"/> that contains information about the failure</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected abstract void OnCommunicationFailure(Response response, object state);

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
        /// Sends the request and handles any responses
        /// </summary>
        /// <param name="mb">The <see cref="MessageBuilder"/> used to contruct the request</param>
        /// <param name="del">The <see cref="ResponseReceivedEventHandler"/> for handling the response</param>
        /// <param name="waitForCallback"><c>true</c> to wait for a callback;<c>false</c> otherwise</param>
        /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
        protected void Send(MessageBuilder mb, ResponseReceivedEventHandler del, bool waitForCallback, object state)
        {
            // do some of this *before* we spin up a new thread so we can just throw an exception if the error occurs
            // *before* we even send the request (like when generating the message bytes)
            bool doSend = this.OnBeforeSend(mb);
            if (doSend)
            {
                byte[] bytes = mb.GetBytes();
                mb = null;

                // start a new thread for the network connection stuff
                ConnectionState cs = new ConnectionState(bytes, del, waitForCallback, state);
                ParameterizedThreadStart pts = new ParameterizedThreadStart(SendAsync);
                Thread t = new Thread(pts);
                t.Start(cs);
            }
        }

        /// <summary>
        /// Sends the request on a background thread.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <remarks>
        /// Using the built-in async methods (Begin*/End*) results in flakey behavior.
        /// Using the synchronous methods in another thread avoids the issue.
        /// </remarks>
        private void SendAsync(object obj)
        {
            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                ConnectionState cs = (ConnectionState)obj;
                byte[] bytes = cs.Bytes;
                ResponseReceivedEventHandler del = cs.Delegate;
                bool waitForCallback = cs.WaitForCallback;

                // connect
                try
                {
                    client = new TcpClient();
                    client.Connect(this.hostname, this.port);
                }
                catch
                {
                    OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.CONNECTION_FAILURE), cs.UserState);
                }

                // write
                try
                {
                    stream = client.GetStream();
                    stream.Write(bytes, 0, bytes.Length);
                }
                catch
                {
                    OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.WRITE_FAILURE), cs.UserState);
                }

                // read
                try
                {
                    string response = String.Empty;
                    byte[] buffer = new byte[4096];
                    while (!response.EndsWith(EOM, StringComparison.InvariantCulture))
                    {
                        int length = stream.Read(buffer, 0, buffer.Length);
                        if (length > 0)
                        {
                            response += System.Text.Encoding.UTF8.GetString(buffer, 0, length);
                        }
                        else
                        {
                            break;
                        }
                    }
                    del(response, cs.UserState);

                    // wait for callback
                    if (waitForCallback)
                    {
                        response = String.Empty;
                        buffer = new byte[4096];

                        while (!response.EndsWith(EOM, StringComparison.InvariantCulture))
                        {
                            int length = stream.Read(buffer, 0, buffer.Length);
                            if (length > 0)
                            {
                                response += System.Text.Encoding.UTF8.GetString(buffer, 0, length);
                            }
                            else
                            {
                                break;
                            }
                        }
                        del(response, cs.UserState);
                    }
                }
                catch
                {
                    OnCommunicationFailure(new Response(ErrorCode.NETWORK_FAILURE, ErrorDescription.READ_FAILURE), cs.UserState);
                }
            }
            catch (Exception ex)
            {
                Growl.CoreLibrary.DebugInfo.WriteLine(ex.ToString());
            }
            finally
            {
                try
                {
                    if (client != null && client.Client != null)
                    {
                        client.Client.Blocking = true;
                        try { client.Client.Shutdown(SocketShutdown.Both); }
                        catch { }
                        client.Client.Close();
                        client.Close();
                    }

                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }

                    client = null;
                }
                catch
                {
                    // suppress
                }
            }
        }


        /// <summary>
        /// Contains state information for a connection.
        /// </summary>
        private class ConnectionState
        {
            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            protected ConnectionState() { }

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            /// <param name="bytes">The request bytes to be written</param>
            /// <param name="del">The <see cref="ResponseReceivedEventHandler"/> method to call to handle the response</param>
            /// <param name="waitForCallback"><c>true</c> if the connection should wait for a callback;<c>false</c> otherwise</param>
            /// <param name="state">An optional state object that will be passed into the response events associated with this request</param>
            public ConnectionState(byte[] bytes, ResponseReceivedEventHandler del, bool waitForCallback, object state)
            {
                this.Bytes = bytes;
                this.Delegate = del;
                this.WaitForCallback = waitForCallback;
                this.UserState = state;
            }

            /// <summary>
            /// Uniquely identifies this instance
            /// </summary>
            public readonly string GUID = System.Guid.NewGuid().ToString();

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

            /// <summary>
            /// An optional state object that will be passed into the response events associated with this request
            /// </summary>
            public object UserState;
        }
    }
}
