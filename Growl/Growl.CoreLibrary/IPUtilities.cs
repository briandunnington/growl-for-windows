using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// Provides helper methods for common IP-address related functionality.
    /// </summary>
    public class IPUtilities
    {
        /// <summary>
        /// Provides a lock while gathering network adapter information from the system.
        /// </summary>
        private static object syncLock = new object();

        /// <summary>
        /// Loads a list of subnet masks for each IP address on the machine (IPv4 only)
        /// </summary>
        private static Dictionary<IPAddress, IPAddress> masks;


        /// <summary>
        /// Determines whether <paramref name="otherAddress"/> is in the same subnet as <paramref name="localAddress"/>.
        /// </summary>
        /// <param name="localAddress">The local address to compare to.</param>
        /// <param name="otherAddress">The other address being compared.</param>
        /// <returns>
        /// 	<c>true</c> if both addresses are in the same subnet; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If the otherAddress is the loopback address, then this method always returns true.
        /// The subnet comparison is done for IPv4 addresses. For IPv6 addresses, this method returns
        /// <c>true</c> if the address is a LinkLocal or SiteLocal address.
        /// </remarks>
        public static bool IsInSameSubnet(IPAddress localAddress, IPAddress otherAddress)
        {
            try
            {
                // handle loopback addresses and IPv6 local addresses
                if (IPAddress.IsLoopback(otherAddress)
                    || otherAddress.IsIPv6LinkLocal
                    || otherAddress.IsIPv6SiteLocal)
                    return true;

                IPAddress subnetMask = GetLocalSubnetMask(localAddress);
                IPAddress network1 = GetNetworkAddress(localAddress, subnetMask);
                IPAddress network2 = GetNetworkAddress(otherAddress, subnetMask);
                return network1.Equals(network2);
            }
            catch
            {
                DebugInfo.WriteLine(String.Format("Could not determine subnet. Local address: {0} - Remote Address: {1}", localAddress, otherAddress));
            }
            return false;
        }

        /// <summary>
        /// Gets the local subnet mask for the given IP address.
        /// </summary>
        /// <param name="ipaddress">The ipaddress.</param>
        /// <returns>
        /// <see cref="IPAddress"/> of the subnet mask of the IP address, or <see cref="IPAddress.None"/>
        /// if the subnet cannot be determined.
        /// </returns>
        public static IPAddress GetLocalSubnetMask(IPAddress ipaddress)
        {
            lock (syncLock)
            {
                if (masks == null)
                {
                    masks = new Dictionary<IPAddress, IPAddress>();

                    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface ni in interfaces)
                    {
                        //Console.WriteLine(ni.Description);

                        UnicastIPAddressInformationCollection unicastAddresses = ni.GetIPProperties().UnicastAddresses;
                        foreach (UnicastIPAddressInformation unicastAddress in unicastAddresses)
                        {
                            IPAddress mask = (unicastAddress.IPv4Mask != null ? unicastAddress.IPv4Mask : IPAddress.None);
                            masks.Add(unicastAddress.Address, mask);

                            //Console.WriteLine("\tIP Address is {0}", unicastAddress.Address);
                            //Console.WriteLine("\tSubnet Mask is {0}", mask);
                        }
                    }
                }
            }

            if (masks.ContainsKey(ipaddress))
                return masks[ipaddress];
            else
                return IPAddress.None;
        }

        /// <summary>
        /// Gets the network broadcast address based on an IP address and subnet mask.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <param name="subnetMask">The subnet mask.</param>
        /// <returns>
        /// The broadcast <see cref="IPAddress"/>
        /// </returns>
        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }
    }
}
