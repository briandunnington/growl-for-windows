using System;
using System.Collections.Generic;
using System.Text;

namespace Growl.Framework
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
        /// Checks to see if a given packets data matches the password
        /// provided by the receiving client.
        /// </summary>
        /// <param name="bytes">The packets data</param>
        /// <param name="password">The receiving client's password</param>
        /// <returns><c>true</c> if the password matches, <c>false</c> otherwise</returns>
        protected static bool IsPasswordValid(byte[] bytes, string password)
        {
            byte[] packetWithoutPassword = new byte[bytes.Length - 16];
            Array.Copy(bytes, 0, packetWithoutPassword, 0, packetWithoutPassword.Length);
            byte[] thatChecksum = new byte[16];
            Array.Copy(bytes, bytes.Length - 16, thatChecksum, 0, 16);
            string thatChecksumString = Encoding.UTF8.GetString(thatChecksum);

            ByteBuilder pb = new ByteBuilder();
            pb.Append(password);
            ByteBuilder bpb = new ByteBuilder();
            bpb.Append(packetWithoutPassword);
            bpb.Append(pb.GetBytes());
            byte[] thisChecksum = Utility.MD5(bpb.GetBytes());
            string thisChecksumString = Encoding.UTF8.GetString(thisChecksum);

            if (thisChecksumString == thatChecksumString)
                return true;
            else
                return false;
        }
    }
}
