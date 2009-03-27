using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents application-specific headers that are not used by Growl
    /// </summary>
    internal class DataHeader : Header
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataHeader"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public DataHeader(string name, string val)
            : base(FormatName(name), val)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DataHeader"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public DataHeader(string name, bool val)
            : base(FormatName(name), val)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DataHeader"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public DataHeader(string name, Resource val)
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
            return String.Format("{0}{1}", DATA_HEADER_PREFIX, name);
        }
    }
}
