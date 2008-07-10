using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestWebServer
{
    public partial class Form1 : Form
    {
        private TcpListener tcp;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IPAddress address = IPAddress.Loopback;
            IPEndPoint endpoint = new IPEndPoint(address, 9889);
            AsyncCallback callback = new AsyncCallback(this.PacketReceived);

            this.tcp = new TcpListener(endpoint);

            TcpState state = new TcpState();
            state.Tcp = this.tcp;
            state.Endpoint = endpoint;
            state.Callback = callback;

            tcp.Start();
            tcp.BeginAcceptTcpClient(callback, state);
        }

        private void PacketReceived(IAsyncResult ar)
        {
            System.Threading.ParameterizedThreadStart pts = new System.Threading.ParameterizedThreadStart(ProcessPacket);
            Thread thread = new Thread(pts);
            thread.Start(ar);

            // start listening again
            TcpListener tcp = (TcpListener)((TcpState)(ar.AsyncState)).Tcp;
            AsyncCallback callback = (AsyncCallback)((TcpState)(ar.AsyncState)).Callback;
            tcp.BeginAcceptTcpClient(callback, ar.AsyncState);
        }

        private void ProcessPacket(object obj)
        {
            try
            {
                IAsyncResult ar = (IAsyncResult)obj;
                TcpListener tcp = (TcpListener)((TcpState)(ar.AsyncState)).Tcp;
                IPEndPoint endpoint = (IPEndPoint)((TcpState)(ar.AsyncState)).Endpoint;
                AsyncCallback callback = (AsyncCallback)((TcpState)(ar.AsyncState)).Callback;

                TcpClient client = tcp.EndAcceptTcpClient(ar);
                NetworkStream stream = client.GetStream();

                string request = null;
                byte[] buffer = new byte[client.ReceiveBufferSize];
                while(stream.DataAvailable)
                {
                    stream.Read(buffer, 0, buffer.Length);
                    request += Encoding.ASCII.GetString(buffer);
                }
                request = request.Trim();

                // write response
                byte[] responseBytes = GenerateResponseBytes("200", "OK", "ok");
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();

                stream.Close();
                client.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Simple class to represent state when used with a TcpListener
        /// </summary>
        private class TcpState
        {
            public TcpListener Tcp;
            public IPEndPoint Endpoint;
            public AsyncCallback Callback;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (this.tcp != null)
                {
                    this.tcp.Stop();
                }
            }
            catch
            {
            }
        }

        public byte[] GenerateResponseBytes(string statusCode, string statusText, string content)
        {
            byte[] contentBytes = Encoding.ASCII.GetBytes(content);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("HTTP/1.1 {0} {1}\r\n", statusCode, statusText);
            sb.Append("Server: Growl WebBridge\r\n");
            sb.Append("Content-Type: text/plain\r\n");
            sb.Append("Accept-Ranges: bytes\r\n");
            sb.AppendFormat("Content-Length: {0}\r\n", contentBytes.Length);
            sb.Append("\r\n");
            sb.Append(content);

            byte[] responseBytes = Encoding.ASCII.GetBytes(sb.ToString());
            return responseBytes;
        }
    }
}