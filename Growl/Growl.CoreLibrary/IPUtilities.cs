using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Growl.CoreLibrary
{
    public class IPUtilities
    {
        private static object syncLock = new object();
        private static Dictionary<IPAddress, IPAddress> masks;

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
