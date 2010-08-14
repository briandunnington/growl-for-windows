using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Growl.CoreLibrary;
using Growl.Connector;

namespace Growl.Daemon
{
    /// <summary>
    /// Handles Flash policy requests
    /// </summary>
    class GNTPFlashSocketReader : GNTPRequestReader
    {
        private const long FLASH_POLICY_REQUEST_TAG = 3001;
        private const long FLASH_POLICY_RESPONSE_TAG = 3002;

        private const int TIMEOUT_FLASHPOLICYREQUEST = -1;
        private const int TIMEOUT_FLASHPOLICYRESPONSE = -1;

        /// <summary>
        /// The <see cref="AsyncSocket"/> making the request
        /// </summary>
        private AsyncSocket socket;

        /// <summary>
        /// Indicates if Flash requests are allowed
        /// </summary>
        private bool allowFlash;


        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPFlashSocketReader"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/> making the request</param>
        /// <param name="allowFlash">Indicates if Flash requests are allowed</param>
        public GNTPFlashSocketReader(AsyncSocket socket, bool allowFlash)
        {
            this.socket = socket;
            this.allowFlash = allowFlash;
        }

        /// <summary>
        /// Reads the socket data and handles the request
        /// </summary>
        /// <param name="alreadyReadBytes">Any bytes that were already read from the socket</param>
        public void Read(byte[] alreadyReadBytes)
        {
            Data data = new Data(alreadyReadBytes);
            string s = data.ToString();
            this.AlreadyReceivedData.Append(s);

            this.socket.DidRead += new AsyncSocket.SocketDidRead(ReadPolicyRequest);
            this.socket.Read(FlashPolicy.REQUEST.Length - s.Length, TIMEOUT_FLASHPOLICYREQUEST, FLASH_POLICY_REQUEST_TAG);
        }

        /// <summary>
        /// Reads the Flash policy request and returns the policy response if Flash requests are allowed.
        /// </summary>
        /// <param name="sender">The <see cref="AsyncSocket"/></param>
        /// <param name="readBytes">The bytes read from the socket</param>
        /// <param name="tag">A tag identifying the read request</param>
        void ReadPolicyRequest(AsyncSocket sender, byte[] readBytes, long tag)
        {
            // remove this now that we are done with it
            this.socket.DidRead -= new AsyncSocket.SocketDidRead(ReadPolicyRequest);

            if (tag == FLASH_POLICY_REQUEST_TAG)
            {
                Data data = new Data(readBytes);
                this.AlreadyReceivedData.Append(data.ToString());

                string request = this.AlreadyReceivedData.ToString();
                if (request == FlashPolicy.REQUEST)
                {
                    if (this.allowFlash)
                    {
                        this.socket.Write(FlashPolicy.ResponseBytes, TIMEOUT_FLASHPOLICYRESPONSE, FLASH_POLICY_RESPONSE_TAG);
                        this.socket.CloseAfterWriting();
                    }
                    else
                    {
                        OnError(ErrorCode.NOT_AUTHORIZED, ErrorDescription.FLASH_CONNECTIONS_NOT_ALLOWED);
                    }
                }
                else
                {
                    OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.UNRECOGNIZED_REQUEST);
                }
            }
        }
    }
}
