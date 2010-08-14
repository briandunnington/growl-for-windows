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
    /// Reads GNTP data over a standard TCP connection.
    /// </summary>
    public class GNTPSocketReader : GNTPRequestReader
    {
        private const int TIMEOUT_GNTP_HEADER = -1;
        private const int TIMEOUT_GNTP_BINARY = -1;

        /// <summary>
        /// The <see cref="AsyncSocket"/> making the request
        /// </summary>
        AsyncSocket socket;

        /// <summary>
        /// Parses the GNTP data, extracting data and checking for validity
        /// </summary>
        GNTPParser parser;


        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPSocketReader"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/></param>
        /// <param name="passwordManager">The <see cref="PasswordManager"/> containing a list of allowed passwords</param>
        /// <param name="passwordRequired">Indicates if a password is required</param>
        /// <param name="allowNetworkNotifications">Indicates if network requests are allowed</param>
        /// <param name="allowBrowserConnections">Indicates if browser requests are allowed</param>
        /// <param name="allowSubscriptions">Indicates if SUBSCRIPTION requests are allowed</param>
        /// <param name="requestInfo">The <see cref="RequestInfo"/> associated with this request</param>
        public GNTPSocketReader(AsyncSocket socket, PasswordManager passwordManager, bool passwordRequired, bool allowNetworkNotifications, bool allowBrowserConnections, bool allowSubscriptions, RequestInfo requestInfo)
        {
            this.parser = new GNTPParser(passwordManager, passwordRequired, allowNetworkNotifications, allowBrowserConnections, allowSubscriptions, requestInfo);
            parser.Error += new GNTPParser.GNTPParserErrorEventHandler(parser_Error);
            parser.MessageParsed += new GNTPParser.GNTPParserMessageParsedEventHandler(parser_MessageParsed);

            this.socket = socket;
            this.socket.Tag = parser;
        }

        /// <summary>
        /// Gets the <see cref="AsyncSocket"/> associated with this request
        /// </summary>
        /// <value>The <see cref="AsyncSocket"/>.</value>
        protected AsyncSocket Socket
        {
            get
            {
                return this.socket;
            }
        }

        /// <summary>
        /// Reads the socket data and handles the request
        /// </summary>
        /// <param name="alreadyReadBytes">Any bytes that were already read from the socket</param>
        public virtual void Read(byte[] alreadyReadBytes)
        {
            socket.DidRead += new AsyncSocket.SocketDidRead(this.SocketDidRead);
            SocketDidRead(this.socket, alreadyReadBytes, 0);
        }

        /// <summary>
        /// Handles the socket's DidRead event.
        /// </summary>
        /// <param name="socket">The <see cref="AsyncSocket"/></param>
        /// <param name="readBytes">Array of <see cref="byte"/>s that were read</param>
        /// <param name="tag">The tag identifying the read operation</param>
        protected virtual void SocketDidRead(AsyncSocket socket, byte[] readBytes, long tag)
        {
            try
            {
                Data data = new Data(readBytes);
                this.AlreadyReceivedData.Append(data.ToString());

                GNTPParser parser = (GNTPParser)socket.Tag;
                NextIndicator next = parser.Parse(readBytes);
                if (next.ShouldContinue)
                {
                    if (next.UseBytes)
                        socket.Read(next.Bytes, TIMEOUT_GNTP_HEADER, parser.Tag);
                    else
                        socket.Read(next.Length, TIMEOUT_GNTP_BINARY, parser.Tag);
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
        /// Handles the parser's <see cref="GNTPParser.MessageParsed"/> event
        /// </summary>
        /// <param name="request">The parsed <see cref="GNTPRequest"/></param>
        void parser_MessageParsed(GNTPRequest request)
        {
            CleanUp();
            this.DecryptedData = parser.DecryptedRequest;
            this.OnMessageParsed(request);
        }

        /// <summary>
        /// Handles the parser's <see cref="GNTPParser.Error"/> event
        /// </summary>
        /// <param name="error">The <see cref="Error"/> information</param>
        void parser_Error(Error error)
        {
            CleanUp();
            this.OnError(error.ErrorCode, error.ErrorDescription);
        }

        /// <summary>
        /// Cleans up things by unhooking event handlers.
        /// [This might not be needed, but i am leaving it for now]
        /// </summary>
        private void CleanUp()
        {
            socket.DidRead -= new AsyncSocket.SocketDidRead(this.SocketDidRead);
            parser.Error -= new GNTPParser.GNTPParserErrorEventHandler(parser_Error);
            parser.MessageParsed -= new GNTPParser.GNTPParserMessageParsedEventHandler(parser_MessageParsed);
        }
    }
}
