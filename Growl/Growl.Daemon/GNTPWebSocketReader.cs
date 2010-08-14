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
    /// Reads GNTP data over a WebSocket connection
    /// </summary>
    public class GNTPWebSocketReader : GNTPSocketReader
    {
        private const int TIMEOUT_UNLIMITED = -1;
        private const int TIMEOUT_GNTP_HEADER = -1;
        private const int TIMEOUT_GNTP_BINARY = -1;

        private const long CONNECTION_ESTABLISHED_TAG = 2000;
        private const long BEGIN_FRAMING_TAG = 2001;
        private const long END_FRAMING_TAG = 2002;
        private const long GNTP_DATA_TAG = 2003;

        const byte FRAME_BEGIN = (byte)0;   // the special byte used to frame the start of a WebSocket transmission
        const byte FRAME_END = (byte)255;   // the special byte used to frame the end of a WebSocket transmission

        byte[] FRAME_BEGIN_INDICATOR = new byte[] { FRAME_BEGIN };
        byte[] FRAME_END_INDICATOR = new byte[] { FRAME_END };


        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPWebSocketReader"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/></param>
        /// <param name="passwordManager">The <see cref="PasswordManager"/> containing a list of allowed passwords</param>
        /// <param name="passwordRequired">Indicates if a password is required</param>
        /// <param name="allowNetworkNotifications">Indicates if network requests are allowed</param>
        /// <param name="allowBrowserConnections">Indicates if browser requests are allowed</param>
        /// <param name="allowSubscriptions">Indicates if SUBSCRIPTION requests are allowed</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with this request</param>
        public GNTPWebSocketReader(AsyncSocket socket, PasswordManager passwordManager, bool passwordRequired, bool allowNetworkNotifications, bool allowBrowserConnections, bool allowSubscriptions, RequestInfo requestInfo)
            : base(socket, passwordManager, passwordRequired, allowNetworkNotifications, allowBrowserConnections, allowSubscriptions, requestInfo)
        {

        }

        /// <summary>
        /// Reads the socket data and handles the request
        /// </summary>
        /// <param name="alreadyReadBytes">Any bytes that were already read from the socket</param>
        public override void Read(byte[] alreadyReadBytes)
        {
            this.Socket.DidRead += new AsyncSocket.SocketDidRead(this.SocketDidRead);
            SocketDidRead(this.Socket, alreadyReadBytes, CONNECTION_ESTABLISHED_TAG);
        }

        /// <summary>
        /// Handles the socket's DidRead event.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/></param>
        /// <param name="readBytes">Array of <see cref="byte"/>s that were read</param>
        /// <param name="tag">The tag identifying the read operation</param>
        protected override void SocketDidRead(AsyncSocket socket, byte[] readBytes, long tag)
        {
            try
            {
                if (tag == CONNECTION_ESTABLISHED_TAG)
                {
                    this.Socket.Read(FRAME_BEGIN_INDICATOR, TIMEOUT_UNLIMITED, BEGIN_FRAMING_TAG);
                }

                else if (tag == BEGIN_FRAMING_TAG)
                {
                    // reset this
                    this.AlreadyReceivedData.Remove(0, this.AlreadyReceivedData.Length);

                    // this emulates the GNTPSocketReader's first read
                    this.Socket.Read(4, TIMEOUT_GNTP_HEADER, GNTP_DATA_TAG);
                }

                else if (tag == END_FRAMING_TAG)
                {
                    // wait for another framed packet - NOTE: this currently will not be triggered or useful since GfW doesnt allow socket re-use
                    this.Socket.Read(FRAME_BEGIN_INDICATOR, TIMEOUT_UNLIMITED, BEGIN_FRAMING_TAG);
                }
                else
                {
                    // all other cases should occur within a framed packet, so they should be treated like GNTP data
                    base.SocketDidRead(this.Socket, readBytes, tag);
                }
            }
            catch (GrowlException gEx)
            {
                OnError(gEx.ErrorCode, gEx.Message, gEx.AdditionalInfo);
            }
            catch (Exception ex)
            {
                OnError(ErrorCode.INVALID_REQUEST, ErrorDescription.MALFORMED_REQUEST, ex.Message);
            }
        }

        /// <summary>
        /// Frames the response with the special WebSocket framing bytes before sending.
        /// </summary>
        /// <param name="bytes">The data to be sent.</param>
        public override void  BeforeResponse(ref byte[] bytes)
        {
            // wrap the array with the framing bytes
            byte[] wrappedArray = new byte[bytes.Length + 2];
            wrappedArray[0] = FRAME_BEGIN;
            wrappedArray[wrappedArray.Length - 1] = FRAME_END;
            Array.Copy(bytes, 0, wrappedArray, 1, bytes.Length);
            bytes = wrappedArray;
        }

        // TODO: consider removing this or making it useful
        private void CleanUp()
        {
            /*
            socket.Read(FRAME_END_INDICATOR, TIMEOUT_UNLIMITED, END_FRAMING_TAG);

            socket.DidRead -= new AsyncSocket.SocketDidRead(this.SocketDidRead);
            parser.Error -= new GNTPParser.MessageHandlerErrorEventHandler(parser_Error);
            parser.MessageParsed -= new GNTPParser.MessageHandlerMessageParsedEventHandler(parser_MessageParsed);
             * */
        }
    }
}
