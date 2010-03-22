using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Growl
{
    public class CometClient : IDisposable
    {
        public delegate void ResponseReceivedEventHandler(string response);

        public event ResponseReceivedEventHandler ResponseReceived;
        public event EventHandler Disconnected;
        public event EventHandler Connected;

        private object locker = new object();
        private bool isWaiting;
        private string url;
        private int reconnectDelay = 1; // in seconds
        private DateTime nextReconnectTime = DateTime.MinValue;
        private int autoResetInterval = 15 * 60 * 1000;

        public readonly string GUID = System.Guid.NewGuid().ToString();

        /// <summary>
        /// A collection of active ConnectState objects awaiting the EndConnect callback
        /// </summary>
        private Dictionary<string, ConnectState> connecting = new Dictionary<string, ConnectState>();

        /// <summary>
        /// A collection of active TcpState objects awaiting the EndRead callback
        /// </summary>
        private Dictionary<string, ResponseState> reading = new Dictionary<string, ResponseState>();


        public CometClient(string url)
        {
            this.url = url;
        }

        public void Start()
        {
            Utility.WriteDebugInfo("Comet Client Starting");

            ConnectState state = null;
            try
            {
                if (!this.isWaiting)
                {
                    lock (this.locker)
                    {
                        if (!this.isWaiting)
                        {
                            // well, we arent really waiting yet, but the connecting is considered part of it
                            // since another attempt should not be made while we are trying this attempt
                            this.isWaiting = true;

                            // dont reconnect too quickly or it causes HttpWebRequest exceptions
                            DateTime now = DateTime.Now;
                            if (now < this.nextReconnectTime)
                            {
                                int wait = (this.nextReconnectTime - now).Milliseconds;
                                System.Threading.Thread.Sleep(wait);
                            }

                            // build HTTP request
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.url);
                            request.AllowWriteStreamBuffering = false;
                            request.Pipelined = true;
                            state = new ConnectState(request);
                            connecting.Add(state.GUID, state);
                            request.Method = "GET";
                            request.UserAgent = "notify.io Windows Client";
                            request.Timeout = this.autoResetInterval;

                            // deal with a bug
                            request.KeepAlive = false;
                            request.ServicePoint.MaxIdleTime = this.reconnectDelay * 1000;

                            Utility.WriteDebugInfo("CometClient waiting: TRUE");
                            HttpWebRequestHelper hwrh = new HttpWebRequestHelper(request);
                            hwrh.GetResponseAsync(ConnectCallback, state);
                            
                            this.OnConnected();
                        }
                        else
                        {
                            //Utility.WriteLine("already connecting 2");
                        }
                    }
                }
                else
                {
                    //Utility.WriteLine("already connecting 1");
                }
            }
            catch
            {
                //Utility.WriteLine("EXCEPTION - CometClient.Start");

                // suppress
                OnDisconnected();
            }
        }

        public void Stop()
        {
            Utility.WriteDebugInfo("Comet Client stopping");

            try
            {
                while (this.connecting.Count > 0)
                {
                    // this is not the best way to get a single item from a dictionary, but we dont know the key...
                    foreach (ConnectState state in this.connecting.Values)
                    {
                        CleanUpSocket(state);
                        break;
                    }
                }

                while (this.reading.Count > 0)
                {
                    // this is not the best way to get a single item from a dictionary, but we dont know the key...
                    foreach (ConnectState state in this.reading.Values)
                    {
                        CleanUpSocket(state);
                        break;
                    }
                }
            }
            catch
            {
                Utility.WriteDebugInfo("EXCEPTION - CometClient.Stop");
            }

            // OnDisconnect will get called automatically when any open readers are closed, so dont call it here
        }

        private void ConnectCallback(HttpWebRequest request, HttpWebResponse response, object state)
        {
            ConnectState connectState = null;
            ResponseState responseState = null;
            try
            {
                connectState = (ConnectState) state;

                // Read the response into a Stream object.
                Stream stream = (Stream)response.GetResponseStream();
                stream.ReadTimeout = this.autoResetInterval;

                byte[] buffer = new byte[4096];
                responseState = ResponseState.FromConnectState(connectState, stream, buffer);
                reading.Add(responseState.GUID, responseState);

                AsyncCallback callback = new AsyncCallback(ReadCallback);
                stream.BeginRead(responseState.Buffer, 0, responseState.Buffer.Length, callback, responseState);
                Utility.WriteDebugInfo("CometClient waiting: " + this.isWaiting.ToString());
            }
            catch
            {
                Utility.WriteDebugInfo("EXCEPTION - CometClient.ConnectCallback");

                OnDisconnected();
            }
            finally
            {
                if (connectState != null) connecting.Remove(connectState.GUID);
            }
        }

        /// <summary>
        /// Called once the response has been read. If the transaction is complete, the connection is closed.
        /// If there is an outstanding callback, the connection waits to read it before closing.
        /// </summary>
        /// <param name="iar">The result of the asynchronous operation.</param>
        private void ReadCallback(IAsyncResult iar)
        {
            ResponseState state = null;
            string response = null;

            try
            {
                state = (ResponseState)iar.AsyncState;

                int length = 0;
                if(state != null && state.Stream != null)
                    length = state.Stream.EndRead(iar);

                if (length > 0)
                {
                    response = System.Text.Encoding.UTF8.GetString(state.Buffer, 0, length);

                    // we dont get the length for some reason, so lets add it
                    response = String.Format("{0}\r\n{1}", response.Length.ToString("X"), response);
                    state.HasReceivedData = true;

                    state.Response += response.Trim(); ;

                    OnResponseReceived(state.Response);
                    state.Response = null;  // reset

                    // keep listening for more
                    AsyncCallback callback = new AsyncCallback(ReadCallback);
                    state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, callback, state);
                    this.isWaiting = true;
                }

                else
                {
                    Utility.WriteDebugInfo("Length was zero - this should not happen");
                    CleanUpSocket(state);
                    OnDisconnected();
                }
            }
            catch
            {
                Utility.WriteDebugInfo("EXCEPTION - CometClient.ReadCallback");

                OnDisconnected();
            }
            finally
            {
                Utility.WriteDebugInfo("CometClient waiting: " + this.isWaiting.ToString());
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
                    if (reading.ContainsKey(state.GUID)) reading.Remove(state.GUID);

                    if (state.Request != null)
                    {
                        state.Request.Abort();
                    }

                    ResponseState responseState = state as ResponseState;
                    if (responseState != null)
                    {
                        if (responseState.Stream != null)
                        {
                            responseState.Stream.Close();
                        }
                        responseState.Stream = null;
                    }

                    state.Request = null;
                }
            }
            catch
            {
                Utility.WriteDebugInfo("CometClient cleanup socket failed");
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
            this.isWaiting = false;
            this.nextReconnectTime = DateTime.Now.AddSeconds(this.reconnectDelay);

            Stop();

            if (this.Disconnected != null)
            {
                this.Disconnected(this, EventArgs.Empty);
            }
        }

        protected void OnConnected()
        {
            if (this.Connected != null)
            {
                this.Connected(this, EventArgs.Empty);
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
            public ConnectState(HttpWebRequest request)
            {
                this.Request = request;
            }

            /// <summary>
            /// Uniquely identifies this instance
            /// </summary>
            public readonly string GUID = System.Guid.NewGuid().ToString();

            /// <summary>
            /// The <see cref="TcpClient"/> used to make the connection
            /// </summary>
            public HttpWebRequest Request;
        }

        /// <summary>
        /// Contains state information for a connection that has already connected and is either writing
        /// or reading data.
        /// </summary>
        private class ResponseState : ConnectState
        {
            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            private ResponseState()
            {
            }

            /// <summary>
            /// Creates a new instance of the TcpState class from an exisiting <see cref="ConnectState"/>.
            /// </summary>
            /// <param name="cs">The <see cref="ConnectState"/></param>
            /// <param name="stream">The <see cref="Stream"/> of the connection</param>
            /// <param name="buffer">The buffer to hold the response</param>
            /// <returns><see cref="TcpState"/></returns>
            public static ResponseState FromConnectState(ConnectState cs, Stream stream, byte[] buffer)
            {
                ResponseState state = new ResponseState();
                state.Request = cs.Request;
                state.Stream = stream;
                state.Buffer = buffer;
                return state;
            }

            /// <summary>
            /// The <see cref="NetworkStream"/> of the connection
            /// </summary>
            public Stream Stream;

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

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Stop();
                }
            }
            catch
            {
                // never fail in Dispose
            }
        }

        #endregion
    }
}
