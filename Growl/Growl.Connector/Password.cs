using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a password that can be used to authorize notifications
    /// </summary>
    [Serializable]
    public class Password : ISerializable
    {
        /// <summary>
        /// Default password description when none is provided
        /// </summary>
        private const string DEFAULT_DESCRIPTION = "[No description provided]";

        /// <summary>
        /// The actual password
        /// </summary>
        private string password;

        /// <summary>
        /// A description of the password
        /// </summary>
        private string description;


        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        /// <param name="password">The actual password</param>
        public Password(string password)
            : this(password, DEFAULT_DESCRIPTION)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        /// <param name="password">The actual password</param>
        /// <param name="description">A description of the password</param>
        public Password(string password, string description)
        {
            this.password = password;
            this.description = description;
        }

        /// <summary>
        /// Initializes a new instance of the Password class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized password data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// The serialization routine uses a simple Base64 encoding on the password text so that the passwords are
        /// not stored in clear text, but there is no additional security provided to the serialized password data.
        /// </remarks>
        protected Password(SerializationInfo info, StreamingContext context)
        {
            string p = info.GetString("password");
            string d = info.GetString("description");

            this.password = Base64.Decode(p);
            this.description = d;
        }

        /// <summary>
        /// Gets or sets the actual password.
        /// </summary>
        /// <value>string</value>
        public string ActualPassword
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        /// <summary>
        /// Gets or sets a description of the password
        /// </summary>
        /// <value>string</value>
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        #region ISerializable Members

        /// <summary>
        /// Prepares the password for serialization
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized password data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <remarks>
        /// The serialization routine uses a simple Base64 encoding on the password text so that the passwords are
        /// not stored in clear text, but there is no additional security provided to the serialized password data.
        /// </remarks>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string p = Base64.Encode(this.password);
            info.AddValue("password", p, typeof(string));
            info.AddValue("description", this.description, typeof(string));
        }

        #endregion
    }
}
