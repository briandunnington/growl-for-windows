using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Provides convenience methods for Base64 encoding and decoding strings
    /// </summary>
    public static class Base64
    {
        /// <summary>
        /// Encodes a string using Base64 format
        /// </summary>
        /// <param name="str">The string to encode</param>
        /// <returns>Base64 encoded string</returns>
        public static string Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// Decodes a Base64 encoded string
        /// </summary>
        /// <param name="str">The string to decode</param>
        /// <returns>The decoded string</returns>
        public static string Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }
    }
}
