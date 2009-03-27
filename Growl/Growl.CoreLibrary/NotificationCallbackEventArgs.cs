using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Provides information about notification callback events
    /// </summary>
    [Serializable]
    public class NotificationCallbackEventArgs : EventArgs
    {
        /// <summary>
        /// The UUID of the notification making the callback
        /// </summary>
        private string notificationUUID;

        /// <summary>
        /// The callback result
        /// </summary>
        private CallbackResult result;

        /// <summary>
        /// Additional information to be returned in the callback response (as custom headers)
        /// </summary>
        Dictionary<string, string> customInfo = new Dictionary<string, string>();

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="notificationUUID">The UUID of the notification making the callback</param>
        /// <param name="result">The callback result</param>
        public NotificationCallbackEventArgs(string notificationUUID, CallbackResult result)
        {
            this.notificationUUID = notificationUUID;
            this.result = result;
        }

        /// <summary>
        /// The notification UUID of the notification making the callback
        /// </summary>
        public string NotificationUUID
        {
            get
            {
                return this.notificationUUID;
            }
        }

        /// <summary>
        /// The callback result
        /// </summary>
        /// <value><see cref="CallbackResult"/></value>
        public CallbackResult Result
        {
            get
            {
                return this.result;
            }
        }

        /// <summary>
        /// Returns the list of additional information to be returned in the callback response
        /// (as custom headers)
        /// </summary>
        /// <value><see cref="Dictionary{TKey, TValue}"/></value>
        public Dictionary<string, string> CustomInfo
        {
            get
            {
                return this.customInfo;
            }
        }
    }
}
