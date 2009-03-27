using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// A base class for other callback-related classes that need to represent 
    /// the original callback Data and Type
    /// </summary>
    public class CallbackDataBase
    {
        /// <summary>
        /// The data to provide in the callback
        /// </summary>
        private string data;

        /// <summary>
        /// The type of the data provided in the callback
        /// </summary>
        private string type;

        /// <summary>
        /// The application-specified data to provide in the callback
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }

        /// <summary>
        /// The type of data specified in the <see cref="Data"/> property
        /// </summary>
        /// <string>
        /// string - NOTE: the type does not need to be of any recognized type, it can be any arbitrary string that has meaning to the notifying application
        /// </string>
        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CallbackDataBase"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="CallbackDataBase"/></returns>
        public static CallbackDataBase FromHeaders(HeaderCollection headers)
        {
            string data = headers.GetHeaderStringValue(Header.NOTIFICATION_CALLBACK_CONTEXT, true);
            string type = headers.GetHeaderResourceValue(Header.NOTIFICATION_CALLBACK_CONTEXT_TYPE, true);

            CallbackDataBase context = new CallbackDataBase();
            context.Data = data;
            context.Type = type;

            return context;
        }
    }
}
