using System;
using System.Collections.Generic;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Represents additional application-specific data that can be passed with a request and
    /// will be returned with the response from Growl. The actual items and their values are
    /// not used by Growl.
    /// </summary>
    [Serializable]
    public class RequestData : Dictionary<string, string>
    {
        /// <summary>
        /// Converts the object to a list of headers
        /// </summary>
        /// <returns><see cref="HeaderCollection"/></returns>
        public HeaderCollection ToHeaders()
        {
            HeaderCollection headers = new HeaderCollection();

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> item in this)
                {
                    Header dataHeader = new DataHeader(item.Key, item.Value);
                    headers.AddHeader(dataHeader);
                }
            }

            return headers;
        }

        /// <summary>
        /// Creates a new <see cref="RequestData"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="RequestData"/></returns>
        public static RequestData FromHeaders(HeaderCollection headers)
        {
            RequestData rd = new RequestData();

            if (headers != null)
            {
                foreach (Header header in headers.DataHeaders)
                {
                    if (header != null)
                    {
                        rd.Add(header.ActualName, header.Value);
                    }
                }
            }

            return rd;
        }
    }
}
