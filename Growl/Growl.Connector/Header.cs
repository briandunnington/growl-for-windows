using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a single header in a GNTP message
    /// </summary>
    public class Header
    {
        /// <summary>
        /// The prefix used for application-specific (non-defined) data headers
        /// </summary>
        protected const string DATA_HEADER_PREFIX = "Data-";

        /// <summary>
        /// The prefix used for custom (non-defined) headers
        /// </summary>
        protected const string CUSTOM_HEADER_PREFIX = "X-";

        /// <summary>
        /// The prefix used to identify binary resources that will be passed along with the message
        /// </summary>
        private const string GROWL_RESOURCE_POINTER_PREFIX = "x-growl-resource://";

        /// <summary>
        /// The value of a boolean header that is set to true
        /// </summary>
        private const string BOOL_HEADER_TRUE_VALUE = "Yes";

        /// <summary>
        /// The value of a boolean header that is set to false
        /// </summary>
        private const string BOOL_HEADER_FALSE_VALUE = "No";

        /// <summary>
        /// The RegEx group name for the match that contains the header name (see regExHeader expression below)
        /// </summary>
        private const string HEADER_NAME_REGEX_GROUP_NAME = "HeaderName";

        /// <summary>
        /// The RegEx group name for the match that contains the header value (see regExHeader expression below)
        /// </summary>
        private const string HEADER_VALUE_REGEX_GROUP_NAME = "HeaderValue";

        /// <summary>
        /// The regular expression used to parse the header line
        /// </summary>
        //private static Regex regExHeader = new Regex(@"(?<HeaderName>[^\r\n:]+):\s+(?<HeaderValue>([\s\S]*\r\n)|(.+))");
        private static Regex regExHeader = new Regex(@"(?<HeaderName>[^\r\n:]+):\s+(?<HeaderValue>([\s\S]*\Z)|(.+))");

        /// <summary>
        /// A special <see cref="Header"/> that represents a header that was not found in the message
        /// </summary>
        public static readonly Header NotFoundHeader = new Header();

        /// <summary>
        /// The header name
        /// </summary>
        private string name;

        /// <summary>
        /// The header value
        /// </summary>
        private string val;

        /// <summary>
        /// Indicates if the header is valid or not
        /// </summary>
        private bool isValid;

        /// <summary>
        /// Indicates if the header is a blank line or not
        /// </summary>
        private bool isBlankLine;

        /// <summary>
        /// Indicates if the header is a pointer to a binary resource
        /// </summary>
        private bool isGrowlResourcePointer;

        /// <summary>
        /// The resource pointer value
        /// </summary>
        private string growlResourcePointerID;

        /// <summary>
        /// Indicates if this header is an 'Identifier' header
        /// </summary>
        private bool isIdentifier;

        /// <summary>
        /// Indicates if this header is a custom header
        /// </summary>
        private bool isCustomHeader;

        /// <summary>
        /// Indicates if this header is a application-specific data header
        /// </summary>
        private bool isDataHeader;

        /// <summary>
        /// The binary data associated with this header (if applicable)
        /// </summary>
        private BinaryData growlResource = null;

        /// <summary>
        /// Creates a new uninitialized instance of the <see cref="Header"/> class.
        /// </summary>
        public Header()
        {
            // skip initialize
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Header"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public Header(string name, string val)
        {
            Initialize(name, val);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Header"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public Header(string name, bool val)
        {
            string yesOrNo = (val ? BOOL_HEADER_TRUE_VALUE : BOOL_HEADER_FALSE_VALUE);
            Initialize(name, yesOrNo);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Header"/> class
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        public Header(string name, Resource val)
        {
            if (val != null)
            {
                Initialize(name, val.ToString());
                if (val.IsRawData) this.GrowlResource = val.Data;
            }
        }

        /// <summary>
        /// Initializes the header object
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="val">The header value</param>
        private void Initialize(string name, string val)
        {
            this.name = name;
            this.val = val;
            this.isValid = true;
            if (name == null && val == null) this.isBlankLine = true;
            if (!this.isBlankLine)
            {
                if (val != null && val.StartsWith(GROWL_RESOURCE_POINTER_PREFIX))
                {
                    this.isGrowlResourcePointer = true;
                    this.growlResourcePointerID = val.Replace(GROWL_RESOURCE_POINTER_PREFIX, "");
                }

                if (name != null && name == Header.RESOURCE_IDENTIFIER)
                {
                    this.isIdentifier = true;
                }

                if (name != null && name.StartsWith(CUSTOM_HEADER_PREFIX))
                {
                    this.isCustomHeader = true;
                }
                else if (name != null && name.StartsWith(DATA_HEADER_PREFIX))
                {
                    this.isDataHeader = true;
                }
            }
        }

        /// <summary>
        /// Gets the header name
        /// </summary>
        /// <value>
        /// string - Ex: Application-Name: SurfWriter
        /// </value>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the actual header name
        /// </summary>
        /// <value>
        /// string
        /// </value>
        /// <remarks>
        /// If the header is a defined header, this property returns the same value as the Name property.
        /// If the header is a custom header, this property returns the header name with the custom header prefix removed.
        /// If the header is a data header, this property returns the header name with the custom header prefix removed.
        /// </remarks>
        public string ActualName
        {
            get
            {
                if (this.IsCustomHeader)
                    return this.Name.Remove(0, CUSTOM_HEADER_PREFIX.Length);
                else if (this.isDataHeader)
                    return this.name.Remove(0, DATA_HEADER_PREFIX.Length);

                return this.Name;
            }
        }

        /// <summary>
        /// Gets the value of the header
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string Value
        {
            get
            {
                return this.val;
            }
        }

        /// <summary>
        /// Indicates if the header is valid or not
        /// </summary>
        /// <value>
        /// bool - this value is only <c>true</c> for special cases, like <see cref="Header.NotFoundHeader"/>
        /// </value>
        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        /// <summary>
        /// Indicates if the header is a blank line of not
        /// </summary>
        /// <value>
        /// <c>true</c> if the header is a blank line,
        /// <c>false</c> otherwise
        /// </value>
        public bool IsBlankLine
        {
            get
            {
                return this.isBlankLine;
            }
        }

        /// <summary>
        /// Indicates if the header is a custom header or not
        /// </summary>
        /// <value>
        /// <c>true</c> if the header is a custom header,
        /// <c>false</c> if the header is a defined header
        /// </value>
        public bool IsCustomHeader
        {
            get
            {
                return this.isCustomHeader;
            }
        }

        /// <summary>
        /// Indicates if the header is an application-specific data header or not
        /// </summary>
        /// <value>
        /// <c>true</c> if the header is a data header,
        /// <c>false</c> if the header is a defined header
        /// </value>
        public bool IsDataHeader
        {
            get
            {
                return this.isDataHeader;
            }
        }

        /// <summary>
        /// Indicates if the header is a <see cref="Header.RESOURCE_IDENTIFIER"/> header
        /// </summary>
        /// <value>
        /// <c>true</c> if the header is a <see cref="Header.RESOURCE_IDENTIFIER"/> header,
        /// <c>false</c> otherwise
        /// </value>
        public bool IsIdentifier
        {
            get
            {
                return this.isIdentifier;
            }
        }

        /// <summary>
        /// Indicates if the header is a binary resource pointer
        /// </summary>
        /// <value>
        /// <c>true</c> if the header is a binary resource pointer,
        /// <c>false</c> otherwise
        /// </value>
        public bool IsGrowlResourcePointer
        {
            get
            {
                return this.isGrowlResourcePointer;
            }
        }

        /// <summary>
        /// Gets the resource pointer value if this is a resource pointer header
        /// </summary>
        /// <value>
        /// string - Ex: x-growl-resource://1234567890
        /// </value>
        public string GrowlResourcePointerID
        {
            get
            {
                return this.growlResourcePointerID;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="BinaryData"/> associated with this header if this
        /// is a binary resource pointer header.
        /// </summary>
        /// <value>
        /// <see cref="BinaryData"/> or <c>null</c> if this is not a resource pointer header
        /// </value>
        public BinaryData GrowlResource
        {
            get
            {
                return this.growlResource;
            }
            set
            {
                this.growlResource = value;
            }
        }

        /// <summary>
        /// Creates a <see cref="Header"/> from a message line
        /// </summary>
        /// <param name="line">The individual message line</param>
        /// <returns><see cref="Header"/></returns>
        public static Header ParseHeader(string line)
        {
            Header header = null;

            if (line != null)
            {
                line = line.Trim();
                if(String.IsNullOrEmpty(line))
                {
                    header = new Header();
                }
                else
                {
                    // move this to static member variable
                    Match m = regExHeader.Match(line);
                    if (m.Success)
                    {
                        header = new Header(m.Groups[HEADER_NAME_REGEX_GROUP_NAME].Value.Trim(), m.Groups[HEADER_VALUE_REGEX_GROUP_NAME].Value.Trim());
                    }
                }
            }
            return header;
        }

        /// <summary>
        /// Response-Action header
        /// </summary>
        public const string RESPONSE_ACTION = "Response-Action";
        /// <summary>
        /// Application-Name header
        /// </summary>
        public const string APPLICATION_NAME = "Application-Name";
        /// <summary>
        /// Application-Icon header
        /// </summary>
        public const string APPLICATION_ICON = "Application-Icon";
        /// <summary>
        /// Notifications-Count header
        /// </summary>
        public const string NOTIFICATIONS_COUNT = "Notifications-Count";
        /// <summary>
        /// Notification-Name header
        /// </summary>
        public const string NOTIFICATION_NAME = "Notification-Name";
        /// <summary>
        /// Notification-Display-Name header
        /// </summary>
        public const string NOTIFICATION_DISPLAY_NAME = "Notification-Display-Name";
        /// <summary>
        /// Notification-Enabled header
        /// </summary>
        public const string NOTIFICATION_ENABLED = "Notification-Enabled";
        /// <summary>
        /// Notification-Icon header
        /// </summary>
        public const string NOTIFICATION_ICON = "Notification-Icon";
        /// <summary>
        /// Notification-ID header
        /// </summary>
        public const string NOTIFICATION_ID = "Notification-ID";
        /// <summary>
        /// Notification-Title header
        /// </summary>
        public const string NOTIFICATION_TITLE = "Notification-Title";
        /// <summary>
        /// Notification-Text header
        /// </summary>
        public const string NOTIFICATION_TEXT = "Notification-Text";
        /// <summary>
        /// Notification-Sticky header
        /// </summary>
        public const string NOTIFICATION_STICKY = "Notification-Sticky";
        /// <summary>
        /// Notification-Priority header
        /// </summary>
        public const string NOTIFICATION_PRIORITY = "Notification-Priority";
        /// <summary>
        /// Notification-Coalescing-ID header
        /// </summary>
        public const string NOTIFICATION_COALESCING_ID = "Notification-Coalescing-ID";
        /// <summary>
        /// Notification-Callback-Result header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_RESULT = "Notification-Callback-Result";
        /// <summary>
        /// Notification-Callback-Timestamp header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_TIMESTAMP = "Notification-Callback-Timestamp";
        /// <summary>
        /// Notification-Callback-Context header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT = "Notification-Callback-Context";
        /// <summary>
        /// Notification-Callback-Context-Type header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT_TYPE = "Notification-Callback-Context-Type";
        /// <summary>
        /// Notification-Callback-Context-Target header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT_TARGET = "Notification-Callback-Context-Target";
        /// <summary>
        /// Notification-Callback-Context-Target-Method header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT_TARGET_METHOD = "Notification-Callback-Context-Target-Method";
        /// <summary>
        /// Identifier header
        /// </summary>
        public const string RESOURCE_IDENTIFIER = "Identifier";
        /// <summary>
        /// Length header
        /// </summary>
        public const string RESOURCE_LENGTH = "Length";
        /// <summary>
        /// Origin-Machine-Name header
        /// </summary>
        public const string ORIGIN_MACHINE_NAME = "Origin-Machine-Name";
        /// <summary>
        /// Origin-Software-Name header
        /// </summary>
        public const string ORIGIN_SOFTWARE_NAME = "Origin-Software-Name";
        /// <summary>
        /// Origin-Software-Version header
        /// </summary>
        public const string ORIGIN_SOFTWARE_VERSION = "Origin-Software-Version";
        /// <summary>
        /// Origin-Platform-Name header
        /// </summary>
        public const string ORIGIN_PLATFORM_NAME = "Origin-Platform-Name";
        /// <summary>
        /// Origin-Platform-Version header
        /// </summary>
        public const string ORIGIN_PLATFORM_VERSION = "Origin-Platform-Version";
        /// <summary>
        /// Error-Code header
        /// </summary>
        public const string ERROR_CODE = "Error-Code";
        /// <summary>
        /// Error-Description header
        /// </summary>
        public const string ERROR_DESCRIPTION = "Error-Description";
        /// <summary>
        /// Received header
        /// </summary>
        public const string RECEIVED = "Received";
        /// <summary>
        /// Subscriber-ID header
        /// </summary>
        public const string SUBSCRIBER_ID = "Subscriber-ID";
        /// <summary>
        /// Subscriber-Name header
        /// </summary>
        public const string SUBSCRIBER_NAME = "Subscriber-Name";
        /// <summary>
        /// Subscriber-Port header
        /// </summary>
        public const string SUBSCRIBER_PORT = "Subscriber-Port";
        /// <summary>
        /// Subscription-TTL header
        /// </summary>
        public const string SUBSCRIPTION_TTL = "Subscription-TTL";
    }
}
