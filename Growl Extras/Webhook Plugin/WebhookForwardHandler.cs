using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace Webhook_Plugin
{
    /// <summary>
    /// Manages the creation and operation of the webhook forwarder.
    /// </summary>
    /// <remarks>
    /// The notification data is POSTed using the following values:
    ///     app=ApplicationName
    ///     id=NotificationID   (may be empty)
    ///     type=NotificationType
    ///     title=NotificationTitle
    ///     text=NotificationText
    ///     sticky=Sticky       (true or false)
    ///     priority=Priority   (2,1,0,-1,-2)
    ///     coalescingid=CoalescingID   (may be empty)
    /// 
    /// Additionally, any custom text attributes of the notification are also POSTed
    /// using the following format:
    ///     customAttributeHeaderName=customAttributeValue
    /// 
    /// For a sample url to use as a destination, use: http://www.growlforwindows.com/gfw/examples/webhook/sample.ashx
    /// </remarks>
    public class WebhookForwardHandler : IForwardDestinationHandler
    {
        #region IDestinationHandler Members

        /// <summary>
        /// The name of the webhook instance
        /// </summary>
        /// <value>string</value>
        public string Name
        {
            get { return "Webhook"; }
        }

        /// <summary>
        /// Registers the forwarder with Growl.
        /// </summary>
        /// <returns><see cref="List[Type]"/></returns>
        /// <remarks>
        /// A single handler can register multiple forwarder types if desired.
        /// However, most of the time, you will return a list with just a single
        /// item in it.
        /// </remarks>
        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(WebhookDestination));
            return list;
        }

        /// <summary>
        /// Gets the list of <see cref="DestinationListItem"/>s to display as choices when
        /// the user chooses 'Add Forward'.
        /// </summary>
        /// <returns><see cref="List[DestinationListItem]"/></returns>
        /// <remarks>
        /// A single handler can return multiple list entries if appropriate (for example, the Bonjour forwarder
        /// detects other computers on the network and returns each as a separate list item).
        /// However, most of the time, you will return a list with just a single
        /// item in it.
        /// </remarks>
        public List<DestinationListItem> GetListItems()
        {
            ForwardDestinationListItem item = new ForwardDestinationListItem("Call a webhook (HTTP POST)", GetIcon(), this);
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        /// <summary>
        /// Gets the settings panel associated with this forwarder.
        /// </summary>
        /// <param name="dbli">The <see cref="DestinationListItem"/> as selected by the user</param>
        /// <returns><see cref="DestinationSettingsPanel"/></returns>
        /// <remarks>
        /// This is called when a user is adding a new forwarding destination.
        /// </remarks>
        public DestinationSettingsPanel GetSettingsPanel(DestinationListItem dbli)
        {
            return new WebhookInputs();
        }

        /// <summary>
        /// Gets the settings panel associated with this forwarder.
        /// </summary>
        /// <param name="db">The <see cref="DestinationBase"/> of an exiting forwarder</param>
        /// <returns><see cref="DestinationSettingsPanel"/></returns>
        /// <remarks>
        /// This is called when a user is editing an existing forwarder.
        /// </remarks>
        public DestinationSettingsPanel GetSettingsPanel(DestinationBase db)
        {
            return new WebhookInputs();
        }

        #endregion

        /// <summary>
        /// Gets the icon associated with this forwarder.
        /// </summary>
        /// <returns><see cref="System.Drawing.Image"/></returns>
        internal static System.Drawing.Image GetIcon()
        {
            return new System.Drawing.Bitmap(Properties.Resources.internet);
        }
    }
}
