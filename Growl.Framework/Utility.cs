using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Growl.Framework
{
    /// <summary>
    /// Provides various utility methods used throughout the Growl framework
    /// </summary>
    internal class Utility
    {
        /// <summary>
        /// Computes the MD5 hash on the byte array and returns the hash
        /// </summary>
        /// <param name="originalBytes">The byte array to compute</param>
        /// <returns>MD5 hash as an array of bytes</returns>
        public static byte[] MD5(byte[] originalBytes)
        {
            byte[] encodedBytes;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            encodedBytes = md5.ComputeHash(originalBytes);
            return encodedBytes;

            /*
            // Bytes to string
            return System.Text.RegularExpressions.Regex.Replace(BitConverter.ToString(encodedBytes), "-", "").ToLower();
             * */
        }
    }
}
