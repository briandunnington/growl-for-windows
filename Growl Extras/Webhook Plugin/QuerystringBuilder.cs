using System;
using System.Collections.Generic;
using System.Text;

namespace Webhook_Plugin
{
    /// <summary>
    /// Builds a querystring (or form post data) from the specified name/object pairs.
    /// </summary>
    public class QuerystringBuilder
    {
        /// <summary>
        /// Holds the name/value pairs used to construct the querystring
        /// </summary>
        private Dictionary<string, string> items = new Dictionary<string, string>();

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.ToQuerystring();
        }

        /// <summary>
        /// Returns the constructed querystring, including the leading ?
        /// </summary>
        /// <returns>Querystring</returns>
        public string ToQuerystring()
        {
            return "?" + ToPostData();
        }

        /// <summary>
        /// Returns the constructed form post data (no leading ?)
        /// </summary>
        /// <returns>Form post data</returns>
        public string ToPostData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> item in this.items)
            {
                sb.AppendFormat("{0}={1}&", item.Key, item.Value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Adds the specified name/object pair to the querystring
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object value)
        {
            string val = (value == null ? String.Empty : System.Web.HttpUtility.UrlEncode(value.ToString()));
            this.items.Add(key, val);
        }
    }

}
