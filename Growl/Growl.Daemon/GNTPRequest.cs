using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;
using Growl.CoreLibrary;

namespace Growl.Daemon
{
    /// <summary>
    /// Represents a valid parsed GNTP request
    /// </summary>
    public class GNTPRequest
    {
        /// <summary>
        /// The version of the GNTP request
        /// </summary>
        private string version;

        /// <summary>
        /// The type of GNTP request
        /// </summary>
        private RequestType directive;

        /// <summary>
        /// The key used to validate and encrypt the message
        /// </summary>
        private Key key;

        /// <summary>
        /// The collection of headers parsed from the current request
        /// </summary>
        private HeaderCollection headers;

        /// <summary>
        /// The name of the application sending the request
        /// </summary>
        private string applicationName;

        /// <summary>
        /// A collection of the groups of headers for each notification type to be registered
        /// </summary>
        private List<HeaderCollection> notificationsToBeRegistered;

        /// <summary>
        /// The callback context associated with the request
        /// </summary>
        private CallbackContext callbackContext;


        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPRequest"/> class.
        /// </summary>
        /// <param name="version">The version of the GNTP request.</param>
        /// <param name="directive">The type of GNTP request.</param>
        /// <param name="key">The key used to validate and encrypt the message.</param>
        /// <param name="headers">The collection of headers parsed from the current request.</param>
        /// <param name="applicationName">The name of the application sending the request.</param>
        /// <param name="notificationsToBeRegistered">A collection of the groups of headers for each notification type to be registered.</param>
        /// <param name="callbackContext">The callback context associated with the request.</param>
        public GNTPRequest(string version, RequestType directive, Key key, HeaderCollection headers, string applicationName, List<HeaderCollection> notificationsToBeRegistered, CallbackContext callbackContext)
        {
            this.version = version;
            this.directive = directive;
            this.key = key;
            this.headers = headers;
            this.applicationName = applicationName;
            this.notificationsToBeRegistered = notificationsToBeRegistered;
            this.callbackContext = callbackContext;
        }

        /// <summary>
        /// Gets the version of the GNTP request
        /// </summary>
        /// <value>The only supported value is currently: 1.0</value>
        public string Version
        {
            get
            {
                return this.version;
            }
        }

        /// <summary>
        /// Gets the type of the request
        /// </summary>
        /// <value><see cref="RequestType"/></value>
        public RequestType Directive
        {
            get
            {
                return this.directive;
            }
        }

        /// <summary>
        /// Gets the <see cref="Key"/> used to validate and encrypt the request
        /// </summary>
        /// <value><see cref="Key"/></value>
        internal Key Key
        {
            get
            {
                return this.key;
            }
        }

        /// <summary>
        /// Gets the list of headers parsed from the request.
        /// </summary>
        /// <value><see cref="HeaderCollection"/></value>
        public HeaderCollection Headers
        {
            get
            {
                return this.headers;
            }
        }

        /// <summary>
        /// Gets the name of the application sending the request
        /// </summary>
        /// <value>string</value>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
        }

        /// <summary>
        /// Gets the collection of groups of headers for all notifications to be registered.
        /// </summary>
        /// <value><see cref="List{HeaderCollection}"/></value>
        public List<HeaderCollection> NotificationsToBeRegistered
        {
            get
            {
                return this.notificationsToBeRegistered;
            }
        }

        /// <summary>
        /// Gets the callback context associated with the request.
        /// </summary>
        /// <value><see cref="CallbackContext"/></value>
        public CallbackContext CallbackContext
        {
            get
            {
                return this.callbackContext;
            }
        }
    }
}
