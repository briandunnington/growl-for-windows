using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// Contains the list of error codes that can be returned in error responses
    /// </summary>
    public static class ErrorCode
    {
        /*
        /// <summary>
        /// An unknown error or an error not covered by one of the other error codes
        /// </summary>
        public const int UNKNOWN = 100;
         * */

        /// <summary>
        /// The server timed out waiting for the request to complete
        /// </summary>
        public const int TIMED_OUT = 200;

        /// <summary>
        /// The server was unavailable or the client could not reach the server for any reason
        /// </summary>
        public const int NETWORK_FAILURE = 201;

        /// <summary>
        /// The request contained an unsupported directive, invalid headers or values, or was otherwise malformed
        /// </summary>
        public const int INVALID_REQUEST = 300;

        /// <summary>
        /// The request was not a GNTP request
        /// </summary>
        public const int UNKNOWN_PROTOCOL = 301;

        /// <summary>
        /// The request specified an unknown or unsupported GNTP version
        /// </summary>
        public const int UNKNOWN_PROTOCOL_VERSION = 302;

        /// <summary>
        /// The request was missing required information
        /// </summary>
        public const int REQUIRED_HEADER_MISSING = 303;

        /// <summary>
        /// The request supplied a missing or wrong password/key or was otherwise not authorized
        /// </summary>
        public const int NOT_AUTHORIZED = 400;

        /// <summary>
        /// Application is not registered to send notifications
        /// </summary>
        public const int UNKNOWN_APPLICATION = 401;

        /// <summary>
        /// Notification type is not registered by the application
        /// </summary>
        public const int UNKNOWN_NOTIFICATION = 402;

        /// <summary>
        /// The original request was already processed by this receiver (Normally, a request was forwarded back to a machine that already forwarded it)
        /// </summary>
        public const int ALREADY_PROCESSED = 403;

        /// <summary>
        /// An internal server error occurred while processing the request
        /// </summary>
        public const int INTERNAL_SERVER_ERROR = 500;
    }
}
