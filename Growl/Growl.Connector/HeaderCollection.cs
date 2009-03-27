using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents a set of <see cref="Header"/>s
    /// </summary>
    public class HeaderCollection : List<Header>
    {
        /// <summary>
        /// Contains a list of just the regular (defined) headers in the collection
        /// </summary>
        private List<Header> headers = new List<Header>();

        /// <summary>
        /// Contains a list of just the custom headers in the collection
        /// </summary>
        private List<Header> customHeaders = new List<Header>();

        /// <summary>
        /// Contains a list of just the application-specific data headers in the collection
        /// </summary>
        private List<Header> dataHeaders = new List<Header>();

        /// <summary>
        /// Contains a list of just the resource pointer headers in the collection
        /// </summary>
        private List<Header> pointers = new List<Header>();

        /// <summary>
        /// Contains a list of all of the headers in the collection, regardless of type
        /// </summary>
        private Dictionary<string, Header> allHeaders = new Dictionary<string, Header>();

        /// <summary>
        /// Adds a <see cref="Header"/> to the collection
        /// </summary>
        /// <param name="header"><see cref="Header"/></param>
        public void AddHeader(Header header)
        {
            if (header != null && header.IsValid)
            {
                if (header.IsGrowlResourcePointer)
                    this.pointers.Add(header);
                else if (header.IsCustomHeader)
                    this.customHeaders.Add(header);
                else if (header.IsDataHeader)
                    this.dataHeaders.Add(header);
                else
                    this.headers.Add(header);
                this.Add(header);
                this.allHeaders.Add(header.Name, header);
            }
        }

        /// <summary>
        /// Adds all of the headers in <paramref name="headers"/> to the
        /// currently collection.
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the headers to add</param>
        public void AddHeaders(HeaderCollection headers)
        {
            foreach (Header header in headers)
            {
                this.AddHeader(header);
            }
        }

        /// <summary>
        /// Gets a list of all of the normal (defined) headers in the collection, 
        /// excluding any custom headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<Header> Headers
        {
            get
            {
                return this.headers;
            }
        }

        /// <summary>
        /// Gets a list of all of the custom headers in the collection, 
        /// excluding any normal (defined) headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<Header> CustomHeaders
        {
            get
            {
                return this.customHeaders;
            }
        }

        /// <summary>
        /// Gets a list of all of the application-specific data headers in the collection, 
        /// excluding any normal (defined) headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<Header> DataHeaders
        {
            get
            {
                return this.dataHeaders;
            }
        }

        /// <summary>
        /// Gets a list of all of the resource pointer headers in the collection, 
        /// excluding any other headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<Header> Pointers
        {
            get
            {
                return this.pointers;
            }
        }
 
        /// <summary>
        /// Associates the specified <paramref name="binaryData"/> to its related header.
        /// </summary>
        /// <param name="binaryData"><see cref="BinaryData"/></param>
        public void AssociateBinaryData(BinaryData binaryData)
        {
            foreach (Header header in this.pointers)
            {
                if (header.IsGrowlResourcePointer && header.GrowlResourcePointerID == binaryData.ID)
                {
                    header.GrowlResource = binaryData;
                    break;
                }
            }
        }

        /// <summary>
        /// Looks up the <see cref="Header"/> in the collection by the header name.
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns><see cref="Header"/></returns>
        public Header Get(string name)
        {
            if (this.allHeaders.ContainsKey(name))
            {
                return this.allHeaders[name];
            }
            else
            {
                return Header.NotFoundHeader;
            }
        }

        /// <summary>
        /// Gets the string value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>string - header value</returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>null</c> will be returned.
        /// </remarks>
        public string GetHeaderStringValue(string name, bool required)
        {
            Header header = Get(name);
            if (required && (header == null || header.Value == null)) ThrowRequiredHeaderMissingException(name);
            return header.Value;
        }

        /// <summary>
        /// Gets the boolean value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>bool - header value</returns>
        /// <remarks>
        /// Valid <c>true</c> values include "TRUE" and "YES" in upper or lower case - 
        /// all other values will be considered <c>false</c>.
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>false</c> will be returned.
        /// </remarks>
        public bool GetHeaderBooleanValue(string name, bool required)
        {
            bool b = false;
            string val = GetHeaderStringValue(name, required);
            if (!String.IsNullOrEmpty(val))
            {
                val = val.ToUpper();
                switch (val)
                {
                    case "TRUE" :
                    case "YES" :
                        b = true;
                        break;
                }
            }
            return b;
        }

        /// <summary>
        /// Gets the integer value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>int - header value</returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>zero</c> will be returned.
        /// </remarks>
        public int GetHeaderIntValue(string name, bool required)
        {
            string val = GetHeaderStringValue(name, required);
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns><see cref="Resource"/></returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>null</c> will be returned.
        /// </remarks>
        public Resource GetHeaderResourceValue(string name, bool required)
        {
            Header header = Get(name);
            if (required && (header == null || header.Value == null)) ThrowRequiredHeaderMissingException(name);
            if (header.IsGrowlResourcePointer)
                return header.GrowlResource;
            else
                return header.Value;
        }

        /// <summary>
        /// Creates a <see cref="HeaderCollection"/> from a message
        /// </summary>
        /// <param name="message">The message to parse</param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection FromMessage(string message)
        {
            HeaderCollection headers = new HeaderCollection();
            string[] lines = message.Split('\r', '\n');
            foreach(string line in lines)
            {
                Header header = Header.ParseHeader(line);
                if (header != null)
                {
                    headers.AddHeader(header);
                }
            }

            return headers;
        }

        /// <summary>
        /// Throws a <see cref="GrowlException"/> with an error description that indicates that
        /// a requested required header was not found.
        /// </summary>
        /// <param name="headerName">The header name that was not found</param>
        private static void ThrowRequiredHeaderMissingException(string headerName)
        {
            throw new GrowlException(ErrorCode.REQUIRED_HEADER_MISSING, ErrorDescription.REQUIRED_HEADER_MISSING, headerName);
        }
    }
}
