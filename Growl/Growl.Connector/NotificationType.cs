using System;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a type of notification that an application may send
    /// </summary>
    public class NotificationType : ExtensibleObject
    {
        /// <summary>
        /// The name of the notification type
        /// </summary>
        private string name = "Undefined Notification";

        /// <summary>
        /// The display name of the notification type
        /// </summary>
        private string displayName = null;

        /// <summary>
        /// The default icon for notifications of this type
        /// </summary>
        private Resource icon;

        /// <summary>
        /// Indicates if this type of notification should be enabled or disabled by default
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// Creates a instance of the <see cref="NotificationType"/> class.
        /// </summary>
        /// <param name="name">The name of this type of notification</param>
        public NotificationType(string name)
            : this(name, null, null, true)
        {
        }

        /// <summary>
        /// Creates a instance of the <see cref="NotificationType"/> class.
        /// </summary>
        /// <param name="name">The name of this type of notification</param>
        /// <param name="displayName">The display name of this type of notification</param>
        public NotificationType(string name, string displayName) : this(name, displayName, null, true)
        {
        }

        /// <summary>
        /// Creates a instance of the <see cref="NotificationType"/> class.
        /// </summary>
        /// <param name="name">The name of this type of notification</param>
        /// <param name="displayName">The display name of this type of notification</param>
        /// <param name="icon">The default icon for notifications of this type</param>
        /// <param name="enabled"><c>true</c> if this type of notification should be enabled by default; <c>false</c> if this type of notification should be disabled by default</param>
        public NotificationType(string name, string displayName, Resource icon, bool enabled)
        {
            this.name = name;
            this.displayName = displayName;
            this.icon = icon;
            this.enabled = enabled;
        }

        /// <summary>
        /// The name of this type of notification
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// The display name of this type of notification
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string DisplayName
        {
            get
            {
                return (this.displayName == null ? this.name : this.displayName);
            }
            set
            {
                this.displayName = value;
            }
        }

        /// <summary>
        /// The default icon for notifications of this type
        /// </summary>
        /// <value>
        /// <see cref="Resource"/>
        /// </value>
        public Resource Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        /// <summary>
        /// Indicates if this type of notification should be enabled or disabled by default
        /// </summary>
        /// <value>
        /// <c>true</c> if this type of notification should be enabled by default;
        /// <c>false</c> if this type of notification should be disabled by default
        /// </value>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        /// <summary>
        /// Converts the object to a list of headers
        /// </summary>
        /// <returns><see cref="HeaderCollection"/></returns>
        public HeaderCollection ToHeaders()
        {
            Header hName = new Header(Header.NOTIFICATION_NAME, this.Name);
            Header hDisplayName = new Header(Header.NOTIFICATION_DISPLAY_NAME, this.DisplayName);
            Header hIcon = new Header(Header.NOTIFICATION_ICON, this.Icon);
            Header hEnabled = new Header(Header.NOTIFICATION_ENABLED, this.Enabled.ToString());

            HeaderCollection headers = new HeaderCollection();
            headers.AddHeader(hName);
            headers.AddHeader(hEnabled);

            if(this.displayName != null)
                headers.AddHeader(hDisplayName);

            if (this.Icon != null && this.Icon.IsSet)
            {
                headers.AddHeader(hIcon);
                headers.AssociateBinaryData(this.Icon);
            }

            this.AddCustomAttributesToHeaders(headers); // NOTE: dont call AddInheritedAttributesToHeaders because we want to ignore the common attributes
            return headers;
        }

        /// <summary>
        /// Creates a new <see cref="NotificationType"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the response</param>
        /// <returns><see cref="NotificationType"/></returns>
        public static NotificationType FromHeaders(HeaderCollection headers)
        {
            string name = headers.GetHeaderStringValue(Header.NOTIFICATION_NAME, true);
            string displayName = headers.GetHeaderStringValue(Header.NOTIFICATION_DISPLAY_NAME, false);
            Resource icon = headers.GetHeaderResourceValue(Header.NOTIFICATION_ICON, false);
            bool enabled = headers.GetHeaderBooleanValue(Header.NOTIFICATION_ENABLED, false);

            NotificationType nt = new NotificationType(name, displayName, icon, enabled);
            SetCustomAttributesFromHeaders(nt, headers);    // NOTE: dont call SetInheritedAttributesFromHeaders because we want to ignore the common attributes
            return nt;
        }
    }
}
