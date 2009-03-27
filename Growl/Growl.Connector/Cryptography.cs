using System;
using System.Security.Cryptography;
using System.Text;

namespace Growl.Connector
{
    /// <summary>
    /// Provides utilities for encrypting and decrypting data as well
    /// as computing hash values.
    /// </summary>
    public sealed class Cryptography
    {
        # region member variables & constants

        /// <summary>
        /// chart of all hex characters
        /// </summary>
        private const string hexChart = "0123456789ABCDEF";

        /// <summary>
        /// Random number generator
        /// </summary>
        private static RandomNumberGenerator rng = RNGCryptoServiceProvider.Create();

        # endregion member variables & constants

        # region constructors

        /// <summary>
        /// Since this class provides only static methods, the default constructor is
        /// private to prevent instances from being created with "new Cryptography()".
        /// </summary>
        private Cryptography() { }

        # endregion constructors

        # region Public Methods

        /// <summary>
        /// Hashes the supplied input using using the MD5 hashing algorithm
        /// </summary>
        /// <remarks>
        /// Unlike many methods, this method does NOT trim leading or trailing spaces from
        /// <paramref name="inputString"/>.
        /// </remarks>
        /// <param name="inputString">The string to hash</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="inputString" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="string">The hex-encoded MD5-hashed value</returns>
        public static string ComputeHash(string inputString)
        {
            // parameter checking
            if (inputString == null)
                throw new ArgumentNullException("inputString", "ComputeHash: 'inputString' parameter cannot be null");
            // NOTE: it is ok for inputString to be empty

            try
            {
                // hash it
                string hash = ComputeHash(inputString, HashAlgorithmType.MD5);
                return hash;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("ComputeHash: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Hashes the supplied input using using the specified hashing algorithm
        /// </summary>
        /// <remarks>
        /// Unlike many methods, this method does NOT trim leading or trailing spaces from
        /// <paramref name="inputString"/>.
        /// </remarks>
        /// <param name="inputString">The string to hash</param>
        /// <param name="hashAlgorithmType">The <see cref="HashAlgorithmType"/> to use to hash the input string</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="inputString" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="string">The hex-encoded hashed value</returns>
        public static string ComputeHash(string inputString, HashAlgorithmType hashAlgorithmType)
        {
            // parameter checking
            if (inputString == null)
                throw new ArgumentNullException("inputString", "ComputeHash: 'inputString' parameter cannot be null");
            // NOTE: it is ok for inputString to be empty

            try
            {
                // hash it
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(inputString);
                byte[] hashedBytes = ComputeHash(inputBytes, hashAlgorithmType);
                string hash = HexEncode(hashedBytes);
                return hash;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("ComputeHash: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Hashes the supplied input using using the specified hashing algorithm.
        /// </summary>
        /// <remarks>
        /// Unlike many methods, this method does NOT trim leading or trailing spaces from
        /// <paramref name="inputString"/>.
        /// </remarks>
        /// <param name="inputBytes">The byte array to hash</param>
        /// <param name="hashAlgorithmType">The <see cref="HashAlgorithmType"/> to use to hash the input string</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="inputBytes" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="byte">The hashed byte array</returns>
        public static byte[] ComputeHash(byte[] inputBytes, HashAlgorithmType hashAlgorithmType)
        {
            // parameter checking
            if (inputBytes == null)
                throw new ArgumentNullException("inputBytes", "ComputeHash: 'inputBytes' parameter cannot be null");

            try
            {
                // determine the hashing algorithm to use
                HashAlgorithm hash;
                switch (hashAlgorithmType)
                {
                    case HashAlgorithmType.SHA1:
                        hash = new SHA1Managed();
                        break;
                    case HashAlgorithmType.SHA256:
                        hash = new SHA256Managed();
                        break;
                    case HashAlgorithmType.SHA384:
                        hash = new SHA384Managed();
                        break;
                    case HashAlgorithmType.SHA512:
                        hash = new SHA512Managed();
                        break;
                    default:
                        hash = new MD5CryptoServiceProvider();
                        break;
                }

                // hash it
                byte[] hashedBytes = hash.ComputeHash(inputBytes);
                return hashedBytes;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("ComputeHash: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Encrypts the supplied input using using the default encryption algorithm (AES).
        /// </summary>
        /// <param name="key">The key used to encrypt the data</param>
        /// <param name="inputBytes">The bytes to encrypt</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="inputBytes" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="byte">array of encrypted bytes</returns>
        public static EncryptionResult Encrypt(byte[] key, byte[] inputBytes)
        {
            return Encrypt(key, inputBytes, SymmetricAlgorithmType.AES);
        }

        /// <summary>
        /// Encrypts the supplied input using using the specified encryption algorithm.
        /// </summary>
        /// <param name="key">The key used to encrypt the data</param>
        /// <param name="inputBytes">The bytes to encrypt</param>
        /// <param name="algorithmType">The <see cref="SymmetricAlgorithmType"/> to use to encrypt the input string</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="inputBytes" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="byte">array of encrypted bytes</returns>
        public static EncryptionResult Encrypt(byte[] key, byte[] inputBytes, SymmetricAlgorithmType algorithmType)
        {
            byte[] iv = null;
            return Encrypt(key, inputBytes, algorithmType, ref iv);
        }

        /// <summary>
        /// Encrypts the supplied input using using the specified encryption algorithm.
        /// </summary>
        /// <param name="key">The key used to encrypt the data</param>
        /// <param name="inputBytes">The bytes to encrypt</param>
        /// <param name="algorithmType">The <see cref="SymmetricAlgorithmType"/> to use to encrypt the input string</param>
        /// <param name="iv">The initialization vector to use (<c>null</c> to auto-generate the value)</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="inputBytes" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="byte">array of encrypted bytes</returns>
        public static EncryptionResult Encrypt(byte[] key, byte[] inputBytes, SymmetricAlgorithmType algorithmType, ref byte[] iv)
        {
            // parameter checking
            if (inputBytes == null)
                throw new ArgumentNullException("inputBytes", "Encrypt: 'inputBytes' parameter cannot be null");

            try
            {
                EncryptionResult result = new EncryptionResult();

                // determine the encryption algorithm to use
                int keySize;
                int ivSize;
                SymmetricAlgorithm algorithm;
                switch (algorithmType)
                {
                    case SymmetricAlgorithmType.RC2:
                        algorithm = new RC2CryptoServiceProvider();
                        keySize = 8;
                        ivSize = 8;
                        break;
                    case SymmetricAlgorithmType.DES:
                        algorithm = new DESCryptoServiceProvider();
                        keySize = 8;
                        ivSize = 8;
                        break;
                    case SymmetricAlgorithmType.TripleDES:
                        algorithm = new TripleDESCryptoServiceProvider();
                        keySize = 16;
                        ivSize = 8;
                        break;
                    case SymmetricAlgorithmType.AES:
                        algorithm = new RijndaelManaged();
                        keySize = 24;
                        ivSize = 16;
                        break;
                    default:
                        result.EncryptedBytes = inputBytes;
                        return result; // return the bytes unmodified
                }

                // handle key
                if(key.Length < keySize)
                    throw new CryptographicException(String.Format("Encrypt: Algorithm '{0}' requires an minimum key size of {1} - (you supplied a key that was {2} long)", algorithmType, keySize, key.Length));
                algorithm.Key = GetKeyFromSize(key, keySize);

                // handle IV
                if (iv == null)
                    algorithm.GenerateIV();
                else
                    algorithm.IV = iv;
                if(algorithm.IV.Length != ivSize)
                    throw new CryptographicException(String.Format("Encrypt: Algorithm '{0}' requires an IV size of {1} - (you supplied an IV that was {2} long)", algorithmType, ivSize, algorithm.IV.Length));
                result.IV = algorithm.IV;

                // encrypt the input string
                ICryptoTransform encryptor = algorithm.CreateEncryptor();
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                result.EncryptedBytes = encryptedBytes;
                return result;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("Encrypt: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Decrypts the supplied input using using the default decryption algorithm.
        /// </summary>
        /// <param name="key">The key used to encrypt the data</param>
        /// <param name="iv">The initialization vector used during encryption</param>
        /// <param name="encryptedBytes">The bytes to decrypt</param>
        /// <returns>Array of decrypted bytes</returns>
        public static byte[] Decrypt(byte[] key, byte[] iv, byte[] encryptedBytes)
        {
            try
            {
                return Decrypt(key, iv, encryptedBytes, SymmetricAlgorithmType.AES);
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("Decrypt: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Decrypts the supplied input using using the specified decryption algorithm.
        /// </summary>
        /// <param name="key">The key used to encrypt the data</param>
        /// <param name="iv">The initialization vector used during encryption</param>
        /// <param name="encryptedBytes">The bytes to decrypt</param>
        /// <param name="algorithmType">The <see cref="SymmetricAlgorithmType"/> to use to decrypt the input string</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="encryptedBytes" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="string">The decrypted value as clear text</returns>
        public static byte[] Decrypt(byte[] key, byte[] iv, byte[] encryptedBytes, SymmetricAlgorithmType algorithmType)
        {
            // parameter checking
            if (encryptedBytes == null)
                throw new ArgumentNullException("encryptedBytes", "Decrypt: 'encryptedBytes' parameter cannot be null");

            try
            {
                // determine the decryption algorithm to use and set the key and IV size
                int keySize;
                int ivSize;
                int blockSize;
                SymmetricAlgorithm algorithm;
                switch (algorithmType)
                {
                    case SymmetricAlgorithmType.RC2:
                        algorithm = new RC2CryptoServiceProvider();
                        keySize = 8;
                        ivSize = 8;
                        blockSize = algorithm.BlockSize;
                        break;
                    case SymmetricAlgorithmType.DES:
                        algorithm = new DESCryptoServiceProvider();
                        keySize = 8;
                        ivSize = 8;
                        blockSize = algorithm.BlockSize;
                        break;
                    case SymmetricAlgorithmType.TripleDES:
                        algorithm = new TripleDESCryptoServiceProvider();
                        keySize = 16;
                        ivSize = 8;
                        blockSize = algorithm.BlockSize;
                        break;
                    case SymmetricAlgorithmType.AES:
                        algorithm = new RijndaelManaged();
                        keySize = 24;
                        ivSize = 16;
                        blockSize = 128; // AES only supports 128-bit blocks
                        break;
                    default:
                        return encryptedBytes; // return bytes unchanged
                }

                algorithm.BlockSize = blockSize;

                // handle key
                if (key.Length < keySize)
                    throw new CryptographicException(String.Format("Decrypt: Algorithm '{0}' requires an minimum key size of {1} - (you supplied a key that was {2} long)", algorithmType, keySize, key.Length));
                algorithm.Key = GetKeyFromSize(key, keySize);

                // handle IV
                if (iv.Length != ivSize)
                    throw new CryptographicException(String.Format("Decrypt: Algorithm '{0}' requires an IV size of {1} - (you supplied an IV that was {2} long)", algorithmType, ivSize, iv.Length));
                algorithm.IV = iv;

                algorithm.Padding = PaddingMode.None;   //TODO: agree on a padding scheme
                algorithm.Mode = CipherMode.CBC;

                // decrypt
                ICryptoTransform decryptor = algorithm.CreateDecryptor();
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return decryptedBytes;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("Decrypt: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Generates a random array of bytes of the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="length">The number of bytes to generate</param>
        /// <returns>Array of bytes</returns>
        public static byte[] GenerateBytes(int length)
        {
            byte[] bytes = new byte[length];
            rng.GetNonZeroBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// Encodes the supplied byte array into a string of hex characters
        /// </summary>
        /// <param name="bytes">The array of <see cref="byte">bytes</see> to encode</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="bytes" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="string">The hex-encoded string</returns>
        public static string HexEncode(byte[] bytes)
        {
            // parameter checking
            if (bytes == null)
                throw new ArgumentNullException("bytes", "HexEncode: 'bytes' parameter cannot be null");
            // NOTE: it is ok for bytes to have zero items

            try
            {
                // see keith for implementation questions
                int size = bytes.Length * 2;
                StringBuilder sb = new StringBuilder(size, size);
                int b1;
                int b2;
                int b3;
                for (int i = 0; i < bytes.Length; i++)
                {
                    b1 = bytes[i];
                    b2 = b1 & 0x0f;
                    b3 = b1 >> 4;
                    sb.Append(hexChart[b3]);
                    sb.Append(hexChart[b2]);
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("HexEncode: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Unencodes the supplied hex string into an array of bytes
        /// </summary>
        /// <param name="hexString">The hex string to unencode</param>
        /// <exception cref="ArgumentNullException">Returned when <paramref name="hexString" /> is null</exception>
        /// <exception cref="CryptographicException">Returned when any other exception occurs</exception>
        /// <returns cref="byte">Array of bytes</returns>
        public static byte[] HexUnencode(string hexString)
        {
            // parameter checking
            if (hexString == null)
                throw new ArgumentNullException("hexString", "HexUnencode: 'hexString' parameter cannot be null");
            // NOTE: it is ok for hexString to be empty

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                byte.TryParse(hexString.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out bytes[i]);
            }
            return bytes;
        }

        # endregion Public Methods

        # region Private Methods

        /// <summary>
        /// Returns a key of <paramref name="keySize"/> length based on the input <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The original key</param>
        /// <param name="keySize">The length of the key to return</param>
        /// <returns>Array of bytes</returns>
        private static byte[] GetKeyFromSize(byte[] key, int keySize)
        {
            int start = 0;
            int end = start + keySize;

            // parameter checking
            if (end > key.Length)
                throw new ArgumentOutOfRangeException("keySize", String.Format("GetKeyFromSize: The requested key size is longer than the supplied key. Key size: {0}, key length: {1}", keySize, key.Length));

            try
            {
                byte[] newKey = new byte[keySize];
                Array.Copy(key, newKey, keySize);
                return newKey;
            }
            catch (Exception ex)
            {
                throw new CryptographicException(String.Format("GetKeyFromSize: {0} - {1}", ex.GetType().Name, ex.Message));
            }
        }

        # endregion Private Methods

        # region enumerations

        /// <summary>
        /// Enumeration of HashAlgorithmTypes
        /// </summary>
        public enum HashAlgorithmType
        {
            /// <summary>
            /// MD5 hash algorithm (128-bit)
            /// </summary>
            [DisplayName("MD5")]
            MD5 = 128,
            /// <summary>
            /// SHA algorithm (160-bit)
            /// </summary>
            [DisplayName("SHA1")]
            SHA1 = 160,
            /// <summary>
            /// SHA algorithm (256-bit)
            /// </summary>
            [DisplayName("SHA256")]
            SHA256 = 256,
            /// <summary>
            /// SHA algorithm (384-bit)
            /// </summary>
            [DisplayName("SHA384")]
            SHA384 = 384,
            /// <summary>
            /// SHA algorithm (512-bit)
            /// </summary>
            [DisplayName("SHA512")]
            SHA512 = 512
        }

        /// <summary>
        /// Enumeration of SymmetricAlgorithmTypes
        /// </summary>
        public enum SymmetricAlgorithmType
        {
            /// <summary>
            /// No encryption
            /// </summary>
            [DisplayName("NONE")]
            PlainText,
            /// <summary>
            /// RC2 Encryption (64-bit key, 64-bit IV)
            /// </summary>
            [DisplayName("RC2")]
            RC2,
            /// <summary>
            /// DES Encryption (64-bit key, 64-bit IV)
            /// </summary>
            [DisplayName("DES")]
            DES,
            /// <summary>
            /// TripleDES Encryption (128-bit key, 64-bit IV)
            /// </summary>
            [DisplayName("3DES")]
            TripleDES,
            /// <summary>
            /// AES Encryption (192-bit key, 128-bit IV, 128-bit block size)
            /// </summary>
            [DisplayName("AES")]
            AES
        }

        # endregion enumerations
    }
}
