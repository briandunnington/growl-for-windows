/*
using System;

namespace Growl.Connector
{
    /// <summary>
    /// Represents the target for a url-style callback
    /// </summary>
    public class UrlCallbackTarget
    {
        public const string APP = "APP";

        /// <summary>
        /// HTTP GET method
        /// </summary>
        public const string GET = "GET";

        /// <summary>
        /// HTTP POST method
        /// </summary>
        public const string POST = "POST";

        /// <summary>
        /// The url to send the callback to
        /// </summary>
        private string url;

        /// <summary>
        /// The HTTP method to use for the callback
        /// </summary>
        private string method = GET;

        /// <summary>
        /// The url to call
        /// </summary>
        /// <value>Fully-qualified url. Example: http://www.domain.net/page.cgi </value>
        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        /// <summary>
        /// The HTTP method to use for the callback
        /// </summary>
        /// <value>The only allowable values are "GET" and "POST" - any other value will default to "GET"</value>
        public string Method
        {
            get
            {
                return this.method;
            }
            set
            {
                if (value != null)
                {
                    string m = value.Trim().ToUpper();
                    this.method = (m == GET || m == POST ? m : GET);
                }
            }
        }
    }
}
*/