using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Growl.CoreLibrary;

namespace Growl.Connector
{
    /// <summary>
    /// Contains a list of passwords and their associated keys, used to authorize incoming requests
    /// </summary>
    [Serializable]
    public class PasswordManager
    {
        /// <summary>
        /// A list of valid passwords
        /// </summary>
        private Dictionary<string, Password> passwords;

        /// <summary>
        /// Creates a new instance of the PasswordManager class
        /// </summary>
        public PasswordManager()
        {
            this.passwords = new Dictionary<string, Password>();
        }

        /// <summary>
        /// Gets the list of valid passwords
        /// </summary>
        public Dictionary<string, Password> Passwords
        {
            get
            {
                return this.passwords;
            }
        }

        /// <summary>
        /// Adds a password to the list of valid passwords
        /// </summary>
        /// <param name="password">The new password</param>
        /// <param name="permanent">Indicates if the password is permanent (user-specified) vs. temporary (automatically added by a subscription)</param>
        public void Add(string password, bool permanent)
        {
            if(!String.IsNullOrEmpty(password) && !this.passwords.ContainsKey(password))
                Add(new Password(password, null, permanent));
        }

        /// <summary>
        /// Adds a password to the list of valid passwords
        /// </summary>
        /// <param name="password">The <see cref="Password"/> to add</param>
        public void Add(Password password)
        {
            this.passwords.Add(password.ActualPassword, password);
        }

        /// <summary>
        /// Removes the specified password from the list of valid passwords.
        /// </summary>
        /// <param name="password">The password to remove.</param>
        public void Remove(string password)
        {
            if (!String.IsNullOrEmpty(password) && this.passwords.ContainsKey(password))
                this.passwords.Remove(password);
        }

        /// <summary>
        /// Checks the supplied <paramref name="keyHash"/> against all of the stored passwords to 
        /// see if the hash is valid.
        /// </summary>
        /// <param name="keyHash">The hex-encoded hash to validate</param>
        /// <param name="salt">The hex-encoded salt value</param>
        /// <param name="hashAlgorithm">The <see cref="Cryptography.HashAlgorithmType"/> used to generate the hash</param>
        /// <returns>
        /// <c>true</c> if the hash matches one of the stored password/key values;
        /// <c>false</c> if no match is found
        /// </returns>
        public bool IsValid(string keyHash, string salt, Cryptography.HashAlgorithmType hashAlgorithm)
        {
            // the key returned here will not actually be valid, but will indicate a match
            // (this is because the EncryptionAlgorithm will not be set properly)
            Key key;
            return IsValid(keyHash, salt, hashAlgorithm, Cryptography.SymmetricAlgorithmType.PlainText, out key);
        }

        /// <summary>
        /// Checks the supplied <paramref name="keyHash"/> against all of the stored passwords to 
        /// see if the hash is valid, and retuns the matching <see cref="Key"/> if a match is found.
        /// </summary>
        /// <param name="keyHash">The hex-encoded hash to validate</param>
        /// <param name="salt">The hex-encoded salt value</param>
        /// <param name="hashAlgorithm">The <see cref="Cryptography.HashAlgorithmType"/> used to generate the hash</param>
        /// <param name="encryptionAlgorithm">The <see cref="Cryptography.SymmetricAlgorithmType"/> used by this key to do encryption/decryption</param>
        /// <param name="matchingKey">Contains the matching <see cref="Key"/> if a match is found</param>
        /// <returns>
        /// <c>true</c> if the hash matches one of the stored password/key values;
        /// <c>false</c> if no match is found
        /// If no match is found, <paramref name="matchingKey"/> will return <c>null</c>.
        /// </returns>
        public bool IsValid(string keyHash, string salt, Cryptography.HashAlgorithmType hashAlgorithm, Cryptography.SymmetricAlgorithmType encryptionAlgorithm, out Key matchingKey)
        {
            matchingKey = null;

            if (String.IsNullOrEmpty(keyHash)) return false;

            keyHash = keyHash.ToUpper();
            foreach (Password password in this.passwords.Values)
            {
                bool match = Key.Compare(password.ActualPassword, keyHash, salt, hashAlgorithm, encryptionAlgorithm, out matchingKey);
                if (match)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
