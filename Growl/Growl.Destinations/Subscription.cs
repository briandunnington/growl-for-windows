using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Provides the base class for all subscriptions.
    /// </summary>
    /// <remarks>
    /// A subscription could listen for notifications from another computer or program running on the same computer, or 
    /// poll for notifications from other computers, processes, websites, or services (ex: RSS).
    /// 
    /// A subscription is responsible for registering any applications that it will handle notifications for. This may
    /// be a single generic application or a subscription may handle notifications for multiple notifications (for example:
    /// the notify.io subscriber handles subscriptions for a wide variety of notify.io 'Sources').
    /// </remarks>
    [Serializable]
    public abstract class Subscription : DestinationBase
    {
        /// <summary>
        /// Handles the <see cref="Subscription.StatusChanged"/> event
        /// </summary>
        public delegate void SubscriptionStatusChangedEventHandler(Subscription subscription);

        /// <summary>
        /// Occurs when the subscription's status changes.
        /// </summary>
        [field: NonSerialized]
        public event SubscriptionStatusChangedEventHandler StatusChanged;

        /// <summary>
        /// Indicates if the subscription is available or not
        /// </summary>
        [NonSerialized]
        private bool available; // by not serializing this property, deserialized Subscriptions will default to being unavailable

        /// <summary>
        /// The GrowlConnector used to send notification from the subscription to GfW
        /// </summary>
        [NonSerialized]
        Growl.Connector.GrowlConnector growl;

        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        /// <param name="description">The friendly name that identifies this instance</param>
        /// <param name="enabled"><c>true</c> if the instance is enabled;<c>false</c> otherwise</param>
        protected Subscription(string description, bool enabled)
            : base(description, enabled)
        {
            Initialize();
        }

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        /// <remarks>
        /// When GfW is closed, information about configured forward destinations and subscriptions is serialized to disk.
        /// When GfW is restarted, that information is deserialized to reconstruct the instances.
        /// Use this method to perform any additional initialization that is required after the object has
        /// been deserialized (such as setting up timers, calling webservices, re-creating non-serialized fields, etc).
        /// </remarks>
        public override void OnDeserialization(object sender)
        {
            Initialize();
            base.OnDeserialization(sender);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            this.growl = new Growl.Connector.GrowlConnector();
            this.growl.EncryptionAlgorithm = Growl.Connector.Cryptography.SymmetricAlgorithmType.PlainText;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DestinationBase"/> is available.
        /// </summary>
        /// <value><c>true</c> if available; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// This mainly applies to hosts that may go offline (such as other GNTP computers on a network)
        /// and want to indicate their state as such.
        /// For hosts that either do not go offline or do not report their status, this
        /// property should just always return <c>true</c>.
        /// </remarks>
        public override bool Available
        {
            get
            {
                return this.available;
            }
            protected set
            {
                this.available = value;
            }
        }

        /// <summary>
        /// Starts the subscription listening/polling for notifications.
        /// </summary>
        public virtual void Subscribe()
        {
        }

        /// <summary>
        /// Stops the subscription from listening/polling for notifications.
        /// </summary>
        public virtual void Kill()
        {
        }

        /// <summary>
        /// Changes the status of the subscription
        /// </summary>
        /// <param name="available"><c>true</c> if the subscription is Available</param>
        /// <param name="additionalInfo">Any additional information about the subscription in this state</param>
        /// <remarks>
        /// This is a handy way to update the information displayed to the user in GfW when the
        /// status of the subscription changes in any way.
        /// </remarks>
        protected void ChangeStatus(bool available, string additionalInfo)
        {
            this.Available = available;
            if(additionalInfo != null) this.AdditionalDisplayInfo = additionalInfo;
            this.OnStatusChanged();
        }

        /// <summary>
        /// Called when the subscription's status changes
        /// </summary>
        protected void OnStatusChanged()
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this);
            }
        }

        /// <summary>
        /// Called when the <see cref="DestinationBase.Enabled"/> property changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            base.OnEnabledChanged(sender, eventArgs);
            UpdateSubscription(this.Enabled, false, null);
        }

        /// <summary>
        /// Starts or stops the subscription as necessary and updates any status information.
        /// </summary>
        /// <param name="subscribe"><c>true</c> if the subscription should be started;<c>false</c> if it should be stopped</param>
        /// <param name="updateDisplay"><c>true</c> to update the AdditionalDisplayInfo;<c>false</c> otherwise</param>
        /// <param name="additionalDisplayInfo">Any additional information to display to the user</param>
        private void UpdateSubscription(bool subscribe, bool updateDisplay, string additionalDisplayInfo)
        {
            if (updateDisplay)
            {
                this.AdditionalDisplayInfo = additionalDisplayInfo;
            }
            if (subscribe && this.Enabled)
                Subscribe();
            else
                Kill();
            this.OnStatusChanged();
        }

        /// <summary>
        /// Registers the specified application with GfW.
        /// </summary>
        /// <param name="application">The <see cref="Growl.Connector.Application"/> to register</param>
        /// <param name="notificationTypes">A list of <see cref="Growl.Connector.NotificationType"/>s to register</param>
        protected void Register(Growl.Connector.Application application, Growl.Connector.NotificationType[] notificationTypes)
        {
            if(this.Enabled)
                this.growl.Register(application, notificationTypes);
        }

        /// <summary>
        /// Triggers a notification
        /// </summary>
        /// <param name="notification">The <see cref="Growl.Connector.Notification"/> to display</param>
        protected void Notify(Growl.Connector.Notification notification)
        {
            if(this.Enabled)
                Notify(notification, null);
        }

        /// <summary>
        /// Triggers a notification
        /// </summary>
        /// <param name="notification">The <see cref="Growl.Connector.Notification"/> to display</param>
        /// <param name="callbackContext">The <see cref="Growl.Connector.CallbackContext"/> of the notification</param>
        protected void Notify(Growl.Connector.Notification notification, Growl.Connector.CallbackContext callbackContext)
        {
            if(this.Enabled)
                this.growl.Notify(notification, callbackContext);
        }
    }
}
