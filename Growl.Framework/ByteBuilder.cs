using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Framework
{
    /// <summary>
    /// Helper class for converting basic data types to a byte array.
    /// This class is similar to the <see cref="StringBuilder"/> class.
    /// </summary>
    internal class ByteBuilder
    {
        protected List<byte> byteList = new List<byte>();
        protected bool isLittleEndian = BitConverter.IsLittleEndian;

        /// <summary>
        /// Appends a single byte to the array
        /// </summary>
        /// <param name="val">The byte to append</param>
        public void Append(byte val)
        {
            this.byteList.Add(val);
        }

        /// <summary>
        /// Appends an array of bytes to the array
        /// </summary>
        /// <param name="val">The bytes to append</param>
        public void Append(byte[] val)
        {
            this.byteList.AddRange(val);
        }

        /// <summary>
        /// Appends a string as an array of UTF8-encoded bytes
        /// </summary>
        /// <param name="val">The string to append</param>
        public void Append(string val)
        {
            if (val != null)
                this.byteList.AddRange(Encoding.UTF8.GetBytes(val));
        }

        /// <summary>
        /// Appends an <see cref="int"/> as a 2-byte array using Big Endian encoding
        /// </summary>
        /// <param name="val">The number to append</param>
        public void Append(int val)
        {
            Append((short)val);
        }

        /// <summary>
        /// Appends a <see cref="short"/> as a 2-byte array using Big Endian encoding
        /// </summary>
        /// <param name="val">The number to append</param>
        public void Append(short val)
        {
            byte[] b = BitConverter.GetBytes(val);
            if(this.isLittleEndian) Array.Reverse(b);
            this.byteList.AddRange(b);
        }

        /// <summary>
        /// Returns all of the bytes as an array
        /// </summary>
        /// <returns>Array of <see cref="byte"/>s</returns>
        public byte[] GetBytes()
        {
            return this.byteList.ToArray();
        }

        /// <summary>
        /// Returns the number of bytes making up the string
        /// </summary>
        /// <param name="val">The string whose length you want</param>
        /// <returns>The number of bytes making up the string (NOT the number of characters, since some characters will require 2 bytes)</returns>
        public static int GetStringLength(string val)
        {
            return Encoding.UTF8.GetBytes(val).Length;
        }
    }
}
