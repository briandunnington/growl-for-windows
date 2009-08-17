using System;
using System.Collections.Generic;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a GNTP response
    /// </summary>
    public class Response : Error
    {
        /// <summary>
        /// Indicates if this is an OK response
        /// </summary>
        private bool isOK;

        private string inResponseTo;

        /// <summary>
        /// Contains the callback information and result
        /// </summary>
        private CallbackData callbackData;

        /// <summary>
        /// Creates a new instance of the <see cref="Response"/> class,
        /// setting the IsOK property to <c>true</c>.
        /// </summary>
        public Response() : base()
        {
            this.IsOK = true;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Response"/> class,
        /// setting the ErrorCode and ErrorDescription properties.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorDescription">The error description</param>
        public Response(int errorCode, string errorDescription) : base(errorCode, errorDescription)
        {
            this.IsOK = false;
        }

        /// <summary>
        /// Gets or sets a flag that indicates if this is an OK response
        /// </summary>
        /// <value>
        /// <c>true</c> if this is an OK or CALLBACK response,
        /// <c>false</c> if this is an ERROR response
        /// </value>
        public bool IsOK
        {
            get
            {
                return this.isOK;
            }
            set
            {
                this.isOK = value;
            }
        }

        /// <summary>
        /// Gets a flag that indicates if this is an ERROR response
        /// </summary>
        /// <value>
        /// <c>true</c> if this is an ERROR response,
        /// <c>false</c> if this is any other response
        /// </value>
        public bool IsError
        {
            get
            {
                return !this.IsOK;
            }
        }

        /// <summary>
        /// Gets a flag that indicates if this is a CALLBACK response
        /// </summary>
        /// <value>
        /// <c>true</c> if this is a CALLBACK response
        /// <c>false</c> if this is any other response
        /// </value>
        public bool IsCallback
        {
            get
            {
                if (this.callbackData != null)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="CallbackData"/> if this is a callback-type response
        /// </summary>
        /// <value><see cref="CallbackData"/></value>
        internal CallbackData CallbackData
        {
            get
            {
                return this.callbackData;
            }
        }

        /// <summary>
        /// Gets or sets the type of request that this response is in response to.
        /// </summary>
        /// <value>string</value>
        public string InResponseTo
        {
            get
            {
                return this.inResponseTo;
            }
            set
            {
                this.inResponseTo = value;
            }
        }

        /// <summary>
        /// Sets the <see cref="CallbackData"/> for this response
        /// </summary>
        /// <param name="notificationID">The ID of the notification making the callback</param>
        /// <param name="callbackContext">The <see cref="CallbackContext"/> of the request</param>
        /// <param name="callbackResult">The <see cref="CallbackResult"/> (clicked, closed)</param>
        public void SetCallbackData(string notificationID, CallbackContext callbackContext, CallbackResult callbackResult)
        {
            if (callbackContext != null)
            {
                CallbackData cd = new CallbackData(callbackContext.Data, callbackContext.Type, callbackResult, notificationID);
                this.callbackData = cd;
            }
        }

        /// <summary>
        /// Converts the Response to a list of headers
        /// </summary>
        /// <returns><see cref="HeaderCollection"/></returns>
        public override HeaderCollection ToHeaders()
        {
            HeaderCollection headers = new HeaderCollection();

            if (this.IsOK && !this.IsCallback)
            {
                Header hResponseAction = new Header(Header.RESPONSE_ACTION, this.InResponseTo);

                headers.AddHeader(hResponseAction);
            }

            if (this.IsError)
            {
                Header hErrorCode = new Header(Header.ERROR_CODE, this.ErrorCode.ToString());
                Header hDescription = new Header(Header.ERROR_DESCRIPTION, this.ErrorDescription);

                headers.AddHeader(hErrorCode);
                headers.AddHeader(hDescription);
            }

            if (this.IsCallback)
            {
                Header hNotificationID = new Header(Header.NOTIFICATION_ID, this.callbackData.NotificationID);
                Header hCallbackResult = new Header(Header.NOTIFICATION_CALLBACK_RESULT, Enum.GetName(typeof(CallbackResult), this.callbackData.Result));
                Header hCallbackContext = new Header(Header.NOTIFICATION_CALLBACK_CONTEXT, this.callbackData.Data);
                Header hCallbackContextType = new Header(Header.NOTIFICATION_CALLBACK_CONTEXT_TYPE, this.callbackData.Type);
                //Header hCallbackTimestamp = new Header(Header.NOTIFICATION_CALLBACK_TIMESTAMP, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
                Header hCallbackTimestamp = new Header(Header.NOTIFICATION_CALLBACK_TIMESTAMP, DateTime.UtcNow.ToString("u"));

                headers.AddHeader(hNotificationID);
                headers.AddHeader(hCallbackResult);
                headers.AddHeader(hCallbackContext);
                headers.AddHeader(hCallbackContextType);
                headers.AddHeader(hCallbackTimestamp);
            }

            this.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// Creates a new <see cref="Response"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the response</param>
        /// <returns><see cref="Response"/></returns>
        public new static Response FromHeaders(HeaderCollection headers)
        {
            int errorCode = headers.GetHeaderIntValue(Header.ERROR_CODE, true);
            string description = headers.GetHeaderStringValue(Header.ERROR_DESCRIPTION, false);

            Response response = new Response(errorCode, description);
            SetInhertiedAttributesFromHeaders(response, headers);
            return response;
        }

        /// <summary>
        /// Sets any properties from a collection of header values
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> of header values</param>
        /// <param name="isCallback">Indicates if this is a callback response</param>
        internal void SetAttributesFromHeaders(HeaderCollection headers, bool isCallback)
        {
            if (isCallback)
            {
                CallbackData callbackData = CallbackData.FromHeaders(headers);
                this.callbackData = callbackData;
            }

            SetInhertiedAttributesFromHeaders(this, headers);
        }
    }
}
