using System;
using System.Collections.Generic;
using System.Text;
using Growl.Destinations;

namespace GrowlExtras.Subscriptions.FeedMonitor
{
    /// <summary>
    /// Manages the creation and operation of the feed subscriber.
    /// </summary>
    /// <remarks>
    /// For each feed url that is configured, GfW will poll on the user-specified interval
    /// to check for new items.
    /// </remarks>
    public class FeedSubscriptionHandler : ISubscriptionHandler
    {
        public static System.Drawing.Image Icon = Properties.Resources.rss;

        #region IDestinationHandler Members

        /// <summary>
        /// The name of the subscription instance
        /// </summary>
        /// <value>string</value>
        public string Name
        {
            get
            {
                return "Feed Monitor";
            }
        }

        /// <summary>
        /// Registers the subscriber with Growl.
        /// </summary>
        /// <returns><see cref="List[Type]"/></returns>
        /// <remarks>
        /// A single handler can register multiple subscriber types if desired.
        /// However, most of the time, you will return a list with just a single
        /// item in it.
        /// </remarks>
        public List<Type> Register()
        {
            List<Type> list = new List<Type>();
            list.Add(typeof(FeedSubscription));
            return list;
        }

        /// <summary>
        /// Gets the list of <see cref="DestinationListItem"/>s to display as choices when
        /// the user chooses 'Add Subscription'.
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
            SubscriptionListItem item = new SubscriptionListItem("Subscribe to an RSS or Atom feed", Icon, this);  // TODO: LOCAL: LOCALIZE:
            List<DestinationListItem> list = new List<DestinationListItem>();
            list.Add(item);
            return list;
        }

        /// <summary>
        /// Gets the settings panel associated with this subscriber.
        /// </summary>
        /// <param name="dbli">The <see cref="DestinationListItem"/> as selected by the user</param>
        /// <returns><see cref="DestinationSettingsPanel"/></returns>
        /// <remarks>
        /// This is called when a user is adding a new subscription.
        /// </remarks>
        public DestinationSettingsPanel GetSettingsPanel(DestinationListItem fdli)
        {
            return new FeedSubscriptionSettings();
        }

        /// <summary>
        /// Gets the settings panel associated with this subscriber.
        /// </summary>
        /// <param name="db">The <see cref="DestinationBase"/> of an exiting subscription</param>
        /// <returns><see cref="DestinationSettingsPanel"/></returns>
        /// <remarks>
        /// This is called when a user is editing an existing subscription.
        /// </remarks>
        public DestinationSettingsPanel GetSettingsPanel(DestinationBase fd)
        {
            return new FeedSubscriptionSettings();
        }

        #endregion
    }
}
