using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a custom (non-defined) header
    /// </summary>
    internal class CustomHeader : Header
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CustomHeader"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public CustomHeader(string name, string val) 
            : base(FormatName(name), val)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomHeader"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public CustomHeader(string name, bool val)
            : base(FormatName(name), val)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomHeader"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public CustomHeader(string name, Resource val)
            : base(FormatName(name), val)
        {
        }

        /// <summary>
        /// Formats the custom header name by prepending the appropriate prefix
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>The header name with the custom header prefix added</returns>
        private static string FormatName(string name)
        {
            return String.Format("{0}{1}", CUSTOM_HEADER_PREFIX, name);
        }
    }
}
