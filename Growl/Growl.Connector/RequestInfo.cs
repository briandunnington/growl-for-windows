using System;
using System.Collections.Generic;

namespace Growl.Connector
{
    /// <summary>
    /// Represents metadata about a received request such as when it was received, by whom, etc.
    /// </summary>
    public class RequestInfo
    {
        /// <summary>
        /// The address from which the request was received
        /// </summary>
        private string receivedFrom;

        /// <summary>
        /// The address of the receiving server
        /// </summary>
        private string receivedBy;

        /// <summary>
        /// The time the request was received
        /// </summary>
        private DateTime timeReceived = DateTime.UtcNow;

        /// <summary>
        /// The name or type of the receiving server
        /// </summary>
        private string receivedWith;

        /// <summary>
        /// A unique request ID
        /// </summary>
        private string requestID = System.Guid.NewGuid().ToString();

        /// <summary>
        /// A list of 'Received' headers in the current request
        /// </summary>
        private List<Header> previousReceivedHeaders = new List<Header>();

        private List<string> handlingInfo = new List<string>();

        /// <summary>
        /// The address from which the request was received
        /// </summary>
        /// <remarks>
        /// This value may be an IP address or hostname, with or without port information.
        /// </remarks>
        /// <value>
        /// string
        /// </value>
        public string ReceivedFrom
        {
            get
            {
                return this.receivedFrom;
            }
            set
            {
                this.receivedFrom = value;
            }
        }

        /// <summary>
        /// The address of the receiving server
        /// </summary>
        /// <remarks>
        /// This value may be an IP address or hostname, with or without port information.
        /// </remarks>
        /// <value>
        /// string
        /// </value>
        public string ReceivedBy
        {
            get
            {
                return this.receivedBy;
            }
            set
            {
                this.receivedBy = value;
            }
        }

        /// <summary>
        /// The name or type of server that received the request
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string ReceivedWith
        {
            get
            {
                return this.receivedWith;
            }
            set
            {
                this.receivedWith = value;
            }
        }

        /// <summary>
        /// The time when the request was received
        /// </summary>
        /// <remarks>
        /// This value is in UTC
        /// </remarks>
        /// <value>
        /// string
        /// </value>
        public DateTime TimeReceived
        {
            get
            {
                return this.timeReceived;
            }
        }

        /// <summary>
        /// A unique ID that identifies the request
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string RequestID
        {
            get
            {
                return this.requestID;
            }
        }

        /// <summary>
        /// A list of exisiting 'Recevied' headers in the request
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<Header> PreviousReceivedHeaders
        {
            get
            {
                return this.previousReceivedHeaders;
            }
        }

        /// <summary>
        /// Saves arbitrary information about how the notification was handled.
        /// </summary>
        /// <param name="info">The information to save</param>
        /// <remarks>
        /// The handling information saved is primarily used for writing to the log file (if enabled)
        /// </remarks>
        public void SaveHandlingInfo(string info)
        {
            this.handlingInfo.Add(info);
        }

        /// <summary>
        /// Gets the collection of handling information strings associated with the request
        /// </summary>
        /// <value><see cref="List{TValue}"/></value>
        public List<string> HandlingInfo
        {
            get
            {
                return this.handlingInfo;
            }
        }

        /// <summary>
        /// Indicates if the request was forwarded from another machine
        /// </summary>
        /// <returns><c>true</c> if the request was forwarded from another machine;<c>false</c> otherwise</returns>
        public bool WasForwarded()
        {
            if (this.previousReceivedHeaders != null && this.previousReceivedHeaders.Count > 0)
            {
                return true;
            }
            else
                return false;
        }
    }
}
