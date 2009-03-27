using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// Contains a list of the valid types of responses
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// Response is good, returns immediately
        /// </summary>
        OK,

        /// <summary>
        /// Response is good, returns when a callback action occurs
        /// </summary>
        CALLBACK,

        /// <summary>
        /// Response indicates an error
        /// </summary>
        ERROR
    }
}
