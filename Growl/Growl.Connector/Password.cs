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
        /// Indicates if the password is permanent (user-specified) vs. temporary (automatically added by a subscription)
        /// </summary>
        private bool permanent;


        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        /// <param name="password">The actual password</param>
        /// <param name="permanent">Indicates if the password is permanent (user-specified) vs. temporary (automatically added by a subscription)</param>
        public Password(string password, bool permanent)
            : this(password, DEFAULT_DESCRIPTION, permanent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        /// <param name="password">The actual password</param>
        /// <param name="description">A description of the password</param>
        /// <param name="permanent">Indicates if the password is permanent (user-specified) vs. temporary (automatically added by a subscription)</param>
        public Password(string password, string description, bool permanent)
        {
            this.password = password;
            this.description = description;
            this.permanent = permanent;
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
            bool m = true;
            try
            {
                m = info.GetBoolean("permanent");
            }
            catch
            {
            }

            this.password = Base64.Decode(p);
            this.description = d;
            this.permanent = m;
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

        /// <summary>
        /// Gets or sets a value that indicates if this is a permanent password
        /// </summary>
        /// <value>bool</value>
        public bool Permanent
        {
            get
            {
                return this.permanent;
            }
            set
            {
                this.permanent = value;
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
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string p = Base64.Encode(this.password);
            info.AddValue("password", p, typeof(string));
            info.AddValue("description", this.description, typeof(string));
            info.AddValue("permanent", this.permanent);
        }

        #endregion
    }
}
