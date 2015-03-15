using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Growl.Destinations;

namespace Webhook_Plugin
{
    /// <summary>
    /// Forwards notifications to any url using an HTTP POST request.
    /// </summary>
    [Serializable]
    public class WebhookDestination : ForwardDestination
    {
        /// <summary>
        /// The url of the webhook
        /// </summary>
        private string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookDestination"/> class.
        /// </summary>
        /// <param name="url">The URL of the webhook</param>
        public WebhookDestination(string name, string url)
            : base(name, true)
        {
            this.url = url;
        }

        /// <summary>
        /// Gets the address display.
        /// </summary>
        /// <value>The address display.</value>
        /// <remarks>
        /// This is shown in GfW as the second line of the item in the 'Forwards' list view.
        /// </remarks>
        public override string AddressDisplay
        {
            get { return this.url; }
        }

        /// <summary>
        /// Gets or sets the URL of the webhook
        /// </summary>
        /// <value>string</value>
        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="WebhookDestination"/> is available.
        /// </summary>
        /// <value>Always returns <c>true</c>.</value>
        /// <remarks>
        /// This value is essentially read-only. Setting the value will have no effect.
        /// </remarks>
        public override bool Available
        {
            get
            {
                return true;
            }
            protected set
            {
                //throw new Exception("The method or operation is not implemented.");
            }
        }

        /// <summary>
        /// Called when a notification is received by GfW.
        /// </summary>
        /// <param name="notification">The notification information</param>
        /// <param name="callbackContext">The callback context.</param>
        /// <param name="requestInfo">The request info.</param>
        /// <param name="isIdle"><c>true</c> if the user is currently idle;<c>false</c> otherwise</param>
        /// <param name="callbackFunction">The function GfW will run if this notification is responded to on the forwarded computer</param>
        /// <remarks>
        /// Unless your forwarder is going to handle socket-style callbacks from the remote computer, you should ignore
        /// the <paramref name="callbackFunction"/> parameter.
        /// </remarks>
        public override void ForwardNotification(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext, Growl.Connector.RequestInfo requestInfo, bool isIdle, ForwardDestination.ForwardedNotificationCallbackHandler callbackFunction)
        {
            try
            {
                QuerystringBuilder qsb = new QuerystringBuilder();
                qsb.Add("app", notification.ApplicationName);
                qsb.Add("id", notification.ID);
                qsb.Add("type", notification.Name);
                qsb.Add("title", notification.Title);
                qsb.Add("text", notification.Text);
                qsb.Add("sticky", notification.Sticky);
                qsb.Add("priority", (int)notification.Priority);
                qsb.Add("coalescingid", notification.CoalescingID);
                if (notification.CustomTextAttributes != null)
                {
                    foreach (KeyValuePair<string, string> item in notification.CustomTextAttributes)
                    {
                        qsb.Add(item.Key, item.Value);
                    }
                }

                string data = qsb.ToPostData();
                Growl.CoreLibrary.WebClientEx wc = new Growl.CoreLibrary.WebClientEx();
                using (wc)
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "Growl for Windows Webhook Plugin/1.0");
                    string result = wc.UploadString(this.url, data);
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                // this is an example of writing to the main GfW debug log:
                Growl.CoreLibrary.DebugInfo.WriteLine(String.Format("Webhook forwarding failed: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Called when an application registration is received by GfW.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="notificationTypes">The notification types.</param>
        /// <param name="requestInfo">The request info.</param>
        /// <param name="isIdle"><c>true</c> if the user is currently idle;<c>false</c> otherwise</param>
        /// <remarks>
        /// Many types of forwarders can just ignore this event.
        /// </remarks>
        public override void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, bool isIdle)
        {
            // do nothing
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns><see cref="WebhookDestination"/></returns>
        public override DestinationBase Clone()
        {
            return new WebhookDestination(this.Description, this.url);
        }

        /// <summary>
        /// Gets the icon that represents this type of forwarder.
        /// </summary>
        /// <returns><see cref="System.Drawing.Image"/></returns>
        public override System.Drawing.Image GetIcon()
        {
            return WebhookForwardHandler.GetIcon();
        }
    }
}
