using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Represents binary data that can be sent in a GNTP message
    /// </summary>
    [Serializable]
    public class BinaryData
    {
        /// <summary>
        /// Format for inline resource identifiers
        /// </summary>
        private const string ID_FORMAT = "x-growl-resource://{0}";

        /// <summary>
        /// Provides the mechanism to generate default IDs
        /// </summary>
        private static MD5 md5 = MD5CryptoServiceProvider.Create();
        
        /// <summary>
        /// The identifier of the data
        /// </summary>
        private string id;

        /// <summary>
        /// The actual data
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Creates a new instance of the <see cref="BinaryData"/> class
        /// and auto-generates a unique ID.
        /// </summary>
        /// <param name="data">Array of bytes that make up the data</param>
        public BinaryData(byte[] data) : this(GenerateID(data), data)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BinaryData"/> class
        /// and uses the supplied value for the unique ID.
        /// </summary>
        /// <param name="id">The value of the ID</param>
        /// <param name="data">Array of bytes that make up the data</param>
        /// <remarks>
        /// The ID provided should be unique for different binary data, but
        /// should be the same for duplicate versions of the same data (by
        /// generating the hash of the data, for example).
        /// </remarks>
        public BinaryData(string id, byte[] data)
        {
            this.id = id;
            this.data = data;
        }

        /// <summary>
        /// Gets the value of the unique ID associated with the data
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string ID
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// Gets the length of the actual data
        /// </summary>
        /// <value>
        /// int - number of bytes
        /// </value>
        public int Length
        {
            get
            {
                return (this.data != null ? this.data.Length : 0);
            }
        }

        /// <summary>
        /// The actual binary data
        /// </summary>
        /// <value>
        /// Array of bytes
        /// </value>
        public byte[] Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// The unique pointer associated with the data
        /// </summary>
        /// <remarks>
        /// When passed in a GNTP message, this is the value used
        /// for any headers that are pointing to binary data.
        /// </remarks>
        /// <value>
        /// string in the format:  x-growl-resource://id
        /// </value>
        public string IDPointer
        {
            get
            {
                return String.Format(ID_FORMAT, this.id);
            }
        }

        /// <summary>
        /// Generates a unique ID for the supplied binary data.
        /// </summary>
        /// <param name="data">Array of bytes</param>
        /// <remarks>
        /// This implementation generates the MD5 hash of the data and
        /// returns the hex-encoded hash.
        /// </remarks>
        /// <returns>string</returns>
        public static string GenerateID(byte[] data)
        {
            if (data != null)
            {
                byte[] hash = null;
                lock (md5)
                {
                    hash = md5.ComputeHash(data);
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("x2").ToLower());
                return sb.ToString();
            }
            return null;
        }

        /// <summary>
        /// Converts the value of a <see cref="BinaryData"/> object to a byte array
        /// </summary>
        /// <param name="data"><see cref="BinaryData"/></param>
        /// <returns>byte array</returns>
        static public implicit operator byte[](BinaryData data)
        {
            if (data != null) return data.Data;
            else return null;
        }
    }
}
