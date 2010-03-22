using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Provides the interface that must be implemented in order to create a new type of
    /// forward destination or subscription.
    /// </summary>
    public interface IDestinationHandler
    {
        /// <summary>
        /// The name of the destination type
        /// </summary>
        /// <value>string</value>
        string Name {get;}

        /// <summary>
        /// Registers the forwarder with Growl.
        /// </summary>
        /// <returns><see cref="List[Type]"/></returns>
        /// <remarks>
        /// A single handler can register multiple forwarder types if desired.
        /// However, most of the time, you will return a list with just a single
        /// item in it.
        /// </remarks>
        List<Type> Register();

        /// <summary>
        /// Gets the settings panel associated with this destination.
        /// </summary>
        /// <param name="db">The <see cref="DestinationBase"/> of an exiting destination</param>
        /// <returns><see cref="DestinationSettingsPanel"/></returns>
        /// <remarks>
        /// This is called when a user is editing an existing destination.
        /// </remarks>
        DestinationSettingsPanel GetSettingsPanel(DestinationBase db);

        /// <summary>
        /// Gets the settings panel associated with this destination.
        /// </summary>
        /// <param name="dbli">The <see cref="DestinationListItem"/> as selected by the user</param>
        /// <returns><see cref="DestinationSettingsPanel"/></returns>
        /// <remarks>
        /// This is called when a user is adding a new destination.
        /// </remarks>
        DestinationSettingsPanel GetSettingsPanel(DestinationListItem dbli);

        /// <summary>
        /// Gets the list of <see cref="DestinationListItem"/>s to display as choices when
        /// the user chooses 'Add Forward' or 'Add Subscription'.
        /// </summary>
        /// <returns><see cref="List[DestinationListItem]"/></returns>
        /// <remarks>
        /// A single handler can return multiple list entries if appropriate (for example, the Bonjour forwarder
        /// detects other computers on the network and returns each as a separate list item).
        /// However, most of the time, you will return a list with just a single
        /// item in it.
        /// </remarks>
        List<DestinationListItem> GetListItems();
    }
}