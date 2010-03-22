using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Growl.Destinations
{
    /// <summary>
    /// Represents the platform type of a destination.
    /// </summary>
    /// <remarks>
    /// The 'platform' can represent the type of OS (Windows, Mac, etc), device (iPhone, Android phone, etc),
    /// or service (RSS, notify.io, etc) - anything that is appropriate to identify the type to the user.
    /// </remarks>
    [Serializable]
    public class DestinationPlatformType
    {
        /// <summary>
        /// A generic platform type used when no more specific type is defined.
        /// </summary>
        public static DestinationPlatformType Generic = new DestinationPlatformType(PLATFORM_GENERIC);

        /// <summary>
        /// String name of the generic type
        /// </summary>
        private const string PLATFORM_GENERIC = "generic";

        /// <summary>
        /// String name of the platform
        /// </summary>
        private string platform;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationPlatformType"/> class.
        /// </summary>
        /// <param name="platform">String name of the platform</param>
        /// <remarks>
        /// Examples of the string name include:
        ///     windows
        ///     mac
        ///     iphone
        ///     internet
        ///     rss
        ///     notify.io
        /// </remarks>
        public DestinationPlatformType(string platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Gets the string name of the platform
        /// </summary>
        /// <value>string</value>
        public string Name
        {
            get
            {
                return this.platform;
            }
        }

        /// <summary>
        /// Gets the icon associated the represents the platform type.
        /// </summary>
        /// <returns><see cref="System.Drawing.Image"/></returns>
        public virtual Image GetIcon()
        {
            return Growl.Destinations.Properties.Resources.other;
        }
    }
}
