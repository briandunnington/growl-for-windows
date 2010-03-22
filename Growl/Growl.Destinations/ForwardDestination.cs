using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;

namespace Growl.Destinations
{
    /// <summary>
    /// Provides the base class for all forward destinations.
    /// </summary>
    /// <remarks>
    /// A forward destination could be another computer running GfW or other notification daemon, a website or service,
    /// another application on the same machine, or just custom actions to be performed for each notification (such as 
    /// additional logging).
    /// </remarks>
    [Serializable]
    public abstract class ForwardDestination : DestinationBase
    {
        /// <summary>
        /// Handles the <see cref="ForwardDestination.ForwardedNotificationCallback"/> event.
        /// </summary>
        public delegate void ForwardedNotificationCallbackHandler(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData);

        /// <summary>
        /// Occurs when a notification that has been forwarded triggers a callback from the forwarded destination.
        /// </summary>
        [field: NonSerialized]
        public event ForwardedNotificationCallbackHandler ForwardedNotificationCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardDestination"/> class.
        /// </summary>
        /// <param name="description">The friendly name that identifies this instance</param>
        /// <param name="enabled"><c>true</c> if the instance is enabled;<c>false</c> otherwise</param>
        protected ForwardDestination(string description, bool enabled)
            : base(description, enabled)
        {
        }

        /// <summary>
        /// Called when an application registration is received by GfW.
        /// </summary>
        /// <param name="application">The <see cref="Growl.Connector.Application"/> that is registering</param>
        /// <param name="notificationTypes">A list of <see cref="Growl.Connector.NotificationType"/>s being registered</param>
        /// <param name="requestInfo">The <see cref="Growl.Connector.RequestInfo"/> associated with the registration request</param>
        /// <param name="isIdle"><c>true</c> if the user is currently idle;<c>false</c> otherwise</param>
        public abstract void ForwardRegistration(Growl.Connector.Application application, List<Growl.Connector.NotificationType> notificationTypes, Growl.Connector.RequestInfo requestInfo, bool isIdle);

        /// <summary>
        /// Called when a notification is received by GfW.
        /// </summary>
        /// <param name="notification">The notification information</param>
        /// <param name="callbackContext">The callback context.</param>
        /// <param name="requestInfo">The <see cref="Growl.Connector.RequestInfo"/> associated with the notification request</param>
        /// <param name="isIdle"><c>true</c> if the user is currently idle;<c>false</c> otherwise</param>
        /// <param name="callbackFunction">The function GfW will run if this notification is responded to on the forwarded computer</param>
        /// <remarks>
        /// Unless your forwarder is going to handle socket-style callbacks from the remote computer, you should ignore
        /// the <paramref name="callbackFunction"/> parameter.
        /// </remarks>
        public abstract void ForwardNotification(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext, Growl.Connector.RequestInfo requestInfo, bool isIdle, ForwardedNotificationCallbackHandler callbackFunction);

        /// <summary>
        /// Called when a notification that has been forwarded triggers a callback from the forwarded destination.
        /// </summary>
        /// <param name="response">The <see cref="Response"/> from the forwarded destination</param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        protected void OnForwardNotificationCallback(Response response, CallbackData callbackData)
        {
            if (this.ForwardedNotificationCallback != null)
            {
                this.ForwardedNotificationCallback(response, callbackData);
            }
        }
    }
}

