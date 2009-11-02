using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents the data returned during a callback, including the original Data and Type, 
    /// as well as the resulting action performed by the user
    /// </summary>
    public class CallbackData : CallbackDataBase
    {
        /// <summary>
        /// The callback result
        /// </summary>
        private CallbackResult result;

        /// <summary>
        /// The ID of the notification making the callback
        /// </summary>
        private string notificationID;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackData"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        /// <param name="result">The result.</param>
        /// <param name="notificationID">The notification ID.</param>
        public CallbackData(string data, string type, CallbackResult result, string notificationID)
            : base(data, type)
        {
            this.result = result;
            this.notificationID = notificationID;
        }

        /// <summary>
        /// The callback result (clicked, closed, etc)
        /// </summary>
        /// <value>
        /// <see cref="CallbackResult"/>
        /// </value>
        public CallbackResult Result
        {
            get
            {
                return this.result;
            }
        }

        /// <summary>
        /// Gets or sets the ID of the notification making the callback
        /// </summary>
        public string NotificationID
        {
            get
            {
                return this.notificationID;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CallbackData"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="CallbackData"/></returns>
        public new static CallbackData FromHeaders(HeaderCollection headers)
        {
            try
            {
                CallbackDataBase baseObj = CallbackDataBase.FromHeaders(headers);
                CallbackResult result = CallbackResult.TIMEDOUT;

                string resultString = headers.GetHeaderStringValue(Header.NOTIFICATION_CALLBACK_RESULT, true);
                if(!String.IsNullOrEmpty(resultString))
                    result = (CallbackResult)Enum.Parse(typeof(CallbackResult), resultString, true);

                string notificationID = headers.GetHeaderStringValue(Header.NOTIFICATION_ID, false);

                CallbackData context = new CallbackData(baseObj.Data, baseObj.Type, result, notificationID);

                return context;
            }
            catch
            {
                return null;
            }
        }
    }
}
