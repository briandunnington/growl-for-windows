using System;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a notification
    /// </summary>
    public class Notification : ExtensibleObject
    {
        /// <summary>
        /// The name of the application sending the notification
        /// </summary>
        private string applicationName;

        /// <summary>
        /// The name (type) of the notification
        /// </summary>
        private string name;

        /// <summary>
        /// A unique id for the notification (sender-specified)
        /// </summary>
        private string id;

        /// <summary>
        /// The notification title
        /// </summary>
        private string title;

        /// <summary>
        /// The notification text
        /// </summary>
        private string text;

        /// <summary>
        /// Indicates if the notification should be sticky or not
        /// </summary>
        private bool sticky = false;

        /// <summary>
        /// The notification priority
        /// </summary>
        private Priority priority = Growl.Connector.Priority.Normal;

        /// <summary>
        /// The notification icon
        /// </summary>
        private Resource icon;

        /// <summary>
        /// The coalescing (grouping) id
        /// </summary>
        private string coalescingID;


        /// <summary>
        /// Creates a instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application sending the notification</param>
        /// <param name="notificationName">The notification name (type)</param>
        /// <param name="id">A unique ID for the notification</param>
        /// <param name="title">The notification title</param>
        /// <param name="text">The notification text</param>
        public Notification(string applicationName, string notificationName, string id, string title, string text) : this(applicationName, notificationName, id, title, text, null, false, Priority.Normal, null)
        {
        }

        /// <summary>
        /// Creates a instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application sending the notification</param>
        /// <param name="notificationName">The notification name (type)</param>
        /// <param name="id">A unique ID for the notification</param>
        /// <param name="title">The notification title</param>
        /// <param name="text">The notification text</param>
        /// <param name="icon">A <see cref="Resource"/> for the icon associated with the notification</param>
        /// <param name="sticky"><c>true</c> to suggest that the notification should be sticky;<c>false</c> otherwise</param>
        /// <param name="priority">The <see cref="Priority"/> of the notification</param>
        /// <param name="coalescingID">The coalescing (grouping) ID (used to replace exisiting notifications)</param>
        public Notification(string applicationName, string notificationName, string id, string title, string text, Resource icon, bool sticky, Priority priority, string coalescingID)
        {
            this.applicationName = applicationName;
            this.name = notificationName;
            this.id = id;
            this.title = title;
            this.text = text;
            this.icon = icon;
            this.sticky = sticky;
            this.priority = priority;
            this.coalescingID = coalescingID;
        }

        /// <summary>
        /// The name of the application sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: SurfWriter
        /// </value>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }

        /// <summary>
        /// The name (type) of the notification.
        /// </summary>
        /// <value>
        /// string - This should match the name of one of the registered <see cref="NotificationType"/>s
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
        /// A unique ID for the notification
        /// </summary>
        /// <value>
        /// string - This value is assigned by the sending application and can be any arbitrary string. This value is optional.
        /// </value>
        public string ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// The title of the notification
        /// </summary>
        /// <value>
        /// string - Ex: Download Complete
        /// </value>
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        /// <summary>
        /// The text of the notification
        /// </summary>
        /// <value>
        /// string - Ex: The file 'filename.txt' had finished downloading
        /// </value>
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        /// <summary>
        /// Indicates if the notification should be sticky or not.
        /// </summary>
        /// <value>
        /// <c>true</c> to suggest that the notification should be sticky;
        /// <c>false</c> otherwise
        /// </value>
        public bool Sticky
        {
            get
            {
                return this.sticky;
            }
            set
            {
                this.sticky = value;
            }
        }

        /// <summary>
        /// The priority of the notification
        /// </summary>
        /// <value>
        /// <see cref="Priority"/>
        /// </value>
        public Priority Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }

        /// <summary>
        /// The icon to associate with this notification
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
        /// An ID used to group notifications.
        /// </summary>
        /// <value>
        /// string - This value is assigned by the sending application and can be any arbitrary string. This value is optional.
        /// </value>
        /// <remarks>
        /// 'Coalescing' in Growl is actually referring to replacement. If a previously sent notification is still on-screen, it
        /// can be updated/replaced by specifying the same CoalescingID.
        /// </remarks>
        public string CoalescingID
        {
            get
            {
                return this.coalescingID;
            }
            set
            {
                this.coalescingID = value;
            }
        }

        /// <summary>
        /// Converts the object to a list of headers
        /// </summary>
        /// <returns><see cref="HeaderCollection"/></returns>
        public HeaderCollection ToHeaders()
        {
            Header hAppName = new Header(Header.APPLICATION_NAME, this.ApplicationName);
            Header hName = new Header(Header.NOTIFICATION_NAME, this.Name);
            Header hID = new Header(Header.NOTIFICATION_ID, this.ID);
            Header hTitle = new Header(Header.NOTIFICATION_TITLE, this.Title);
            Header hText = new Header(Header.NOTIFICATION_TEXT, this.Text);
            Header hSticky = new Header(Header.NOTIFICATION_STICKY, this.Sticky);
            Header hPriority = new Header(Header.NOTIFICATION_PRIORITY, ((int) this.Priority).ToString());
            Header hIcon = new Header(Header.NOTIFICATION_ICON, this.Icon);
            Header hCoalescingID = new Header(Header.NOTIFICATION_COALESCING_ID, this.CoalescingID);

            HeaderCollection headers = new HeaderCollection();
            headers.AddHeader(hAppName);
            headers.AddHeader(hName);
            headers.AddHeader(hID);
            headers.AddHeader(hTitle);
            headers.AddHeader(hText);
            headers.AddHeader(hSticky);
            headers.AddHeader(hPriority);
            headers.AddHeader(hCoalescingID);

            if (this.Icon != null && this.Icon.IsSet)
            {
                headers.AddHeader(hIcon);
                headers.AssociateBinaryData(this.Icon);
            }

            this.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// Creates a new <see cref="Notification"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Notification"/></returns>
        public static Notification FromHeaders(HeaderCollection headers)
        {
            string appName = headers.GetHeaderStringValue(Header.APPLICATION_NAME, true);
            string name = headers.GetHeaderStringValue(Header.NOTIFICATION_NAME, true);
            string id = headers.GetHeaderStringValue(Header.NOTIFICATION_ID, false);
            string title = headers.GetHeaderStringValue(Header.NOTIFICATION_TITLE, true);
            string text = headers.GetHeaderStringValue(Header.NOTIFICATION_TEXT, false);
            if (text == null) text = String.Empty;
            string coalescingID = headers.GetHeaderStringValue(Header.NOTIFICATION_COALESCING_ID, false);
            Resource icon = headers.GetHeaderResourceValue(Header.NOTIFICATION_ICON, false);
            bool sticky = headers.GetHeaderBooleanValue(Header.NOTIFICATION_STICKY, false);
            string p = headers.GetHeaderStringValue(Header.NOTIFICATION_PRIORITY, false);
            Priority priority = Growl.Connector.Priority.Normal;
            if(p != null && Enum.IsDefined(typeof(Priority), p))
            {
                priority = (Priority)Enum.Parse(typeof(Priority), p);
            }

            Notification notification = new Notification(appName, name, id, title, text, icon, sticky, priority, coalescingID);
            SetInhertiedAttributesFromHeaders(notification, headers);
            return notification;
        }
    }
}
