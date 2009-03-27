using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// Represents the results of an encryption operation
    /// </summary>
    public struct EncryptionResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EncryptionResult"/> class
        /// </summary>
        /// <param name="encryptedBytes">The encrypted bytes</param>
        /// <param name="iv">The IV used</param>
        public EncryptionResult(byte[] encryptedBytes, byte[] iv)
        {
            this.EncryptedBytes = encryptedBytes;
            this.IV = iv;
        }

        /// <summary>
        /// The encrypted bytes
        /// </summary>
        /// <value>
        /// Array of bytes
        /// </value>
        public byte[] EncryptedBytes;

        /// <summary>
        /// The initialization vector used to do the encryption
        /// </summary>
        /// <value>
        /// Array of bytes
        /// </value>
        public byte[] IV;
    }
}
