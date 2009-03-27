using System;
using System.Collections.Generic;
using System.Text;
using Growl.Connector;

namespace Growl.UDPLegacy
{
    /// <summary>
    /// Represents the information sent in a UDP packet as specified by the Growl protocol.
    /// </summary>
    /// <remarks>
    /// This is an abstract base class that cannot be instantiated directly.
    /// </remarks>
    public abstract class BasePacket
    {
        /// <summary>
        /// The <see cref="PacketType">type</see> of packet
        /// </summary>
        protected PacketType packetType;
        /// <summary>
        /// Indicates the protocol version.
        /// </summary>
        protected int protocolVersion;
        /// <summary>
        /// The name of the application sending the packet
        /// </summary>
        protected string applicationName;
        /// <summary>
        /// The password used to validate the receiving client
        /// </summary>
        protected string password;
        /// <summary>
        /// The entire packet's data, encoded as a byte array.
        /// </summary>
        protected byte[] data;

        /// <summary>
        /// The <see cref="PacketType">type</see> of packet
        /// </summary>
        public PacketType PacketType
        {
            get
            {
                return this.packetType;
            }
        }

        /// <summary>
        /// Indicates the protocol version.
        /// </summary>
        /// <remarks>The only currently supported version is 1.</remarks>
        public int ProtocolVersion
        {
            get
            {
                return this.protocolVersion;
            }
        }

        /// <summary>
        /// The name of the application sending the packet
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
        }

        /// <summary>
        /// The entire packet's data, encoded as a byte array.
        /// </summary>
        public virtual byte[] Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// The password used to validate the receiving client
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }
        }

        /// <summary>
        /// Checks to see if a given packets data matches a password
        /// provided by the receiving client and returns the matching password if found.
        /// </summary>
        /// <param name="bytes">The packets data</param>
        /// <param name="passwordManager">The receiving client's list of passwords</param>
        /// <param name="isLocal">Indicates if the request came from the local machine</param>
        /// <param name="requireLocalPassword">Indicates if local requests must supply a valid password</param>
        /// <param name="password">Returns the matching password if found; null otherwise</param>
        /// <returns><c>true</c> if the password matches, <c>false</c> otherwise</returns>
        protected static bool IsPasswordValid(byte[] bytes, PasswordManager passwordManager, bool isLocal, bool requireLocalPassword, out string password)
        {
            password = null;

            byte[] packetWithoutPassword = new byte[bytes.Length - 16];
            Array.Copy(bytes, 0, packetWithoutPassword, 0, packetWithoutPassword.Length);
            byte[] thatChecksum = new byte[16];
            Array.Copy(bytes, bytes.Length - 16, thatChecksum, 0, 16);
            string thatChecksumString = Encoding.UTF8.GetString(thatChecksum);

            ByteBuilder bpb = new ByteBuilder();
            bpb.Append(packetWithoutPassword);

            Queue<string> validPasswords = new Queue<string>();
            foreach (KeyValuePair<string, Password> item in passwordManager.Passwords)
            {
                validPasswords.Enqueue(item.Key);
            }
            // if the request is from the local machine, then
            // we can also try no (blank) password if allowed
            if (isLocal && !requireLocalPassword) validPasswords.Enqueue(String.Empty);

            while(validPasswords.Count > 0)
            {
                string p = validPasswords.Dequeue();
                ByteBuilder pb = new ByteBuilder();
                pb.Append(bpb.GetBytes());
                pb.Append(p);
                byte[] thisChecksum = Cryptography.ComputeHash(pb.GetBytes(), Cryptography.HashAlgorithmType.MD5);
                string thisChecksumString = Encoding.UTF8.GetString(thisChecksum);

                if (thisChecksumString == thatChecksumString)
                {
                    password = p;
                    return true;
                }
            }

            return false;
        }
    }
}
