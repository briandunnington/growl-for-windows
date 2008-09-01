using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Growl.Framework
{
    /// <summary>
    /// Indicates the relative priority of a notification.
    /// </summary>
    /// <remarks>
    /// The priority of a notification can be used to change the way the notification is handled
    /// and presented to the end user. For instance, higher priority notifications might be displayed
    /// with a red color or exclamation icon. However, each display is responsible for
    /// handling changes related to priority and may not make any distinction between different priority
    /// levels. Further, although each notification can request its own priority, the end user may elect 
    /// to override this priority setting, so the notification's requested priority is not guaranteed.
    /// </remarks>
    public enum Priority
    {
        /// <summary>
        /// Very low
        /// </summary>
        [Description("Very Low")]
        VeryLow = -2,

        /// <summary>
        /// Moderate
        /// </summary>
        Moderate = -1,

        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// High
        /// </summary>
        High = 1,

        /// <summary>
        /// Emergency
        /// </summary>
        Emergency = 2
    }
}
