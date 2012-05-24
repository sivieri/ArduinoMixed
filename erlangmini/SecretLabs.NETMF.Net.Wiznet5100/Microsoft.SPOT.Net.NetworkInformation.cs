////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Secret Labs LLC
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// This code is licensed under the Apache 2.0 license.
// THIS CODE IS EXPERIMENTAL, PRE-BETA SOFTWARE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using Microsoft.SPOT;

namespace Microsoft.SPOT.Net.NetworkInformation
{
    public delegate void NetworkAddressChangedEventHandler(object sender, Microsoft.SPOT.EventArgs e);
        
    public delegate void NetworkAvailabilityChangedEventHandler(object sender, Microsoft.SPOT.Net.NetworkInformation.NetworkAvailabilityEventArgs e);
    public class NetworkAvailabilityEventArgs : Microsoft.SPOT.EventArgs
    {
        private bool m_isAvailable;

        private NetworkAvailabilityEventArgs(bool isAvailable)
        {
            m_isAvailable = isAvailable;
        }

        public bool IsAvailable 
        {
            get
            {
                return m_isAvailable;
            }
        }
    }

    public static class NetworkChange
    {
        public static event Microsoft.SPOT.Net.NetworkInformation.NetworkAddressChangedEventHandler NetworkAddressChanged;
        public static event Microsoft.SPOT.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged;
    }

    public class NetworkInterface
    {
        static private NetworkInterface[] m_networkInterfaces = null;

        private Int32 m_interfaceIndex = 0;
        private NetworkInterfaceType m_networkInterfaceType = NetworkInterfaceType.Ethernet;
        protected IPAddress m_ipAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });
        protected IPAddress m_subnetMask = new IPAddress(new byte[] { 0, 0, 0, 0 });
        protected IPAddress m_gatewayAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });
        protected IPAddress[] m_dnsAddresses = new IPAddress[] { };
        protected byte[] m_physicalAddress = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        static NetworkInterface()
        {
            // we have one network interface: our WIZnet shield
            m_networkInterfaces = new NetworkInterface[] {
                new W5100NetworkInterface(0)
                };

            m_networkInterfaces[0].NetworkInterfaceType = NetworkInterfaceType.Ethernet;

            // TODO: possibly read in our physicalAddress and static IP settings from the configuration sector
            m_networkInterfaces[0].EnableStaticIP("0.0.0.0", "0.0.0.0", "0.0.0.0"); // temporarily use "all zeros"
            // TODO: possibly read in our DNS Settings from the configuration sector
            m_networkInterfaces[0].EnableStaticDns(new string[] { "0.0.0.0", "0.0.0.0" }); // temporarily read in our static DNS settings
        }

        protected NetworkInterface(Int32 interfaceIndex)
        {
            m_interfaceIndex = interfaceIndex;
        }

        public void EnableDhcp()
        {
            throw new NotSupportedException();
        }

        public void EnableDynamicDns()
        {
            throw new NotSupportedException();
        }

        public void EnableStaticDns(string[] dnsAddresses)
        {
            m_dnsAddresses = new IPAddress[dnsAddresses.Length];
            for (int i = 0; i < dnsAddresses.Length; i++)
            {
                m_dnsAddresses[i] = System.Net.IPAddress.Parse(dnsAddresses[i]);
            }
        }

        public virtual void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress)
        {
            throw new NotImplementedException();
        }

        public static Microsoft.SPOT.Net.NetworkInformation.NetworkInterface[] GetAllNetworkInterfaces()
        {
            return m_networkInterfaces;
        }

        public void ReleaseDhcpLease()
        {
            throw new NotSupportedException();
        }

        public void RenewDhcpLease()
        {
            throw new NotSupportedException();
        }

        public string[] DnsAddresses 
        {
            get
            {
                string[] dnsAddresses = new string[m_dnsAddresses.Length];
                for (int i = 0; i < m_dnsAddresses.Length; i++)
                {
                    dnsAddresses[i] = m_dnsAddresses[i].ToString();
                }
                return dnsAddresses;
            }
        }

        public string GatewayAddress
        {
            get
            {
                return m_gatewayAddress.ToString();
            }
        }

        public string IPAddress
        {
            get
            {
                return m_ipAddress.ToString();
            }
        }

        public bool IsDhcpEnabled
        {
            get
            {
                return false;
            }
        }

        public bool IsDynamicDnsEnabled
        {
            get
            {
                return false;
            }
        }

        public Microsoft.SPOT.Net.NetworkInformation.NetworkInterfaceType NetworkInterfaceType
        {
            protected set
            {
                m_networkInterfaceType = value;
            }

            get
            {
                return m_networkInterfaceType;
            }
        }

        public virtual byte[] PhysicalAddress
        {
            set
            {
                if (value.Length != 6)
                    throw new ArgumentException();

                m_physicalAddress = value;
            }

            get
            {
                return m_physicalAddress;
            }
        }

        public string SubnetMask
        {
            get
            {
                return m_subnetMask.ToString();
            }
        }
    }

    internal class W5100NetworkInterface : NetworkInterface
    {
        private SecretLabs.NETMF.Net.Wiznet5100 m_wiznetChip = null;

        public W5100NetworkInterface(Int32 interfaceIndex) 
            : base(interfaceIndex)
        {
        }

        public SecretLabs.NETMF.Net.Wiznet5100 WiznetChip
        {
            set
            {
                m_wiznetChip = value;
            }

            get
            {
                return m_wiznetChip;
            }
        }

        public override void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress)
        {
            // save the static IP settings
            m_ipAddress = System.Net.IPAddress.Parse(ipAddress);
            m_subnetMask = System.Net.IPAddress.Parse(subnetMask);
            m_gatewayAddress = System.Net.IPAddress.Parse(gatewayAddress);

            // if our wiznet chip is already initialized, change its static ip
            if (m_wiznetChip != null)
                m_wiznetChip.EnableStaticIP(ipAddress, subnetMask, gatewayAddress);
        }

        public override byte[] PhysicalAddress
        {
            set
            {
                // save our new physical address
                base.PhysicalAddress = value;

                // if our wiznet chip is already initialized, change its static ip
                if (m_wiznetChip != null)
                    m_wiznetChip.SetPhysicalAddress(value);
            }

            get
            {
                return base.PhysicalAddress;
            }
        }
    }

    public enum NetworkInterfaceType
    {
        Unknown = 1,
        Ethernet = 6
        //Wireless80211 = 71
    }
}
