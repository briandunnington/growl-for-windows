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
        /// Initializes a new instance of the <see cref="CallbackDataBase"/> class with
        /// empty Data and Type properties.
        /// </summary>
        protected CallbackDataBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackDataBase"/> class.
        /// </summary>
        /// <param name="data">The callback data.</param>
        /// <param name="type">The callback data type.</param>
        public CallbackDataBase(string data, string type)
        {
            this.data = data;
            this.type = type;
        }

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

            CallbackDataBase context = new CallbackDataBase(data, type);

            return context;
        }
    }
}
