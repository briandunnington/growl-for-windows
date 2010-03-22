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
        /// Handles the <see cref="CallbackInfo.ForwardedNotificationCallback"/> event
        /// </summary>
        public delegate void ForwardedNotificationCallbackHandler(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData, CallbackInfo callbackInfo);

        /// <summary>
        /// Occurs when a forwarded notification triggers a callback from the forwarded destination
        /// </summary>
        [field: NonSerialized]
        public event ForwardedNotificationCallbackHandler ForwardedNotificationCallback;

        /// <summary>
        /// The callback context from the request
        /// </summary>
        private CallbackContext context;

        /// <summary>
        /// The MessageHandler that will peform the callback write
        /// </summary>
        private MessageHandler messageHandler;

        /// <summary>
        /// The unique notification ID
        /// </summary>
        private string notificationID;

        /// <summary>
        /// Indicates if a callback associated with this notification has already been triggered
        /// </summary>
        private bool alreadyResponded;

        /// <summary>
        /// Any additional information to return in the callback
        /// </summary>
        private Dictionary<string, string> additionalInfo;

        // TODO: see below
        private RequestInfo requestInfo;


        /// <summary>
        /// The callback context from the request
        /// </summary>
        public CallbackContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        /// <summary>
        /// The MessageHandler that will peform the callback write
        /// </summary>
        public MessageHandler MessageHandler
        {
            get
            {
                return this.messageHandler;
            }
            set
            {
                this.messageHandler = value;
            }
        }

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

        // TODO: figure out if this really should be in here or not
        public RequestInfo RequestInfo;

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
        /// Saves all extended information that should be returned with the callback response.
        /// </summary>
        /// <param name="additionalInfo">A <see cref="Dictionary{TKey, TValue}"/> containing all of the additional information key/value pairs</param>
        public void SetAdditionalInfo(Dictionary<string, string> additionalInfo)
        {
            this.additionalInfo = additionalInfo;
        }



        /*
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
         * */

        /// <summary>
        /// Handles the callback from a forwarder.
        /// </summary>
        /// <param name="response">The <see cref="Response"/> from the forwarder</param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        public void HandleCallbackFromForwarder(Response response, CallbackData callbackData)
        {
            this.RequestInfo.SaveHandlingInfo(String.Format("Was responded to on {0} - Action: {1}", response.MachineName, callbackData.Result));

            if (this.ForwardedNotificationCallback != null)
            {
                this.ForwardedNotificationCallback(response, callbackData, this);
            }
        }
    }
}
