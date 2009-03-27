using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Contains a list of the available types of callback actions that can be reported
    /// </summary>
    public enum CallbackResult
    {
        /// <summary>
        /// The user clicked on the notification
        /// </summary>
        CLICK,

        /// <summary>
        /// The user closed the notification explicitly
        /// </summary>
        CLOSE,

        /// <summary>
        /// The notification timed out without user intervention
        /// </summary>
        TIMEDOUT
    }
}
