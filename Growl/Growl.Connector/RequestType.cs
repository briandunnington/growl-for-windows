using System;

namespace Growl.Connector
{
    /// <summary>
    /// Contains a list of the valid types of requests
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// Register an application and its notification types
        /// </summary>
        REGISTER,

        /// <summary>
        /// Send a notification
        /// </summary>
        NOTIFY,

        /// <summary>
        /// Subscribes a client to Growl notifications
        /// </summary>
        SUBSCRIBE
    }
}
