using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;

namespace Growl.Daemon
{
    /// <summary>
    /// Represents a <see cref="Key"/> that can be used by a subscribing client when communicating with Growl.
    /// </summary>
    public class SubscriberKey : Key
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberKey"/> class.
        /// </summary>
        /// <param name="key">The <see cref="Key"/> to base this key upon.</param>
        /// <param name="subscriberID">The unique subscriber ID</param>
        /// <param name="hashAlgorithm">The <see cref="Cryptography.HashAlgorithmType"/> used when hashing values</param>
        /// <param name="encryptionAlgorithm">The <see cref="Cryptography.SymmetricAlgorithmType"/> used when encrypting values</param>
        public SubscriberKey(Key key, string subscriberID, Cryptography.HashAlgorithmType hashAlgorithm, Cryptography.SymmetricAlgorithmType encryptionAlgorithm)
            : base(key.Password + subscriberID, hashAlgorithm, encryptionAlgorithm)
        {
        }
    }
}
