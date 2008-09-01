using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Growl.Framework
{
    /// <summary>
    /// Represents a type of notification that an application may send
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class NotificationType
    {
        /// <summary>
        /// The common name of this type of notification
        /// </summary>
        protected string name = "Generic Application Notification";
        /// <summary>
        /// Indicates if this type of notification should be enabled or disabled by default
        /// </summary>
        protected bool enabled = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>The default constructor is parameterless to enable COM interoperability.</remarks>
        public NotificationType()
        {
        }

        /// <summary>
        /// Creates a new <see cref="NotificationType"/>
        /// </summary>
        /// <param name="name">The common name of this type of notification</param>
        /// <param name="enabled"><c>true</c> if this type of notification should be enabled by default; <c>false</c> if this type of notification should be disabled by default</param>
        public NotificationType(string name, bool enabled)
        {
            this.name = name;
            this.enabled = enabled;
        }

        /// <summary>
        /// The common name of this type of notification
        /// </summary>
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
        /// Indicates if this type of notification should be enabled or disabled by default
        /// </summary>
        /// <value><c>true</c> if this type of notification should be enabled by default; <c>false</c> if this type of notification should be disabled by default</value>
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
        /// Returns the associated <see cref="NotificationType"/> given the type's name
        /// </summary>
        /// <param name="name">The common name of the type of notification</param>
        /// <returns><see cref="NotificationType"/></returns>
        public static NotificationType GetByName(string name)
        {
            // TODO:
            NotificationType nt = new NotificationType();
            nt.Name = name;
            nt.Enabled = false;
            return nt;
        }
    }
}
