using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// Contains the list of error description strings
    /// </summary>
    public static class ErrorDescription
    {
        /// <summary>
        /// The server timed out waiting for the remainder of the request
        /// </summary>
        public const string TIMED_OUT = "The server timed out waiting for the remainder of the request";
        /// <summary>
        /// Unrecognized request
        /// </summary>
        public const string UNRECOGNIZED_REQUEST = "Unrecognized request";
        /// <summary>
        /// Unsupported directive
        /// </summary>
        public const string UNSUPPORTED_DIRECTIVE = "Unsupported directive";
        /// <summary>
        /// Unsupported version
        /// </summary>
        public const string UNSUPPORTED_VERSION = "Unsupported version";
        /// <summary>
        /// No notifications registered
        /// </summary>
        public const string NO_NOTIFICATIONS_REGISTERED = "No notifications registered";
        /// <summary>
        /// Invalid resource length
        /// </summary>
        public const string INVALID_RESOURCE_LENGTH = "Invalid resource length";
        /// <summary>
        /// Malformed request
        /// </summary>
        public const string MALFORMED_REQUEST = "Malformed request";
        /// <summary>
        /// Unrecognized resource header
        /// </summary>
        public const string UNRECOGNIZED_RESOURCE_HEADER = "Unrecognized resource header";
        /// <summary>
        /// Internal server error
        /// </summary>
        public const string INTERNAL_SERVER_ERROR = "Internal server error";
        /// <summary>
        /// Invalid key hash
        /// </summary>
        public const string INVALID_KEY = "Invalid key hash";
        /// <summary>
        /// Required header missing
        /// </summary>
        public const string REQUIRED_HEADER_MISSING = "Required header missing";
        /// <summary>
        /// Unsupported password hash algorithm
        /// </summary>
        public const string UNSUPPORTED_HASH_ALGORITHM = "Unsupported password hash algorithm";
        /// <summary>
        /// Unsupported encryption algorithm
        /// </summary>
        public const string UNSUPPORTED_ENCRYPTION_ALGORITHM = "Unsupported encryption algorithm";
        /// <summary>
        /// Application not registered
        /// </summary>
        public const string APPLICATION_NOT_REGISTERED = "Application not registered";
        /// <summary>
        /// Notification type not registered
        /// </summary>
        public const string NOTIFICATION_TYPE_NOT_REGISTERED = "Notification type not registered";
        /// <summary>
        /// Flash-based connections are not allowed
        /// </summary>
        public const string FLASH_CONNECTIONS_NOT_ALLOWED = "Flash-based connections are not allowed";
        /// <summary>
        /// This server does not allow subscriptions
        /// </summary>
        public const string SUBSCRIPTIONS_NOT_ALLOWED = "This server does not allow subscriptions";
        /// <summary>
        /// The request was already handled by this machine. (Normally, this means the message was forwarded back to a machine that had already forwarded it.)
        /// </summary>
        public const string ALREADY_PROCESSED = "The request was already handled by this machine. (Normally, this means the message was forwarded back to a machine that had already forwarded it.)";

        // ------------------------------------------------------------------------------------------
        // The error descriptions below here are for pseudo errors; they are not derived from a 
        // GNTP response but instead are used to represent lower-level errors (such as network
        // connectivity, unreachable server, etc).
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// The destination was not reachable (invalid address or port, network connectivity, firewall, etc)
        /// </summary>
        public const string CONNECTION_FAILURE = "The destination server was not reachable";
        /// <summary>
        /// An unexpected error occurred while writing the request
        /// </summary>
        public const string WRITE_FAILURE = "The request failed to be sent successfully due to a network problem.";
        /// <summary>
        /// An unexpected error occurred while reading the response
        /// </summary>
        public const string READ_FAILURE = "The response failed to be read successfully due to a network problem.";
    }
}
