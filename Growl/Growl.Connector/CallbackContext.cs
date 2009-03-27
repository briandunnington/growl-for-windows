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
        /// <summary>
        /// The url target information
        /// </summary>
        private UrlCallbackTarget target;

        /// <summary>
        /// Indicates if the callback context is properly set
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !(String.IsNullOrEmpty(this.Data) || String.IsNullOrEmpty(this.Type));
            }
        }

        /// <summary>
        /// Sets the target information if the callback is a url callback
        /// </summary>
        /// <param name="target"><see cref="UrlCallbackTarget"/></param>
        public void SetUrlCallbackTarget(UrlCallbackTarget target)
        {
            this.target = target;
        }

        /// <summary>
        /// Gets the target information if the callback is a url callback
        /// </summary>
        /// <returns></returns>
        public UrlCallbackTarget GetUrlCallbackTarget()
        {
            return this.target;
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
                (target == null || String.IsNullOrEmpty(target.Url)))
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

            CallbackContext context = new CallbackContext();
            context.Data = baseObj.Data;
            context.Type = baseObj.Type;

            return context;
        }
    }
}
