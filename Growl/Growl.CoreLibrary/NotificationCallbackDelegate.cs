using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// This class supports internal Growl functionality and is not intended for public use.
    /// 
    /// Represents a delegate proxy that can communicate across AppDomain boundaries.
    /// </summary>
    public abstract class NotificationCallbackDelegate : MarshalByRefObject
    {
        /// <summary>
        /// Raises the <c>NotificationCallback</c> event
        /// </summary>
        /// <param name="args"><see cref="NotificationCallbackEventArgs"/> containing data about the event</param>
        public void OnNotificationCallback(NotificationCallbackEventArgs args)
        {
            this.InternalOnNotificationCallback(args);
        }

        /// <summary>
        /// Raises the <c>NotificationCallback</c> event
        /// </summary>
        /// <param name="args"><see cref="NotificationCallbackEventArgs"/> containing data about the event</param>
        /// <remarks>
        /// This is where derived classes in other AppDomains do their actual work.
        /// </remarks>
        protected abstract void InternalOnNotificationCallback(NotificationCallbackEventArgs args);

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"></see> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"></see> property.
        /// </returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
