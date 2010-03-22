using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Provides the base class for all ForwardDestinations and Subscription classes.
    /// </summary>
    [Serializable]
    public abstract class DestinationBase : IDeserializationCallback
    {
        /// <summary>
        /// Occurs when the item is enabled/disabled.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler EnabledChanged;

        /// <summary>
        /// The unique key that identifies this instance
        /// </summary>
        private string key;

        /// <summary>
        /// The friendly name that identifies this instance
        /// </summary>
        private string description;

        /// <summary>
        /// Indicates if the item is enabled or not
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// The type of platform that this instance represents
        /// </summary>
        private DestinationPlatformType platform = DestinationPlatformType.Generic;

        /// <summary>
        /// Additional information displayed to the user about this instance
        /// </summary>
        [NonSerialized]
        private string additionalDisplayInfo;


        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationBase"/> class.
        /// </summary>
        /// <param name="description">The friendly name that identifies this instance</param>
        /// <param name="enabled"><c>true</c> if the instance is enabled;<c>false</c> otherwise</param>
        protected DestinationBase(string description, bool enabled)
        {
            this.description = description;
            this.enabled = enabled;
        }

        /// <summary>
        /// Gets or sets the unique identifier for this instance.
        /// </summary>
        /// <value>string</value>
        public virtual string Key
        {
            get
            {
                if (String.IsNullOrEmpty(this.key)) this.key = System.Guid.NewGuid().ToString();
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// Gets or sets the friendly name that identifies this instance
        /// </summary>
        /// <value>string</value>
        public virtual string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DestinationBase"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
                this.OnEnabledChanged(this, EventArgs.Empty);
            }
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
        public abstract bool Available { get; protected set;}

        /// <summary>
        /// Gets a value indicating whether this <see cref="DestinationBase"/> is both
        /// enabled and available.
        /// </summary>
        /// <value><c>true</c> if the instance is both enabled and available; otherwise, <c>false</c>.</value>
        public virtual bool EnabledAndAvailable
        {
            get
            {
                return (this.Enabled && this.Available);
            }
        }

        /// <summary>
        /// Gets or sets the type of platform of this instance.
        /// </summary>
        /// <value><see cref="DestinationPlatformType"/></value>
        /// <remarks>
        /// The 'platform' can represent the type of OS (Windows, Mac, etc), device (iPhone, Android phone, etc),
        /// or service (RSS, notify.io, etc) - anything that is appropriate to identify the type to the user.
        /// </remarks>
        public DestinationPlatformType Platform
        {
            get
            {
                return this.platform;
            }
            protected set
            {
                this.platform = value;
            }
        }

        /// <summary>
        /// Gets the icon used to represent this type of host.
        /// </summary>
        /// <returns><see cref="System.Drawing.Image"/></returns>
        /// <remarks>
        /// By default, this will return the icon of the <see cref="DestinationBase.Platform"/>, but
        /// can be overridden to provide a custom icon.
        /// </remarks>
        public virtual System.Drawing.Image GetIcon()
        {
            return this.Platform.GetIcon();
        }

        /// <summary>
        /// Gets the text used to identify this instance to the user
        /// </summary>
        /// <value>string</value>
        /// <remarks>
        /// This is shown as the first line of the item in the list view in GfW.
        /// </remarks>
        public virtual string Display
        {
            get
            {
                return this.Description;
            }
        }

        /// <summary>
        /// Gets the text used to identify the address/location of this instance to the user.
        /// </summary>
        /// <value>string</value>
        /// <remarks>
        /// This is shown as the second line of the item in the list view in GfW.
        /// </remarks>
        /// <remarks>
        /// When implemented in a derived class, this should return the effective address of the 
        /// instance, such as a url, ip:port, network name, or other identifying location.
        /// </remarks>
        public abstract string AddressDisplay { get; }

        /// <summary>
        /// Gets any additional information displayed to the user about this instance
        /// </summary>
        /// <value>string</value>
        /// <remarks>
        /// This is shown as the third (optional) line of the item in the list view in GfW.
        /// </remarks>
        public virtual string AdditionalDisplayInfo
        {
            get
            {
                return this.additionalDisplayInfo;
            }
            protected set
            {
                this.additionalDisplayInfo = value;
            }
        }

        /// <summary>
        /// Updates the display info when enabled/disabled.
        /// </summary>
        private void UpdateDisplayInfo()
        {
            if (!this.Enabled)
            {
                this.AdditionalDisplayInfo = "not enabled";
            }
            else
            {
                this.AdditionalDisplayInfo = null;
            }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns><see cref="DestinationBase"/></returns>
        public abstract DestinationBase Clone();

        /// <summary>
        /// Called when the <see cref="DestinationBase.Enabled"/> property changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnEnabledChanged(object sender, EventArgs eventArgs)
        {
            UpdateDisplayInfo();
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(sender, eventArgs);
            }
        }

        #region IDeserializationCallback Members

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
        public virtual void OnDeserialization(object sender)
        {
            UpdateDisplayInfo();
        }

        #endregion
    }
}

