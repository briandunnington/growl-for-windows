using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Daemon
{
    /// <summary>
    /// Provides methods for handling Flash policy requests
    /// </summary>
    internal static class FlashPolicy
    {
        /// <summary>
        /// String that indicates the request may be a Flash policy request
        /// </summary>
        public const string REQUEST_INDICATOR = "<";

        /// <summary>
        /// String that represents a valid Flash policy request
        /// </summary>
        public const string REQUEST = "<policy-file-request/>\0";

        /// <summary>
        /// Policy request as an array of bytes
        /// </summary>
        private static byte[] requestBytes;

        /// <summary>
        /// Policy response as an array of bytes
        /// </summary>
        private static byte[] responseBytes;

        /// <summary>
        /// Type initializer
        /// </summary>
        static FlashPolicy()
        {
            try
            {
                requestBytes = System.Text.Encoding.UTF8.GetBytes(REQUEST);
                byte[] bytes = System.IO.File.ReadAllBytes("flashpolicy.xml");
                responseBytes = new byte[bytes.Length + 1];
                Array.Copy(bytes, responseBytes, bytes.Length);
                responseBytes[bytes.Length] = 0;
            }
            catch
            {
                // flashpolicy.xml is missing?
            }
        }

        /// <summary>
        /// Gets the array of bytes that represent a valid policy request
        /// </summary>
        /// <value>
        /// array of bytes
        /// </value>
        public static byte[] RequestBytes
        {
            get
            {
                return requestBytes;
            }
        }

        /// <summary>
        /// Gets the array of bytes that represent a valid policy response
        /// </summary>
        /// <value>
        /// array of bytes
        /// </value>
        public static byte[] ResponseBytes
        {
            get
            {
                return responseBytes;
            }
        }
    }
}
