////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Secret Labs LLC
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// This code is licensed under the Apache 2.0 license.
// THIS CODE IS EXPERIMENTAL, PRE-BETA SOFTWARE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace System.Net.Sockets
{
    public enum AddressFamily
    {
        //Unknown = -1,
        //Unspecified = 0,
        //Unix = 1,
        InterNetwork = 2
        //ImpLink = 3
        //Pup = 4,
        //Chaos = 5,
        //NS = 6,
        //Ipx = 6,
        //Iso = 7,
        //Osi = 7,
        //Ecma = 8,
        //DataKit = 9,
        //Ccitt = 10,
        //Sna = 11,
        //DecNet = 12,
        //DataLink = 13,
        //Lat = 14,
        //HyperChannel = 15,
        //AppleTalk = 16,
        //NetBios = 17,
        //VoiceView = 18,
        //FireFox = 19,
        //Banyan = 21,
        //Atm = 22,
        //InterNetworkV6 = 23,
        //Cluster = 24,
        //Ieee12844 = 25,
        //Irda = 26,
        //NetworkDesigners = 28,
        //Max = 29
    }

    public enum ProtocolType
    {
        //Unknown = -1,
        //Unspecified = 0,
        //IP = 0,
        //IPv6HopByHopOptions = 0,
        //Icmp = 1,
        //Igmp = 2,
        //Ggp = 3,
        //IPv4 = 4,
        Tcp = 6,
        //Pup = 12,
        Udp = 17,
        //Idp = 22,
        //IPv6 = 41,
        //IPv6RoutingHeader = 43,
        //IPv6FragmentHeader = 44,
        //IPSecEncapsulatingSecurityPayload = 50,
        //IPSecAuthenticationHeader = 51,
        //IcmpV6 = 58,
        //IPv6NoNextHeader = 59,
        //IPv6DestinationOptions = 60,
        //ND = 77,
        //Raw = 255,
        //Ipx = 1000,
        //Spx = 1256,
        //SpxII = 1257
    }

    public enum SelectMode
    {
        SelectRead = 0,
        //SelectWrite = 1,
        //SelectError = 2
    }

    public enum SocketError
    {
        SocketError = -1,
        Success = 0,
        //Interrupted = 10004,
        AccessDenied = 10013,
        //Fault = 10014,
        //InvalidArgument = 10022,
        TooManyOpenSockets = 10024,
        //WouldBlock = 10035,
        //InProgress = 10036,
        //AlreadyInProgress = 10037,
        //NotSocket = 10038,
        //DestinationAddressRequired = 10039,
        //MessageSize = 10040,
        //ProtocolType = 10041,
        //ProtocolOption = 10042,
        ProtocolNotSupported = 10043,
        SocketNotSupported = 10044,
        //OperationNotSupported = 10045,
        //ProtocolFamilyNotSupported = 10046,
        AddressFamilyNotSupported = 10047,
        //AddressAlreadyInUse = 10048,
        //AddressNotAvailable = 10049,
        //NetworkDown = 10050,
        //NetworkUnreachable = 10051,
        //NetworkReset = 10052,
        //ConnectionAborted = 10053,
        //ConnectionReset = 10054,
        //NoBufferSpaceAvailable = 10055,
        //IsConnected = 10056,
        NotConnected = 10057,
        //Shutdown = 10058,
        TimedOut = 10060,
        //ConnectionRefused = 10061,
        //HostDown = 10064,
        //HostUnreachable = 10065,
        //ProcessLimit = 10067,
        //SystemNotReady = 10091,
        //VersionNotSupported = 10092,
        NotInitialized = 10093,
        Disconnecting = 10101,
        //TypeNotFound = 10109,
        //HostNotFound = 11001,
        //TryAgain = 11002,
        //NoRecovery = 11003,
        //NoData = 11004,
    }

    public enum SocketFlags
    {
        None = 0,
        //OutOfBand = 1,
        //Peek = 2,
        //DontRoute = 4,
        //MaxIOVectorLength = 16,
        //Truncated = 256,
        //ControlDataTruncated = 512,
        //Broadcast = 1024,
        //Multicast = 2048,
        //Partial = 32768
    }

    public enum SocketOptionLevel
    {
        //IP = 0,
        //Tcp = 6,
        //Udp = 17,
        //IPv6 = 41,
        //Socket = 65535
    }

    public enum SocketOptionName
    {
        //DontLinger = -129,
        //ExclusiveAddressUse = -5,
        //Debug = 1,
        //IPOptions = 1,
        //NoChecksum = 1,
        //NoDelay = 1,
        //AcceptConnection = 2,
        //BsdUrgent = 2,
        //Expedited = 2,
        //HeaderIncluded = 2,
        //TypeOfService = 3,
        //IpTimeToLive = 4,
        //ReuseAddress = 4,
        //KeepAlive = 8,
        //MulticastInterface = 9,
        //MulticastTimeToLive = 10,
        //MulticastLoopback = 11,
        //AddMembership = 12,
        //DropMembership = 13,
        //DontFragment = 14,
        //AddSourceMembership = 15,
        //DontRoute = 16,
        //DropSourceMembership = 16,
        //BlockSource = 17,
        //UnblockSource = 18,
        //PacketInformation = 19,
        //ChecksumCoverage = 20,
        //HopLimit = 21,
        //Broadcast = 32,
        //UseLoopback = 64,
        //Linger = 128,
        //OutOfBandInline = 256,
        //SendBuffer = 4097,
        //ReceiveBuffer = 4098,
        //SendLowWater = 4099,
        //ReceiveLowWater = 4100,
        //SendTimeout = 4101,
        //ReceiveTimeout = 4102,
        //Error = 4103,
        //Type = 4104,
        //UpdateAcceptContext = 28683,
        //UpdateConnectContext = 28688,
        //MaxConnections = 2147483647
    }

    public enum SocketType
    {
        Unknown = -1,
        Stream = 1,
        Dgram = 2,
        //Raw = 3,
        //Rdm = 4,
        //Seqpacket = 5
    }

    public class SocketException : System.Exception
    {
        private int m_errorCode = (int)SocketError.Success;

        public SocketException(System.Net.Sockets.SocketError errorCode)
        {
            m_errorCode = (int)errorCode;
        }

        public int ErrorCode
        {
            get
            {
                return m_errorCode;
            }
        }
    }

    public class Socket : IDisposable 
    {
        private AddressFamily m_addressFamily;
        private SocketType m_socketType;
        internal ProtocolType m_protocolType;

        private EndPoint m_localEndPoint;
        private EndPoint m_remoteEndPoint = null;

        const byte m_UdpReceiveHeaderBufferLength = 8;
        byte[] m_UdpReceiveHeaderBuffer = new byte[m_UdpReceiveHeaderBufferLength];
        UInt16 m_UdpReceiveBufferDataSize = 0;
        object m_UdpReceiveBufferLock = new object();

        private SecretLabs.NETMF.Net.Wiznet5100 m_wiznetChip;

        private int m_wiznetSocketIndex = -1; // -1 = uninitialized; otherwise, this will be the socket index # (0-3: up to 4 supported) within the WiznetChip...

        private bool m_isListening = false;

        private bool m_isDisposed = false;

        public Socket(System.Net.Sockets.AddressFamily addressFamily, System.Net.Sockets.SocketType socketType, System.Net.Sockets.ProtocolType protocolType)
        {
            // verify that we support the requested type of socket
            switch (addressFamily)
            {
                case AddressFamily.InterNetwork:
                    break;
                default:
                    throw new SocketException(SocketError.AddressFamilyNotSupported);
            }
            switch (socketType)
            {
                case SocketType.Stream: // TCP
                case SocketType.Dgram:  // UPD
                    break;
                default:
                    throw new SocketException(SocketError.SocketNotSupported);
            }
            switch (protocolType)
            {
                case ProtocolType.Tcp:
                case ProtocolType.Udp:
                    break;
                default:
                    // TODO: verify that we should throw this exception and not SocketException.ProtocolType
                    throw new SocketException(SocketError.ProtocolNotSupported);
            }

            // store our setings
            m_addressFamily = addressFamily;
            m_socketType = socketType;
            m_protocolType = protocolType;

            // store a reference to our Wiznet chip...
            m_wiznetChip = ((Microsoft.SPOT.Net.NetworkInformation.W5100NetworkInterface)Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0]).WiznetChip;
            // verify that we are already connected to a Wiznet chip
            if (m_wiznetChip == null)
                throw new SocketException(SocketError.NotInitialized);

            // initialize other variables
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_isDisposed)
            {
                Close();

                m_isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal bool IsListening
        {
            get
            {
                return m_isListening;
            }
            set
            {
                m_isListening = value;
            }
        }

        public System.Net.Sockets.Socket Accept()
        {
            m_wiznetChip.AcceptIncomingSocketConnection(m_wiznetSocketIndex);
            return this;
        }

        public void Bind(System.Net.EndPoint localEP)
        {
            if (!Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress.Equals(((IPEndPoint)localEP).Address.ToString()))
                throw new System.ArgumentOutOfRangeException();
            
            m_wiznetSocketIndex = m_wiznetChip.CreateSocket(this, ((IPEndPoint)localEP).Port);
        }

        public void Close()
        {
            if (m_wiznetSocketIndex == -1)
                throw new SocketException(SocketError.NotConnected);

            switch (m_protocolType)
            {
                case ProtocolType.Tcp:
                    m_wiznetChip.DisconnectSocket(m_wiznetSocketIndex);
                    break;
                case ProtocolType.Udp:
                    m_wiznetChip.CloseSocket(m_wiznetSocketIndex);
                    break;
            }

            if (m_isListening)
            {
                // if we're a TCP server listening for connections, start listening again automatically.
                m_wiznetChip.StartListeningForSocketConnection(m_wiznetSocketIndex);
            }
            else
            {
                m_wiznetSocketIndex = -1; // we are disconnected
            }
        }
        
        public void Connect(System.Net.EndPoint remoteEP)
        {
             m_wiznetSocketIndex = m_wiznetChip.CreateAndConnectSocket(this, remoteEP);
             m_remoteEndPoint = remoteEP;
        }

        public void GetSocketOption(System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, byte[] val)
        {
            throw new NotImplementedException();
        }

        public object GetSocketOption(System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName)
        {
            throw new NotImplementedException();
        }

        public void Listen(int backlog)
        {
            if (m_wiznetSocketIndex == -1)
                throw new SocketException(SocketError.SocketError);

            if (m_isListening)
                throw new SocketException(SocketError.AccessDenied);

            // only one TCP server connection is supported per port
            if (backlog != 1)
                throw new ArgumentOutOfRangeException("backlog");

            m_wiznetChip.StartListeningForSocketConnection(m_wiznetSocketIndex);

        }

        public bool Poll(int microSeconds, System.Net.Sockets.SelectMode mode)
        {
            // no support for protocols other than TCP today
            if (m_protocolType != ProtocolType.Tcp)
                throw new NotImplementedException();

            switch (mode)
            {
                case SelectMode.SelectRead:
                    // wait until we have data (or are disconnected, etc.)
                    return m_wiznetChip.WaitForReceivedData(m_wiznetSocketIndex, microSeconds % 1000 != 0 ? microSeconds / 1000 : (microSeconds / 1000) + 1);
                default:
                    // polling for other modes not yet supported
                    throw new NotImplementedException();
            }
        }

        public int Receive(byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags)
        {
            if (socketFlags != SocketFlags.None)
                throw new NotSupportedException("socketFlags");

            switch (m_protocolType)
            {
                case ProtocolType.Tcp:
                    return m_wiznetChip.Receive(m_wiznetSocketIndex, buffer, offset, size);
                case ProtocolType.Udp:
                    return ReceiveFrom(buffer, offset, size, socketFlags, ref m_remoteEndPoint);
                default:
                    return 0;
            }
        }

        public int Receive(byte[] buffer)
        {
            return Receive(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public int Receive(byte[] buffer, System.Net.Sockets.SocketFlags socketFlags)
        {
            return Receive(buffer, 0, buffer.Length, socketFlags);
        }

        public int Receive(byte[] buffer, int size, System.Net.Sockets.SocketFlags socketFlags)
        {
            return Receive(buffer, 0, size, socketFlags);
        }

        public int ReceiveFrom(byte[] buffer, ref System.Net.EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, System.Net.Sockets.SocketFlags socketFlags, ref System.Net.EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer.Length, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, int size, System.Net.Sockets.SocketFlags socketFlags, ref System.Net.EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, ref System.Net.EndPoint remoteEP)
        {
            if (socketFlags != SocketFlags.None)
                throw new NotSupportedException("socketFlags");

            // if we do not yet have a connection, create one now...
            if (m_wiznetSocketIndex == -1)
                m_wiznetSocketIndex = m_wiznetChip.CreateSocket(this);

            lock (m_UdpReceiveBufferLock)
            {
                while (true)
                {
                    int bytesToRead = m_wiznetChip.GetBytesToRead(m_wiznetSocketIndex);
                    if (bytesToRead < m_UdpReceiveHeaderBufferLength)
                        return 0; // if we did not have at least the 8 byte header, return with nothing.

                    // get our UDP receive header
                    m_wiznetChip.Receive(m_wiznetSocketIndex, m_UdpReceiveHeaderBuffer, 0, m_UdpReceiveHeaderBufferLength);
                    // validate that we have received all the data from this datagram
                    m_UdpReceiveBufferDataSize = (UInt16)((m_UdpReceiveHeaderBuffer[6] * 0x100) + m_UdpReceiveHeaderBuffer[7]);
                    if (bytesToRead < m_UdpReceiveHeaderBufferLength + m_UdpReceiveBufferDataSize)
                        return 0; // if we have not received the entire datagram, return with nothing.

                    // validate that this data is being received from our target device
                    //byte[] remoteEndPointAddressBytes = ((IPEndPoint)remoteEP).Address.GetAddressBytes();
                    //int remoteEndPointPort = ((IPEndPoint)m_remoteEndPoint).Port;
                    //if (m_UdpReceiveHeaderBuffer[0] != remoteEndPointAddressBytes[0] ||
                    //    m_UdpReceiveHeaderBuffer[1] != remoteEndPointAddressBytes[1] ||
                    //    m_UdpReceiveHeaderBuffer[2] != remoteEndPointAddressBytes[2] ||
                    //    m_UdpReceiveHeaderBuffer[3] != remoteEndPointAddressBytes[3] ||
                    //    m_UdpReceiveHeaderBuffer[4] != (byte)(remoteEndPointPort >> 8) ||
                    //    m_UdpReceiveHeaderBuffer[5] != (byte)(remoteEndPointPort & 0xFF))
                    //{
                    //    // flush this data so that we can get data we want
                    //    m_wiznetChip.Receive(m_wiznetSocketIndex, buffer, offset, m_UdpReceiveBufferDataSize);
                    //    continue;
                    //}

                    byte[] addr = { m_UdpReceiveHeaderBuffer[0], m_UdpReceiveHeaderBuffer[1], m_UdpReceiveHeaderBuffer[2], m_UdpReceiveHeaderBuffer[3] };
                    byte[] port = { m_UdpReceiveHeaderBuffer[5], m_UdpReceiveHeaderBuffer[4] };
                    remoteEP = new IPEndPoint(new IPAddress(addr), (ushort) Utility.ExtractValueFromArray(port, 0, 2));

                    // the destination IP address and port match our 'connection'
                    if (size >= m_UdpReceiveBufferDataSize)
                    {
                        return m_wiznetChip.Receive(m_wiznetSocketIndex, buffer, offset, m_UdpReceiveBufferDataSize);
                    }
                    else
                    {
                        // the user did not pass in an array large enough to receive the entire datagram
                        throw new ArgumentOutOfRangeException("size");
                    }
                }
            }
        }

        public int Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public int Send(byte[] buffer, System.Net.Sockets.SocketFlags socketFlags)
        {
            return Send(buffer, 0, buffer.Length, socketFlags);
        }

        public int Send(byte[] buffer, int size, System.Net.Sockets.SocketFlags socketFlags)
        {
            return Send(buffer, 0, size, socketFlags);
        }

        public int Send(byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags)
        {
            if (socketFlags != SocketFlags.None)
                throw new NotSupportedException("socketFlags");

            return m_wiznetChip.Send(m_wiznetSocketIndex, buffer, offset, size);
        }

        public int SendTo(byte[] buffer, System.Net.EndPoint remoteEP)
        {
            return SendTo(buffer, 0, buffer.Length, SocketFlags.None, remoteEP);
        }

        public int SendTo(byte[] buffer, System.Net.Sockets.SocketFlags socketFlags, System.Net.EndPoint remoteEP)
        {
            return SendTo(buffer, 0, buffer.Length, socketFlags, remoteEP);
        }

        public int SendTo(byte[] buffer, int size, System.Net.Sockets.SocketFlags socketFlags, System.Net.EndPoint remoteEP)
        {
            return SendTo(buffer, 0, size, socketFlags, remoteEP);
        }

        public int SendTo(byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, System.Net.EndPoint remoteEP)
        {
            if (socketFlags != SocketFlags.None)
                throw new NotSupportedException("socketFlags");

            // if we do not yet have a connection, create one now...
            if (m_wiznetSocketIndex == -1)
                m_wiznetSocketIndex = m_wiznetChip.CreateSocket(this);

            m_wiznetChip.SetDestinationIPEndPoint(m_wiznetSocketIndex, (System.Net.IPEndPoint)remoteEP);
            return Send(buffer, offset, size, socketFlags);
        }

        public void SetSocketOption(System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, byte[] optionValue)
        {
            throw new NotImplementedException();
        }

        public void SetSocketOption(System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, bool optionValue)
        {
            throw new NotImplementedException();
        }

        public void SetSocketOption(System.Net.Sockets.SocketOptionLevel optionLevel, System.Net.Sockets.SocketOptionName optionName, int optionValue)
        {
            throw new NotImplementedException();
        }

        public int Available
        {
            get
            {
                // if we do not yet have a connection, return 0 bytes...
                if (m_wiznetSocketIndex == -1)
                    return 0;

                switch (m_protocolType)
                {
                    case ProtocolType.Tcp:
                        return m_wiznetChip.GetBytesToRead(m_wiznetSocketIndex);
                    case ProtocolType.Udp:
                        lock (m_UdpReceiveBufferLock)
                        {
                            while (true)
                            {
                                int bytesToRead = m_wiznetChip.GetBytesToRead(m_wiznetSocketIndex);
                                if (bytesToRead < m_UdpReceiveHeaderBufferLength)
                                    return 0; // if we did not have at least the 8 byte header, return with nothing.

                                // peek at our UDP receive header
                                m_wiznetChip.Peek(m_wiznetSocketIndex, m_UdpReceiveHeaderBuffer, 0, m_UdpReceiveHeaderBufferLength);
                                // validate that we have received all the data from this datagram
                                m_UdpReceiveBufferDataSize = (UInt16)((m_UdpReceiveHeaderBuffer[6] * 0x100) + m_UdpReceiveHeaderBuffer[7]);
                                if (bytesToRead < m_UdpReceiveHeaderBufferLength + m_UdpReceiveBufferDataSize)
                                    return 0; // if we have not received the entire datagram, we effectively have no data available.

                                // validate that this data is being received from our target device
                                //byte[] remoteEndPointAddressBytes = ((IPEndPoint)m_remoteEndPoint).Address.GetAddressBytes();
                                //int remoteEndPointPort = ((IPEndPoint)m_remoteEndPoint).Port;
                                //if (m_UdpReceiveHeaderBuffer[0] != remoteEndPointAddressBytes[0] ||
                                //    m_UdpReceiveHeaderBuffer[1] != remoteEndPointAddressBytes[1] ||
                                //    m_UdpReceiveHeaderBuffer[2] != remoteEndPointAddressBytes[2] ||
                                //    m_UdpReceiveHeaderBuffer[3] != remoteEndPointAddressBytes[3] ||
                                //    m_UdpReceiveHeaderBuffer[4] != (byte)(remoteEndPointPort >> 8) ||
                                //    m_UdpReceiveHeaderBuffer[5] != (byte)(remoteEndPointPort & 0xFF))
                                //{
                                //    // flush this data so that we can get data we want
                                //    byte[] flushBuffer = new byte[m_UdpReceiveBufferDataSize + m_UdpReceiveHeaderBufferLength];
                                //    m_wiznetChip.Receive(m_wiznetSocketIndex, flushBuffer, 0, m_UdpReceiveBufferDataSize + m_UdpReceiveHeaderBufferLength);
                                //    continue;
                                //}

                                // we have good data...
                                return m_UdpReceiveBufferDataSize;
                            }
                        }
                    default:
                        return 0;
                }
            }
        }

        public System.Net.EndPoint LocalEndPoint
        {
            get
            {
                return m_localEndPoint;
            }
            internal set
            {
                m_localEndPoint = value;
            }
        }

        public int ReceiveTimeout
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

        public System.Net.EndPoint RemoteEndPoint
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int SendTimeout
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

    public class NetworkStream : System.IO.Stream
    {
        protected bool _disposed = false;
        protected System.Net.EndPoint _remoteEndPoint = null;
        protected int _socketType = (int)SocketType.Unknown;

        public void Close(int timeout)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public NetworkStream(System.Net.Sockets.Socket socket, bool ownsSocket)
        {
            throw new NotImplementedException();
        }

        public NetworkStream(System.Net.Sockets.Socket socket)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual bool DataAvailable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
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

        public override int ReadTimeout
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

        public override int WriteTimeout
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
