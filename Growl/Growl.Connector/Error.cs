using System;
using System.Collections.Generic;

namespace Growl.Connector
{
    /// <summary>
    /// Represents an Error response
    /// </summary>
    public class Error : ExtensibleObject
    {
        /// <summary>
        /// The error code of the response
        /// </summary>
        private int errorCode = 0;

        /// <summary>
        /// The error description of the response
        /// </summary>
        private string description;

        /// <summary>
        /// Creates a new instance of the <see cref="Error"/> class
        /// without setting the error code or description.
        /// </summary>
        protected Error()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="description">The error description</param>
        public Error(int errorCode, string description)
        {
            this.errorCode = errorCode;
            this.description = description;
        }

        /// <summary>
        /// Gets the error code of the response
        /// </summary>
        /// <value>int</value>
        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        /// <summary>
        /// Gets the error description of the response
        /// </summary>
        /// <value>string</value>
        public string ErrorDescription
        {
            get
            {
                return this.description;
            }
        }

        /// <summary>
        /// Converts the Error to a list of headers
        /// </summary>
        /// <returns><see cref="HeaderCollection"/></returns>
        public virtual HeaderCollection ToHeaders()
        {
            Header hErrorCode = new Header(Header.ERROR_CODE, this.ErrorCode.ToString());
            Header hDescription = new Header(Header.ERROR_DESCRIPTION, this.ErrorDescription);

            HeaderCollection headers = new HeaderCollection();
            headers.AddHeader(hErrorCode);
            headers.AddHeader(hDescription);

            this.AddInheritedAttributesToHeaders(headers);
            return headers;
        }

        /// <summary>
        /// Creates a new <see cref="Error"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Error"/></returns>
        public static Error FromHeaders(HeaderCollection headers)
        {
            int errorCode = headers.GetHeaderIntValue(Header.ERROR_CODE, true);
            string description = headers.GetHeaderStringValue(Header.ERROR_DESCRIPTION, false);

            Error error = new Error(errorCode, description);
            SetInhertiedAttributesFromHeaders(error, headers);
            return error;
        }
    }
}
