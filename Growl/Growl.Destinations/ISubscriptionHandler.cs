using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Destinations
{
    /// <summary>
    /// Provides the interface that must be implemented in order to create a new type of
    /// subscription.
    /// </summary>
    public interface ISubscriptionHandler : IDestinationHandler
    {
    }
}
