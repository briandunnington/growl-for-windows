using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;

namespace Growl.Daemon
{
    /// <summary>
    /// Abstract base class for any classes that want to read GNTP messages over various transports.
    /// </summary>
    public abstract class GNTPRequestReader
    {
        /// <summary>
        /// Represents the method that will handle the <see cref="Error"/> event.
        /// </summary>
        public delegate void GNTPRequestReaderErrorEventHandler(Error error);

        /// <summary>
        /// Represents the method that will handle the <see cref="MessageParsed"/> event.
        /// </summary>
        /// <param name="request">The <see cref="MessageHandler"/> that parsed the message</param>
        public delegate void GNTPRequestReaderMessageParsedEventHandler(GNTPRequest request);

        /// <summary>
        /// Occurs when the MessageHandler is going to return an Error response
        /// </summary>
        public event GNTPRequestReaderErrorEventHandler Error;

        /// <summary>
        /// Occurs when the request has been successfully parsed
        /// </summary>
        public event GNTPRequestReaderMessageParsedEventHandler MessageParsed;

        private StringBuilder alreadyReceivedData;
        private string decryptedData;


        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPRequestReader"/> class.
        /// </summary>
        public GNTPRequestReader()
        {
            this.alreadyReceivedData = new StringBuilder();
        }

        /// <summary>
        /// Gets a <see cref="StringBuilder"/> used to hold data as it is read in.
        /// </summary>
        /// <value>The already received data.</value>
        protected StringBuilder AlreadyReceivedData
        {
            get
            {
                return this.alreadyReceivedData;
            }
        }

        /// <summary>
        /// Gets the received data.
        /// </summary>
        /// <value>The received data.</value>
        /// <remarks>
        /// The <see cref="MessageHandler"/> class uses this property to write the log file for GNTP requests.
        /// </remarks>
        public string ReceivedData
        {
            get
            {
                return this.alreadyReceivedData.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the decrypted data.
        /// </summary>
        /// <value>The decrypted data.</value>
        /// <remarks>
        /// For unencrypted requests, this value will be null.
        /// For encrypted requests, this will contain the full decrypted data.
        /// </remarks>
        public string DecryptedData
        {
            get
            {
                return this.decryptedData;
            }
            protected set
            {
                this.decryptedData = value;
            }
        }

        /// <summary>
        /// Called when a GNTP request is successfully parsed and no errors are found.
        /// </summary>
        /// <param name="request">The <see cref="GNTPRequest"/>.</param>
        protected void OnMessageParsed(GNTPRequest request)
        {
            if (this.MessageParsed != null)
            {
                this.MessageParsed(request);
            }
        }

        /// <summary>
        /// Called when the GNTP request is malformed or invalid.
        /// </summary>
        /// <param name="errorCode">The error code. See <see cref="ErrorCode"/></param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="args">Any additional arguments to append to the error message.</param>
        protected void OnError(int errorCode, string errorMessage, params object[] args)
        {
            // handle additional arguments
            if (args != null)
            {
                foreach (object arg in args)
                {
                    errorMessage += String.Format(" ({0})", arg);
                }
            }

            // fire event
            Error error = new Error(errorCode, errorMessage);
            if (this.Error != null)
            {
                this.Error(error);
            }
        }

        /// <summary>
        /// Called immediately before the <see cref="MessageHandler"/> write the response back to the 
        /// sender, this allows the request reader to modify the data right before it is sent.
        /// </summary>
        /// <param name="bytes">The data to be sent.</param>
        /// <remarks>
        /// Most readers will not need to implement this method. It is however useful for scenarios
        /// such as WebSockets that need to frame the response before sending.
        /// </remarks>
        public virtual void BeforeResponse(ref byte[] bytes)
        {
        }
    }
}
