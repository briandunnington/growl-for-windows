using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Vortex.Growl.Framework
{
    /// <summary>
    /// A basic listener that listens for incoming UDP messages on the specified port
    /// and passes the event on to application code whenever a message is received.
    /// </summary>
    public class UdpListener
    {
        /// <summary>
        /// The port to listen for messages on
        /// </summary>
        protected int port;
        /// <summary>
        /// Indicates if messages from remote machines should be allowed or not
        /// </summary>
        protected bool localMessagesOnly = true;
        /// <summary>
        /// The underlying <see cref="UdpClient"/>
        /// </summary>
        protected UdpClient udp;
        /// <summary>
        /// Event handlder for the <see cref="PacketReceived"/> event
        /// </summary>
        /// <param name="bytes">The raw packet data</param>
        /// <param name="receivedFrom">The host that sent the message</param>
        public delegate void PacketHandler(byte[] bytes, string receivedFrom);
        /// <summary>
        /// Fires when a message is received
        /// </summary>
        public event PacketHandler PacketReceived;

        /// <summary>
        /// Creates a new <see cref="UdpListener"/>
        /// </summary>
        /// <param name="port">The port to listen for messages on</param>
        /// <param name="localMessagesOnly"><c>true</c> to only listen for messages from the local machine;<c>false</c> to listen for messages from any source</param>
        public UdpListener(int port, bool localMessagesOnly)
        {
            this.port = port;
            this.localMessagesOnly = localMessagesOnly;
        }

        /// <summary>
        /// Starts listening for messages on the specified port
        /// </summary>
        public void Start()
        {
            IPAddress address = (this.localMessagesOnly ? IPAddress.Loopback : IPAddress.Any);
            IPEndPoint endpoint = new IPEndPoint(address, this.port);
            this.udp = new UdpClient(endpoint);
            AsyncCallback callback = new AsyncCallback(this.ProcessPacket);

            UdpState state = new UdpState();
            state.Udp = udp;
            state.Endpoint = endpoint;
            state.Callback = callback;

            udp.BeginReceive(callback, state);
        }

        /// <summary>
        /// Stops listening for messages and frees the port
        /// </summary>
        public void Stop()
        {
            try
            {
                this.udp.Close();
                this.udp = null;
            }
            finally
            {
            }
        }

        /// <summary>
        /// When a message is received by the listener, the raw data is read from the packet
        /// and the <see cref="PacketReceived"/> event is fired.
        /// </summary>
        /// <param name="ar"><see cref="IAsyncResult"/></param>
        private void ProcessPacket(IAsyncResult ar)
        {
            try
            {
                UdpClient udp = (UdpClient)((UdpState)(ar.AsyncState)).Udp;
                IPEndPoint endpoint = (IPEndPoint)((UdpState)(ar.AsyncState)).Endpoint;
                AsyncCallback callback = (AsyncCallback)((UdpState)(ar.AsyncState)).Callback;

                byte[] bytes = udp.EndReceive(ar, ref endpoint);
                string receivedFrom = endpoint.ToString();

                // start listening again
                udp.BeginReceive(callback, ar.AsyncState);

                // bubble up the event
                if (this.PacketReceived != null) this.PacketReceived(bytes, receivedFrom);
            }
            catch
            {
                // swallow any exceptions (this handles the case when Growl is stopped while still listening for network notifications)
            }
        }

        /// <summary>
        /// Simple class to represent state when used with a UdpListener
        /// </summary>
        private class UdpState
        {
            public UdpClient Udp;
            public IPEndPoint Endpoint;
            public AsyncCallback Callback;
        }
    }
}
