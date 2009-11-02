using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// The base exception type for any exceptions thrown from Growl code.
    /// </summary>
    [Serializable]
    public class GrowlException : Exception
    {
        /// <summary>
        /// The <see cref="ErrorCode"/> of the exception
        /// </summary>
        private int errorCode = Growl.Connector.ErrorCode.INTERNAL_SERVER_ERROR;

        /// <summary>
        /// Any additional information associated with the exception
        /// </summary>
        private object[] args;

        /// <summary>
        /// Creates a new instance of the <see cref="GrowlException"/> class.
        /// </summary>
        /// <param name="errorCode">The <see cref="ErrorCode"/> of the exception</param>
        /// <param name="errorDescription">The <see cref="ErrorDescription"/> of the exception</param>
        /// <param name="args">Any additional information associated with the exception</param>
        public GrowlException(int errorCode, string errorDescription, params object[] args) : base(errorDescription)
        {
            this.errorCode = errorCode;
            this.args = args;
        }

        /// <summary>
        /// Gets the <see cref="ErrorCode"/> of the exception
        /// </summary>
        /// <value>
        /// int
        /// </value>
        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        /// <summary>
        /// Gets any additional information associated with the exception
        /// </summary>
        /// <value>
        /// Array of objects
        /// </value>
        public object[] AdditionalInfo
        {
            get
            {
                return this.args;
            }
        }
    }
}
