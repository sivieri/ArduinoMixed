////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Secret Labs LLC
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// This code is licensed under the Apache 2.0 license.
// THIS CODE IS EXPERIMENTAL, PRE-BETA SOFTWARE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;

namespace System.Net
{
    public abstract class EndPoint
    {
        public abstract System.Net.EndPoint Create(System.Net.SocketAddress socketAddress);
        public abstract SocketAddress Serialize();
    }

    public class IPAddress
    {
        private byte[] m_addressBytes = new byte[4];

        public static IPAddress Any
        {
            get
            {
                return new IPAddress(new byte[] {255, 255, 255, 255});
            }
        }

        public static IPAddress Loopback
        {
            get
            {
                return new IPAddress(new byte[] {127, 0, 0, 1});
            }
        }

        public IPAddress(byte[] newAddressBytes)
        {
            if (newAddressBytes.Length != 4)
                throw new ArgumentException("newAddressBytes");

            Array.Copy(newAddressBytes, m_addressBytes, 4);
        }

        public IPAddress(Int64 newAddress)
        {
            m_addressBytes[0] = (byte)((newAddress & 0xFF000000) >> 24);
            m_addressBytes[1] = (byte)((newAddress & 0xFF0000) >> 16);
            m_addressBytes[2] = (byte)((newAddress & 0xFF00) >> 8);
            m_addressBytes[3] = (byte)(newAddress & 0xFF);
        }

        public byte[] GetAddressBytes()
        {
            return m_addressBytes;
        }

        public static System.Net.IPAddress Parse(string ipString)
        {
            byte[] address = new byte[4];
            int startIndex = 0;

            for (int i = 0; i < 4; i++)
            {
                if (ipString.IndexOf('.', startIndex) > 0)
                {
                    address[i] = byte.Parse(ipString.Substring(startIndex, ipString.IndexOf('.', startIndex) - startIndex));
                    startIndex = ipString.IndexOf('.', startIndex) + 1;
                }
                else
                {
                    address[i] = byte.Parse(ipString.Substring(startIndex));
                }
            }

            return new IPAddress(address);
        }

        public override string ToString()
        {
            return m_addressBytes[0].ToString() + "." + m_addressBytes[1].ToString() + "." + m_addressBytes[2].ToString() + "." + m_addressBytes[3].ToString();
        }
    }

    public class IPEndPoint : EndPoint
    {
        public const int MaxPort = 65535;
        public const int MinPort = 0;

        private IPAddress m_address;
        private Int32 m_port;

        public IPEndPoint(Int64 address, Int32 port)
            : this(new IPAddress(address), port)
        {
            // all code is done in the alternate constructor (see this constructor's definition)
        }

        public IPEndPoint(IPAddress address, Int32 port)
        {
            m_address = address;
            m_port = port;
        }

        public IPAddress Address
        {
            get
            {
                return m_address;
            }
        }

        public override EndPoint Create(SocketAddress socketAddress)
        {
            throw new NotImplementedException();
        }

        public Int32 Port
        {
            get
            {
                return m_port;
            }
        }

        public override SocketAddress Serialize()
        {
            throw new NotImplementedException();
        }
    }

    public class IPHostEntry
    {
        IPAddress[] m_addressList = new IPAddress[] {};
        string m_hostName = string.Empty;

        public IPHostEntry()
        {
//            throw new NotImplementedException();
        }

        public IPAddress[] AddressList
        {
            get
            {
                return m_addressList;
            }
            internal set
            {
                m_addressList = value;
            }
        }

        public string HostName
        {
            get
            {
                return m_hostName;
            }
            internal set
            {
                m_hostName = value;
            }
        }
    }

    public class SocketAddress
    {
        public SocketAddress(Sockets.AddressFamily family, Int32 size)
        {
            throw new NotImplementedException();
        }

        public Sockets.AddressFamily Family
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Int32 Size
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public byte this[Int32 offset]
        {
            set
            {
                throw new NotImplementedException();
            }

            get
            {
                throw new NotImplementedException();
            }
        }
    }
}