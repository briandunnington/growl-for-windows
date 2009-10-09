using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Represents the most basic information associated with a single notification.
    /// </summary>
    [Serializable]
    public class NotificationLite
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

        /// <summary>
        /// The amount of time (in seconds) that the notification should be displayed (for visual displays).
        /// Zero == default value
        /// </summary>
        public int Duration;

        /// <summary>
        /// The name of the machine where the notification originated
        /// </summary>
        public string OriginMachineName;

        /// <summary>
        /// Creates a new <see cref="NotificationLite"/> instance, copying the property values
        /// of the <paramref name="original"/> <see cref="NotificationLite"/>.
        /// </summary>
        /// <param name="original">The <see cref="NotificationLite"/> to clone.</param>
        /// <returns><see cref="NotificationLite"/></returns>
        public static NotificationLite Clone(NotificationLite original)
        {
            NotificationLite n = new NotificationLite();
            n.Name = original.Name;
            n.ApplicationName = original.ApplicationName;
            n.Title = original.Title;
            n.Description = original.Description;
            n.Priority = original.Priority;
            n.Sticky = original.Sticky;
            n.Duration = original.Duration;
            n.OriginMachineName = original.OriginMachineName;
            return n;
        }
    }
}
