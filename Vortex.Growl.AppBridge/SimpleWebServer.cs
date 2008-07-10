using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Vortex.Growl.AppBridge
{
    internal class SimpleWebServer
    {
        private const string HTTP_STATUS_CODE_OK = "200";
        private const string HTTP_STATUS_TEXT_OK = "OK";
        private const string HTTP_STATUS_CODE_ERROR = "404";
        private const string HTTP_STATUS_TEXT_ERROR = "NOT FOUND";
        private const string RESPONSE_OK_TEXT = "ok";
        private const string RESPONSE_ERROR_TEXT = "error";

        public delegate string RequestHandler(string request, string receivedFrom);
        public event RequestHandler RequestReceived;

        private int port;
        private TcpListener tcp;
        private bool isRunning = false;

        public SimpleWebServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            try
            {
                // if already running, skip the rest of this
                if (!this.IsRunning)
                {
                    this.tcp = null;

                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, this.port);
                    this.tcp = new TcpListener(endpoint);

                    ParameterizedThreadStart ts = new ParameterizedThreadStart(StartServerOnNewThread);
                    Thread t = new Thread(ts);
                    this.isRunning = true;
                    t.Start(this.tcp);
                }
            }
            catch
            {
                // suppress any exceptions
            }
        }

        public void Stop()
        {
            try
            {
                this.isRunning = false;
                if (this.tcp != null)
                {
                    this.tcp.Stop();
                }
            }
            catch
            {
                // suppress any exceptions
            }
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        private void StartServerOnNewThread(object obj)
        {
            try
            {
                if (obj is TcpListener)
                {
                    TcpListener listener = (TcpListener)obj;
                    listener.Start();
                    while (this.isRunning)
                    {
                        TcpClient client = this.tcp.AcceptTcpClient();
                        ParameterizedThreadStart ts = new ParameterizedThreadStart(ProcessRequestInNewThread);
                        Thread t = new Thread(ts);
                        t.Start(client);
                    }
                }
            }
            catch
            {
                // suppress any exceptions here
            }
        }

        private void ProcessRequestInNewThread(object obj)
        {
            if (obj is TcpClient)
            {
                TcpClient client = (TcpClient) obj;
                NetworkStream stream = null;

                try
                {
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, this.port);
                    stream = client.GetStream();

                    // read the request
                    string request = null;
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    while (stream.DataAvailable)
                    {
                        stream.Read(buffer, 0, buffer.Length);
                        request += Encoding.ASCII.GetString(buffer);
                    }
                    request = request.Trim();

                    // write the response
                    string response = this.OnRequestReceived(request, endpoint.ToString());
                    byte[] responseBytes = GenerateResponseBytes(HTTP_STATUS_CODE_OK, HTTP_STATUS_TEXT_OK, response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    stream.Flush();
                }
                catch
                {
                    try
                    {
                        // write the error response
                        if (stream != null && stream.CanWrite)
                        {
                            byte[] responseBytes = GenerateResponseBytes(HTTP_STATUS_CODE_ERROR, HTTP_STATUS_TEXT_ERROR, RESPONSE_ERROR_TEXT);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                            stream.Flush();
                        }
                    }
                    catch
                    {
                        // error handling the error
                        Console.WriteLine("ERROR");
                    }
                }
                finally
                {
                    // make sure to clean up
                    try
                    {
                        if (stream != null) stream.Close();
                        if (client != null) client.Close();
                    }
                    catch
                    {
                        // suppress any exceptions
                    }
                }

            }
        }

        protected string OnRequestReceived(string request, string receivedFrom)
        {
            if (this.RequestReceived != null)
            {
                return this.RequestReceived(request, receivedFrom);
            }
            return RESPONSE_OK_TEXT;
        }

        private byte[] GenerateResponseBytes(string statusCode, string statusText, string content)
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
