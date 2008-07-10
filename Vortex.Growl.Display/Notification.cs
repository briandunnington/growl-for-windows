using System;
using System.Collections.Generic;
using System.Text;

namespace Vortex.Growl.DisplayStyle
{
    /// <summary>
    /// Represents all of the information associated with a single notification.
    /// </summary>
    [Serializable]
    public struct Notification
    {
        /// <summary>
        /// The name of the notification (also known as the notification type).
        /// </summary>
        public string Name;

        /// <summary>
        /// The name of the application sending the notification.
        /// </summary>
        public string ApplicationName;

        /// <summary>
        /// The title of the notification.
        /// </summary>
        public string Title;

        /// <summary>
        /// The main text of the notification.
        /// </summary>
        public string Description;

        /// <summary>
        /// The priority of the notification.
        /// </summary>
        /// <remarks>
        ///  2 = Emergency
        ///  1 = High
        ///  0 = Normal
        /// -1 = Moderate
        /// -2 = Very Low
        /// </remarks>
        public int Priority;

        /// <summary>
        /// Indicates if the notification should be sticky or not.
        /// </summary>
        public bool Sticky;
    }
}
