using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// Represents the information needed to perform a callback to the notifying application
    /// </summary>
    public class CallbackContext : CallbackDataBase
    {
        private string url;

        public CallbackContext(string data, string type) : base(data, type)
        {
        }

        public CallbackContext(string url)
        {
            this.url = url;
        }

        private CallbackContext()
        {
        }

        public string CallbackUrl
        {
            get
            {
                return this.url;
            }
        }

        /// <summary>
        /// Indicates if the receiving server should keep the connection open to do the callback
        /// </summary>
        /// <returns>
        /// <c>true</c> if the connection needs to be kept open,
        /// <c>false</c> if the connection can be closed (url callback)
        /// </returns>
        public bool ShouldKeepConnectionOpen()
        {
            if (!String.IsNullOrEmpty(this.Data) && !String.IsNullOrEmpty(this.Type) && 
                (String.IsNullOrEmpty(this.CallbackUrl)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a new <see cref="CallbackContext"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="CallbackContext"/></returns>
        public new static CallbackContext FromHeaders(HeaderCollection headers)
        {
            CallbackDataBase baseObj = CallbackDataBase.FromHeaders(headers);

            CallbackContext context = new CallbackContext(baseObj.Data, baseObj.Type);

            return context;
        }
    }
}
