using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Growl.Daemon
{
    /// <summary>
    /// Represents information needed by the receiver in order to perform a callback
    /// </summary>
    public class CallbackInfo
    {
        /// <summary>
        /// The unique notification ID
        /// </summary>
        private string notificationID;

        // TODO: see below
        private RequestInfo requestInfo;

        private bool alreadyResponded;

        private Dictionary<string, string> additionalInfo;

        /// <summary>
        /// The callback context from the request
        /// </summary>
        public CallbackContext Context;

        /// <summary>
        /// The MessageHandler that will peform the callback write
        /// </summary>
        public MessageHandler MessageHandler;

        // TODO: figure out if this really should be in here or not
        public RequestInfo RequestInfo;

        /// <summary>
        /// Gets or sets the unique notification ID provided in the request
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string NotificationID
        {
            get
            {
                return this.notificationID;
            }
            internal set
            {
                this.notificationID = value;
            }
        }

        /// <summary>
        /// Indicates if the server should keep the connection open to perform the callback
        /// </summary>
        /// <returns>
        /// <c>true</c> to keep the connection open and perform the callback via the connection,
        /// <c>false</c> if the callback is url-based and will be performed out-of-band
        /// </returns>
        public bool ShouldKeepConnectionOpen()
        {
            if (this.Context != null && this.Context.ShouldKeepConnectionOpen())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the url-formatted callback data that is to be sent for url callbacks.
        /// </summary>
        /// <param name="result">The <see cref="CallbackResult"/> indicating the user action</param>
        /// <returns>string - querystring/post format</returns>
        public string GetUrlCallbackData(CallbackResult result)
        {
            string data = null;
            if (this.Context != null)
            {
                UrlCallbackTarget target = this.Context.GetUrlCallbackTarget();
                if (target != null)
                {
                    data = String.Format("notification-id={0}&notification-callback-result={1}&notification-callback-context={2}&notification-callback-context-type={3}", HttpUtility.UrlEncode(this.notificationID), HttpUtility.UrlEncode(result.ToString()), HttpUtility.UrlEncode(this.Context.Data), HttpUtility.UrlEncode(this.Context.Type));
                }
            }
            return data;
        }

        /// <summary>
        /// Saves all extended information that should be returned with the callback response.
        /// </summary>
        /// <param name="additionalInfo">A <see cref="Dictionary{TKey, TValue}"/> containing all of the additional information key/value pairs</param>
        public void SetAdditionalInfo(Dictionary<string, string> additionalInfo)
        {
            this.additionalInfo = additionalInfo;
        }

        /// <summary>
        /// Gets a list of all extended-information key/value pairs that should be returned with the callback response.
        /// </summary>
        internal Dictionary<string, string> AdditionalInfo
        {
            get
            {
                return this.additionalInfo;
            }
        }

        /// <summary>
        /// Indicates if the request that spawned this callback has already been responded to.
        /// </summary>
        /// <remarks>
        /// When a notification is forwarded to another computer, the notification may be clicked/handled on both computers.
        /// Only the first response action is returned and all subsequent actions are ignored.
        /// </remarks>
        public bool AlreadyResponded
        {
            get
            {
                return this.alreadyResponded;
            }
            internal set
            {
                this.alreadyResponded = value;
            }
        }
    }
}
