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
        private const int TIMEOUT_FRAME = -1;

        private const long CONNECTION_ESTABLISHED_TAG = 2000;
        private const long BEGIN_FRAMING_TAG = 2001;
        private const long PAYLOAD_LENGTH_TAG = 2002;
        private const long MASKING_KEY_TAG = 2003;
        private const long GNTP_DATA_TAG = 2005;

        const byte BYTE_FIN_MORE = 0;
        const byte BYTE_FIN_FINAL = 128;
        const byte BYTE_OPCODE_CONTINUATION = 0;
        const byte BYTE_OPCODE_TEXT = 1;
        const byte BYTE_OPCODE_BINARY = 2;
        const byte BYTE_OPCODE_CLOSE = 8;
        const byte BYTE_OPCODE_PING = 9;
        const byte BYTE_OPCODE_PONG = 10;
        const byte BYTE_MASKED_NO = 0;
        const byte BYTE_MASKED_YES = 128;
        const byte BYTE_LENGTH_7 = 125;
        const byte BYTE_LENGTH_16 = 126;
        const byte BYTE_LENGTH_64 = 127;

        bool allowed = false;

        bool masked = false;
        byte[] mask = null;
        long payloadRemainingLength = 0;

        GNTPParser2 parser;

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
            this.allowed = allowBrowserConnections;

            parser = new GNTPParser2(passwordManager, passwordRequired, allowNetworkNotifications, allowBrowserConnections, allowSubscriptions, requestInfo);
            parser.MessageParsed += new GNTPParser2.GNTPParserMessageParsedEventHandler(parser_MessageParsed);
            parser.Error += new GNTPParser2.GNTPParserErrorEventHandler(parser_Error);
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
            Socket.DidRead -= new AsyncSocket.SocketDidRead(this.SocketDidRead);
            parser.Error -= new GNTPParser2.GNTPParserErrorEventHandler(parser_Error);
            parser.MessageParsed -= new GNTPParser2.GNTPParserMessageParsedEventHandler(parser_MessageParsed);
        }

        /// <summary>
        /// Reads the socket data and handles the request
        /// </summary>
        /// <param name="alreadyReadBytes">Any bytes that were already read from the socket</param>
        public override void Read(byte[] alreadyReadBytes)
        {
            if (this.allowed)
            {
                this.Socket.DidRead += new AsyncSocket.SocketDidRead(this.SocketDidRead);
                SocketDidRead(this.Socket, alreadyReadBytes, CONNECTION_ESTABLISHED_TAG);
            }
            else
            {
                OnError(ErrorCode.NOT_AUTHORIZED, ErrorDescription.BROWSER_CONNECTIONS_NOT_ALLOWED);
            }
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
                int numberOfBytesToReadNext = 0;
                long nextTag = 0;

                if (tag == CONNECTION_ESTABLISHED_TAG)
                {
                    // wait for a frame and read the first two bytes (containing the FIN, opcode, masking info, and payload length)
                    this.Socket.Read(2, TIMEOUT_UNLIMITED, BEGIN_FRAMING_TAG);
                }

                else if (tag == BEGIN_FRAMING_TAG)
                {
                    // reset this
                    this.AlreadyReceivedData.Remove(0, this.AlreadyReceivedData.Length);
                    this.ParserTag = 1; //TODO:

                    byte bFrameControl = readBytes[0];
                    byte bDataControl = readBytes[1];

                    bool end = (bFrameControl & BYTE_FIN_FINAL) == BYTE_FIN_FINAL;
                    // TODO: handle fragments

                    if ((bFrameControl & BYTE_OPCODE_PONG) == BYTE_OPCODE_PONG)
                    {
                        // unsolicited Pong - we dont need to do anything
                    }
                    else if ((bFrameControl & BYTE_OPCODE_PING) == BYTE_OPCODE_PING)
                    {
                        // need to response with Pong
                        // TODO: handle this case
                    }
                    else if ((bFrameControl & BYTE_OPCODE_CLOSE) == BYTE_OPCODE_CLOSE)
                    {
                        // we dont really care about this
                    }
                    else if ((bFrameControl & BYTE_OPCODE_BINARY) == BYTE_OPCODE_BINARY)
                    {
                        // we dont really care about this
                    }
                    else if ((bFrameControl & BYTE_OPCODE_TEXT) == BYTE_OPCODE_TEXT)
                    {
                        // we dont really care about this
                    }
                    else if ((bFrameControl & BYTE_OPCODE_CONTINUATION) == BYTE_OPCODE_CONTINUATION)
                    {
                        // TODO: handle fragments
                    }

                    masked = (bDataControl & BYTE_MASKED_YES) == BYTE_MASKED_YES;
                    int length = (int)(bDataControl & ~BYTE_MASKED_YES);
                    if ((bDataControl & BYTE_LENGTH_16) == BYTE_LENGTH_16)
                    {
                        // read next two bytes for length
                        numberOfBytesToReadNext = 2;
                        nextTag = PAYLOAD_LENGTH_TAG;
                    }
                    else if ((bDataControl & BYTE_LENGTH_64) == BYTE_LENGTH_64)
                    {
                        // read next 8 bytes for length
                        numberOfBytesToReadNext = 8;
                        nextTag = PAYLOAD_LENGTH_TAG;
                    }
                    else
                    {
                        if (masked)
                        {
                            // read 4 byte masking key
                            numberOfBytesToReadNext = 4;
                            nextTag = MASKING_KEY_TAG;
                            payloadRemainingLength = length;
                        }
                        else
                        {
                            numberOfBytesToReadNext = length;
                            nextTag = GNTP_DATA_TAG;
                            payloadRemainingLength = 0;

                            // handle zero-length payload
                            if (numberOfBytesToReadNext == 0)
                            {
                                numberOfBytesToReadNext = 2;
                                nextTag = BEGIN_FRAMING_TAG;
                            }
                        }
                    }

                    // read next chunk of data
                    this.Socket.Read(numberOfBytesToReadNext, TIMEOUT_FRAME, nextTag);
                }
                else if (tag == PAYLOAD_LENGTH_TAG)
                {
                    long length = 0;
                    if (readBytes.Length == 2)
                    {
                        short s = BitConverter.ToInt16(readBytes, 0);
                        s = System.Net.IPAddress.NetworkToHostOrder(s);
                        length = s;
                    }
                    else if (readBytes.Length == 8)
                    {
                        long l = BitConverter.ToInt64(readBytes, 0);
                        l = System.Net.IPAddress.NetworkToHostOrder(l);
                        length = l;
                    }

                    if (masked)
                    {
                        // read 4 byte masking key
                        numberOfBytesToReadNext = 4;
                        nextTag = MASKING_KEY_TAG;
                        payloadRemainingLength = length;
                    }
                    else
                    {
                        if (length > int.MaxValue)
                        {
                            numberOfBytesToReadNext = int.MaxValue;
                            payloadRemainingLength = length - numberOfBytesToReadNext;
                        }
                        else
                        {
                            numberOfBytesToReadNext = (int)length;
                            payloadRemainingLength = 0;
                        }
                        nextTag = GNTP_DATA_TAG;

                        // handle zero-length payload
                        if (numberOfBytesToReadNext == 0)
                        {
                            numberOfBytesToReadNext = 2;
                            nextTag = BEGIN_FRAMING_TAG;
                        }
                    }

                    // read next chunk of data
                    this.Socket.Read(numberOfBytesToReadNext, TIMEOUT_FRAME, nextTag);
                }
                else if (tag == MASKING_KEY_TAG)
                {
                    mask = readBytes;

                    if (payloadRemainingLength > int.MaxValue)
                    {
                        numberOfBytesToReadNext = int.MaxValue;
                        payloadRemainingLength = payloadRemainingLength - numberOfBytesToReadNext;
                    }
                    else
                    {
                        numberOfBytesToReadNext = (int)payloadRemainingLength;
                        payloadRemainingLength = 0;
                    }
                    nextTag = GNTP_DATA_TAG;

                    // handle zero-length payload
                    if (numberOfBytesToReadNext == 0)
                    {
                        numberOfBytesToReadNext = 2;
                        nextTag = BEGIN_FRAMING_TAG;
                    }

                    // read next chunk of data
                    this.Socket.Read(numberOfBytesToReadNext, TIMEOUT_FRAME, nextTag);
                }
                else if (tag == GNTP_DATA_TAG)
                {
                    byte[] unmaskedBytes = null;
                    if (masked)
                    {
                        unmaskedBytes = new byte[readBytes.Length];
                        for(int i=0;i<readBytes.Length;i++)
                        {
                            int b = i % 4;
                            byte unmaskedByte = (byte) (readBytes[i] ^ mask[b]);
                            unmaskedBytes[i] = unmaskedByte;
                        }
                    }
                    else
                    {
                        unmaskedBytes = readBytes;
                    }

                    parser.Parse(unmaskedBytes);

                    // normally we would want to kick off another socket.Read() here (looking for the next BEGIN_FRAME_TAG), but we currently only allow one request per socket connection
                }
                else
                {
                    // we can only get here if there was some unaccounted-for data. that is bad, so lets close the socket
                    socket.Close();
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
            ByteBuilder bb = new ByteBuilder();

            // FIN and opcode
            byte bFrameControl = BYTE_FIN_FINAL + BYTE_OPCODE_TEXT;
            bb.Append(bFrameControl);

            // Mask and length
            long length = bytes.LongLength;
            if (length > short.MaxValue)
            {
                bb.Append(BYTE_LENGTH_64);
                bb.Append(length);
            }
            else if (length > (int) BYTE_LENGTH_7)
            {
                bb.Append(BYTE_LENGTH_16);
                bb.Append((short) length);
            }
            else
            {
                bb.Append((byte) length);
            }

            // actual GNTP bytes
            bb.Append(bytes);

            bytes = bb.GetBytes();


            /*
            // wrap the data with the framing
            byte[] wrappedArray = new byte[extraByteCount + bytes.Length];
            wrappedArray[0] = bFrameControl;
            Array.Copy(bDataControl, 0, wrappedArray, 1, bDataControl.Length);
            Array.Copy(bytes, 0, wrappedArray, extraByteCount, bytes.Length);
            bytes = wrappedArray;
             * */

            /* dont do this here - we might need to send callback data
            // send a Close frame
            byte[] bCloseBytes = new byte[2];
            bCloseBytes[0] = 128 + 8;   // FIN and Close
            bCloseBytes[1] = 0;

            wrappedArray = new byte[bCloseBytes.Length + bytes.Length];
            Array.Copy(bytes, 0, wrappedArray, 0, bytes.Length);
            Array.Copy(bCloseBytes, 0, wrappedArray, bytes.Length, bCloseBytes.Length);
            bytes = wrappedArray;
             * */

            /* write out the bits
            System.Collections.BitArray ba = new System.Collections.BitArray(bytes);
            for (int i = 0; i < ba.Count; i++)
            {
                if (i > 0 && i % 8 == 0) Console.WriteLine("");
                bool bit = ba.Get(i);
                Console.Write(bit ? 1 : 0);
            }
            Console.WriteLine();
             * */
        }
    }
}
