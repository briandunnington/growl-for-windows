using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.DisplayStyle
{
    /// <summary>
    /// Represents all of the information associated with a single notification.
    /// </summary>
    [Serializable]
    public class Notification : NotificationLite
    {
        /// <summary>
        /// Contains any custom text attributes associated with the notification
        /// </summary>
        Dictionary<string, string> customTextAttributes;

        /// <summary>
        /// Contains any custom binary attributes associated with the notification
        /// </summary>
        Dictionary<string, Resource> customBinaryAttributes;

        /// <summary>
        /// The globally unique ID of this notification
        /// </summary>
        public string UUID;

        /// <summary>
        /// The notification ID (application-specific/application-provided)
        /// </summary>
        public string NotificationID;

        /// <summary>
        /// The application-specific coalescing id. (See CoalescingGroup property for more info)
        /// </summary>
        public string CoalescingID;

        /// <summary>
        /// The Image associated with the notification
        /// </summary>
        public Resource Image;

        /// <summary>
        /// Gets a collection of custom text attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom text attribute is equivalent to a custom "X-" header
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, string> CustomTextAttributes
        {
            get
            {
                return this.customTextAttributes;
            }
        }

        /// <summary>
        /// Gets a collection of custom binary attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom binary attribute is equivalent to a custom "X-" header with a 
        /// "x-growl-resource://" value, as well as the necessary resource headers
        /// (Identifier, Length, and binary bytes)
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, Resource> CustomBinaryAttributes
        {
            get
            {
                return this.customBinaryAttributes;
            }
        }

        /// <summary>
        /// Adds custom text attributes to the notification. These attributes will be available
        /// to any displays that handle the notification.
        /// </summary>
        /// <param name="attributes"><see cref="Dictionary{TKey, TVal}"/></param>
        public void AddCustomTextAttributes(Dictionary<string, string> attributes)
        {
            if (this.customTextAttributes == null) this.customTextAttributes = new Dictionary<string, string>();
            if (attributes != null)
            {
                foreach (KeyValuePair<string, string> item in attributes)
                {
                    if (this.customTextAttributes.ContainsKey(item.Key))
                        this.customTextAttributes.Remove(item.Key);
                    this.customTextAttributes.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Adds custom binary attributes to the notification. These attributes will be available
        /// to any displays that handle the notification.
        /// </summary>
        /// <param name="attributes"><see cref="Dictionary{TKey, TVal}"/></param>
        public void AddCustomBinaryAttributes(Dictionary<string, Resource> attributes)
        {
            if (this.customBinaryAttributes == null) this.customBinaryAttributes = new Dictionary<string, Resource>();
            if (attributes != null)
            {
                foreach (KeyValuePair<string, Resource> item in attributes)
                {
                    if (this.customBinaryAttributes.ContainsKey(item.Key))
                        this.customBinaryAttributes.Remove(item.Key);
                    this.customBinaryAttributes.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Gets the coalescing group of the notification.
        /// </summary>
        /// <value>The coalescing group.</value>
        /// <remarks>
        /// Notifications from the same application with the same CoalescingID will be in the same
        /// coalescing group. If no CoalescingID is supplied, CoalescingGroup will return an empty string.
        /// Generally, a notification with a matching CoalescingGroup should replace any existing 
        /// notification same CoalescingGroup value.
        /// </remarks>
        public string CoalescingGroup
        {
            get
            {
                if (!String.IsNullOrEmpty(this.CoalescingID))
                {
                    return String.Format("{0}_{1}", this.ApplicationName, this.CoalescingID);
                }
                else
                    return String.Empty;
            }
        }
    }
}
