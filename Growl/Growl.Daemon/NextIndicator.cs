using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Daemon
{
    /// <summary>
    /// Represents information about what is expected next when the <see cref="GNTPParser"/> is
    /// parsing a request. It specifies either an absolute amount of bytes expected (length) or
    /// a set of indicator bytes to look for.
    /// </summary>
    class NextIndicator
    {
        /// <summary>
        /// No more data is expected.
        /// </summary>
        public static NextIndicator None = new NextIndicator(0);

        /// <summary>
        /// The next bit of data should be read until a CRLF is encountered.
        /// </summary>
        public static NextIndicator CRLF = new NextIndicator(AsyncSocket.CRLFData);

        /// <summary>
        /// The next bit of data should be read until two CRLFs in a row are encountered.
        /// </summary>
        public static NextIndicator CRLFCRLF = new NextIndicator(AsyncSocket.CRLFCRLFData);

        /// <summary>
        /// The indicator bytes
        /// </summary>
        byte[] bytes;

        /// <summary>
        /// The amount of data expected
        /// </summary>
        int length;


        /// <summary>
        /// Initializes a new instance of the <see cref="NextIndicator"/> class.
        /// </summary>
        /// <param name="bytes">The indicator bytes to look for.</param>
        public NextIndicator(byte[] bytes)
        {
            this.bytes = bytes;
            this.length = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NextIndicator"/> class.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        public NextIndicator(int length)
        {
            this.bytes = null;
            this.length = length;
        }

        /// <summary>
        /// Gets the indicator bytes to look for.
        /// </summary>
        /// <value>
        /// If <see cref="UseBytes"/> is <c>true</c>, this contains the indicator bytes to look for;
        /// otherwise, it returns null.
        /// </value>
        public byte[] Bytes
        {
            get
            {
                return this.bytes;
            }
        }

        /// <summary>
        /// Gets the number of bytes to read.
        /// </summary>
        /// <value>
        /// If <see cref="UseLength"/> is <c>true</c>, this contains the number of bytes to be read;
        /// otherwise, it returns 0.
        /// </value>
        public int Length
        {
            get
            {
                return this.length;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to use the indicator bytes or not.
        /// </summary>
        /// <value><c>true</c> if the indicator bytes should be used; otherwise, <c>false</c>.</value>
        public bool UseBytes
        {
            get
            {
                if (this.bytes != null)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to use the number of bytes or not.
        /// </summary>
        /// <value><c>true</c> if the number of bytes should be used; otherwise, <c>false</c>.</value>
        public bool UseLength
        {
            get
            {
                if(!this.UseBytes && this.Length > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the reader should continue reading data or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if more data is expected and the reader should continue; 
        /// <c>false</c> if no more data is expected (valid response) or if an error condition was encountered
        /// </value>
        public bool ShouldContinue
        {
            get
            {
                return (UseBytes || UseLength);
            }
        }
    }
}
