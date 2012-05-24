////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Secret Labs LLC
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// This code is licensed under the Apache 2.0 license.
// THIS CODE IS EXPERIMENTAL, PRE-BETA SOFTWARE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SecretLabs.NETMF.Net
{
    public class Wiznet5100
    {
        private SPI m_spi = null;
        private InterruptPort m_interruptPort = null;
        private object m_interruptPortLock = new object();
        private byte[] m_writeCommand = new byte[4];
        private byte[] m_readCommand = new byte[4];
        private byte[] m_readResponse = new byte[1];

        private IPAddress m_ipAddress = IPAddress.Loopback;

        private Socket[] m_Sockets = new Socket[4];
        private Object m_SocketsLock = new object();

        const UInt16 SOCKET_RX_BUFFER_SIZE = 0x800;
        const UInt16 SOCKET_RX_BUFFER_MASK = SOCKET_RX_BUFFER_SIZE - 1;
        const UInt16 SOCKET_TX_BUFFER_SIZE = 0x800;
        const UInt16 SOCKET_TX_BUFFER_MASK = SOCKET_TX_BUFFER_SIZE - 1;

        private struct SocketConfig
        {
            public AutoResetEvent AnyInterrupt;
            public AutoResetEvent ConnectedInterrupt;
            public AutoResetEvent DisconnectedInterrupt;
            public AutoResetEvent DataReceivedInterrupt;
            public AutoResetEvent TimeoutInterrupt;
            public AutoResetEvent DataWrittenInterrupt;
            public object TransmitBufferLock;
            public object ReceiveBufferLock;
        }
        private SocketConfig[] m_SocketConfig = new SocketConfig[4];

        private Random m_randomGenerator = new Random();

        public Wiznet5100(SPI.SPI_module spiModule, Cpu.Pin chipSelect) : this(spiModule, chipSelect, Cpu.Pin.GPIO_NONE)
        {
        }

        public Wiznet5100(SPI.SPI_module spiModule, Cpu.Pin chipSelect, Cpu.Pin interrupt)
        {
            // initialize our write/read commands with their respective first bytes
            m_writeCommand[0] = 0xF0; // 0xF0 = write data byte
            m_readCommand[0] = 0x0F; // 0x0F = read data byte
            m_readCommand[3] = 0x00; // 0x00 = dummy filler data (to be transmitted while reading byte in ReadRegister)

            // setup our SPI connection parameters

            // TODO: we are using default assumptions here; validate hold and setup times and clock rates
            //m_spi = new SPI(new SPI.Configuration(chipSelect, false, 1, 1, false, true, 100, spiModule)); // start at 100KHz
            //m_spi = new SPI(new SPI.Configuration(Pins.GPIO_PIN_D10, false, 1, 1, false, true, 10000, SPI.SPI_module.SPI1)); // then go to 10MHz
            //m_spi = new SPI(new SPI.Configuration(chipSelect, false, 1, 1, false, true, 15000, spiModule)); // then go to 15MHz
            m_spi = new SPI(new SPI.Configuration(chipSelect, false, 0, 0, false, true, 15000, spiModule)); // then get rid of setup/release times
            //m_spi = new SPI(new SPI.Configuration(Pins.GPIO_PIN_D10, false, 0, 0, false, true, 100, SPI.SPI_module.SPI1)); // as a failsafe, get rid of 1ms setup and 1ms release times and 100KHz

            // set up our socket config data...
            for (int i = 0; i < 4; i++)
            {
                m_SocketConfig[i] = new SocketConfig();
                m_SocketConfig[i].AnyInterrupt = new AutoResetEvent(false);
                m_SocketConfig[i].ConnectedInterrupt = new AutoResetEvent(false);
                m_SocketConfig[i].DisconnectedInterrupt = new AutoResetEvent(false);
                m_SocketConfig[i].DataReceivedInterrupt = new AutoResetEvent(false);
                m_SocketConfig[i].TimeoutInterrupt = new AutoResetEvent(false);
                m_SocketConfig[i].DataWrittenInterrupt = new AutoResetEvent(false);
                m_SocketConfig[i].TransmitBufferLock = new object();
                m_SocketConfig[i].ReceiveBufferLock = new object();
            }
            // if an interrupt pin was specified, connect to it now...
            if (interrupt != Cpu.Pin.GPIO_NONE)
            {
                m_interruptPort = new InterruptPort(interrupt, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
                m_interruptPort.OnInterrupt += new NativeEventHandler(m_interruptPort_OnInterrupt);
            }

            // get our Wiznet network interface reference
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()[0];
            
            // initialize our Wiznet chip using the settings from our NetworkInterface
            Init(networkInterface.PhysicalAddress, IPAddress.Parse(networkInterface.IPAddress), IPAddress.Parse(networkInterface.SubnetMask), IPAddress.Parse(networkInterface.GatewayAddress));

            // configure out network interface with this WIZnet chip.
            ((Microsoft.SPOT.Net.NetworkInformation.W5100NetworkInterface)Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0]).WiznetChip = this;
        }

        private void Init(byte[] physicalAddress, IPAddress ipAddress, IPAddress subnetMask, IPAddress gatewayAddress)
        {
            // configure basic settings
            // configure mode register (MR) -- just reset the module
            WriteRegister(0x0000, 0x80);
            // wait for module to reset
            while ((ReadRegister(0x0000) & 0x80) != 0);

            // configure retry time-value register (RTR)
            WriteRegister(0x0017, 0x0F); // 400ms MSB
            WriteRegister(0x0018, 0xA0); // 400ms LSB

            // configure retry count register (RCR)
            WriteRegister(0x0019, 0x04); // 4 retries

            // configure network settings
            SetPhysicalAddress(physicalAddress);
            EnableStaticIP(ipAddress.ToString(), subnetMask.ToString(), gatewayAddress.ToString());

            // configure socket memory allocation
            // configure TX/RX buffers as 2KB per socket -- a good default
            WriteRegister(0x001A, 0x55); // each socket: RX buffer of 2KB
            WriteRegister(0x001B, 0x55); // each socket: TX buffer of 2KB            

            // if an interrupt pin was specified, get it ready for interrupts now...
            if (m_interruptPort != null)
            {
                // configure socket interrupt enable
                // interrupt mask register (IMR)
                // TODO: consider adding interrupts for "ip conflict" and "udp destination unreachable"
                WriteRegister(0x0016, 0x00); // enable no interrupts in the beginning...
                // interrupt register (IR)
                WriteRegister(0x0015, 0x00); // clear our interrupt register -- just to make sure the interrupt line starts high
            }
            else
            {
                // interrupt mask register (IMR)
                WriteRegister(0x0016, 0x00);
            }
        }

        void m_interruptPort_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            lock (m_interruptPortLock)
            {
                // interrupt register (IR)
                byte interruptRegister = ReadRegister(0x0015);

                for (int i = 0; i < 4; i++)
                {
                    if ((interruptRegister & (1 << i)) != 0)
                    {
                        // this socket has an interrupt...
                        m_SocketConfig[i].AnyInterrupt.Set();
                        // check this socket's interrupt...
                        // socket n interrupt register (Sn_IR)
                        byte socketInterruptRegister = ReadRegister((UInt16)(0x400 + (0x100 * i) + 0x0002));
                        // clear the selected bits in the socket interrupt register
                        WriteRegister((UInt16)(0x400 + (0x100 * i) + 0x0002), socketInterruptRegister);
                        // set the appropriate interrupt flags (waithandles) for this socket
                        if ((socketInterruptRegister & 0x01) != 0x00)
                        {
                            // connection established interrupt
                            m_SocketConfig[i].ConnectedInterrupt.Set();
                        }
                        if ((socketInterruptRegister & 0x02) != 0x00)
                        {
                            // disconnection interrupt
                            m_SocketConfig[i].DisconnectedInterrupt.Set();
                        }
                        if ((socketInterruptRegister & 0x04) != 0x00)
                        {
                            // data received interrupt
                            m_SocketConfig[i].DataReceivedInterrupt.Set();
                        }
                        if ((socketInterruptRegister & 0x08) != 0x00)
                        {
                            // timeout interrupt
                            m_SocketConfig[i].TimeoutInterrupt.Set();
                        }
                        if ((socketInterruptRegister & 0x10) != 0x00)
                        {
                            // data written interrupt
                            m_SocketConfig[i].DataWrittenInterrupt.Set();
                        }
                    }
                }

                // clear interrupt (to reset the state of the /INT line)
                // interrupt register (IR)
                WriteRegister(0x0015, 0x00);
            }
        }

        private void WriteRegister(UInt16 address, byte data)
        {
            m_writeCommand[1] = (byte)((address & 0xFF00) >> 8);
            m_writeCommand[2] = (byte)(address & 0xFF);
            m_writeCommand[3] = data;

            m_spi.Write(m_writeCommand);
        }

        private byte ReadRegister(UInt16 address)
        {
            m_readCommand[1] = (byte)((address & 0xFF00) >> 8);
            m_readCommand[2] = (byte)(address & 0xFF);

            m_spi.WriteRead(m_readCommand, m_readResponse, 3);

            return m_readResponse[0];
        }

        internal void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress)
        {
            // configure source ip address register (SIPR)
            byte[] ipAddressBytes = IPAddress.Parse(ipAddress).GetAddressBytes();
            WriteRegister(0x000F, ipAddressBytes[0]);
            WriteRegister(0x0010, ipAddressBytes[1]);
            WriteRegister(0x0011, ipAddressBytes[2]);
            WriteRegister(0x0012, ipAddressBytes[3]);

            // configure subnet mask register (SMR)
            byte[] subnetMaskBytes = IPAddress.Parse(subnetMask).GetAddressBytes();
            WriteRegister(0x0005, subnetMaskBytes[0]);
            WriteRegister(0x0006, subnetMaskBytes[1]);
            WriteRegister(0x0007, subnetMaskBytes[2]);
            WriteRegister(0x0008, subnetMaskBytes[3]);

            // configure gateway address register (GAR)
            byte[] gatewayAddressBytes = IPAddress.Parse(gatewayAddress).GetAddressBytes();
            WriteRegister(0x0001, gatewayAddressBytes[0]);
            WriteRegister(0x0002, gatewayAddressBytes[1]);
            WriteRegister(0x0003, gatewayAddressBytes[2]);
            WriteRegister(0x0004, gatewayAddressBytes[3]);

            // store our new values
            m_ipAddress = IPAddress.Parse(ipAddress);
        }

        internal void SetPhysicalAddress(byte[] physicalAddress)
        {
            // source hardware address register (SHAR) -- MAC address
            WriteRegister(0x0009, physicalAddress[0]);
            WriteRegister(0x000A, physicalAddress[1]);
            WriteRegister(0x000B, physicalAddress[2]);
            WriteRegister(0x000C, physicalAddress[3]);
            WriteRegister(0x000D, physicalAddress[4]);
            WriteRegister(0x000E, physicalAddress[5]);
        }

        internal int CreateAndConnectSocket(System.Net.Sockets.Socket socket, EndPoint remoteEP)
        {
            // initialize the socket
            int socketIndex = CreateSocket(socket);

            // if we could initialize the socket, connect.
            if (socketIndex != -1)
            {
                try
                {
                    ConnectSocket(socketIndex, remoteEP);
                }
                catch (SocketException ex)
                {
                    switch (ex.ErrorCode)
                    {
                        case (int)SocketError.TimedOut:
                        case (int)SocketError.Disconnecting:
                            // our socket connection failed; clean it up...
                            lock (m_SocketsLock)
                            {
                                m_Sockets[socketIndex] = null;
                            }
                            break;
                    }
                    throw ex;
                }
            }

            return socketIndex;
        }

        internal void SetDestinationIPEndPoint(int socketIndex, IPEndPoint remoteEP)
        {
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            // store our destination IP endpoint
            // socket n destination IP address register (Sn_DIPR)
            byte[] remoteIPAddressBytes = ((IPEndPoint)remoteEP).Address.GetAddressBytes();
            WriteRegister((UInt16)(socketBaseAddress + 0x000C), remoteIPAddressBytes[0]);
            WriteRegister((UInt16)(socketBaseAddress + 0x000D), remoteIPAddressBytes[1]);
            WriteRegister((UInt16)(socketBaseAddress + 0x000E), remoteIPAddressBytes[2]);
            WriteRegister((UInt16)(socketBaseAddress + 0x000F), remoteIPAddressBytes[3]);
            // socket n destination port register (Sn_DPORT)
            WriteRegister((UInt16)(socketBaseAddress + 0x0010), (byte)((((IPEndPoint)remoteEP).Port & 0xFF00) >> 8));
            WriteRegister((UInt16)(socketBaseAddress + 0x0011), (byte)(((IPEndPoint)remoteEP).Port & 0xFF));
        }

        internal void StartListeningForSocketConnection(int socketIndex)
        {
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            switch (m_Sockets[socketIndex].m_protocolType)
            {
                case ProtocolType.Tcp:
                    // issue command to start listening
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x02); // 0x02 = LISTEN
                    while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command

                    m_Sockets[socketIndex].IsListening = true;

                    break;
                case ProtocolType.Udp:
                default:
                    throw new NotSupportedException();
            }
        }

        internal void AcceptIncomingSocketConnection(int socketIndex)
        {
            if (!m_Sockets[socketIndex].IsListening)
                throw new SocketException(SocketError.SocketError);

            byte socketStatus;

            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            switch (m_Sockets[socketIndex].m_protocolType)
            {
                case ProtocolType.Tcp:
                    // wait for connect to complete/fail
                    // wait for open (initialize) to complete
                    if (m_interruptPort != null)
                        // TODO: should we implement some sort of timeout here, or generally on any waithandle/status waits?  The user may have unplugged the Wiznet module or it may not be responding...
                        WaitHandle.WaitAny(new WaitHandle[] { m_SocketConfig[socketIndex].ConnectedInterrupt, m_SocketConfig[socketIndex].TimeoutInterrupt }); // wait for connection/timeout
                    do
                    {
                        // socket n status register (Sn_SR)
                        socketStatus = ReadRegister((UInt16)(socketBaseAddress + 0x0003));
                    } while (socketStatus != 0x17 && socketStatus != 0x00 && socketStatus != 0x1C); // 0x17 == SOCK_ESTABLISHED; 0x00 = CLOSED; 0x1C = SOCK_CLOSE_WAIT
                    if (socketStatus == 0x00)
                    {
                        // timeout...our connection failed; clean up our socket reservation
                        throw new SocketException(SocketError.TimedOut);
                    }
                    else if (socketStatus == 0x1C)
                    {
                        // the remote target disconnected; clean up our socket reservation
                        // socket n command register (Sn_CR)
                        WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x10); // 0x10 = CLOSE
                        throw new SocketException(SocketError.Disconnecting);
                    }

                    // if we get here, our socket connection has succeeded!
                    break;
                case ProtocolType.Udp:
                default:
                    throw new NotSupportedException();
            }
        }

        internal void ConnectSocket(int socketIndex, EndPoint remoteEP)
        {
            byte socketStatus;
            
            // now, try to open our socket...  If we cannot open the socket, we must unreserve the socket from our available pool of sockets.
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            // store our destination IP endpoint
            SetDestinationIPEndPoint(socketIndex, (IPEndPoint)remoteEP);

            //// clear the destination hardware address (this may not be strictly necessary)
            //// socket n destination hardware address register (Sn_DHAR)
            //WriteRegister((UInt16)(socketBaseAddress + 0x0006), 0x00);
            //WriteRegister((UInt16)(socketBaseAddress + 0x0007), 0x00);
            //WriteRegister((UInt16)(socketBaseAddress + 0x0008), 0x00);
            //WriteRegister((UInt16)(socketBaseAddress + 0x0009), 0x00);
            //WriteRegister((UInt16)(socketBaseAddress + 0x000A), 0x00);
            //WriteRegister((UInt16)(socketBaseAddress + 0x000B), 0x00);

            switch (m_Sockets[socketIndex].m_protocolType)
            {
                case ProtocolType.Tcp:
                    // issue command to connect to target
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x04); // 0x04 = CONNECT
                    while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command

                    // wait for connect to complete/fail
                    // wait for open (initialize) to complete
                    if (m_interruptPort != null)
                        // TODO: should we implement some sort of timeout here, or generally on any waithandle/status waits?  The user may have unplugged the Wiznet module or it may not be responding...
                        WaitHandle.WaitAny(new WaitHandle[] { m_SocketConfig[socketIndex].ConnectedInterrupt, m_SocketConfig[socketIndex].TimeoutInterrupt }); // wait for connection/timeout
                    do
                    {
                        // socket n status register (Sn_SR)
                        socketStatus = ReadRegister((UInt16)(socketBaseAddress + 0x0003));
                    } while (socketStatus != 0x17 && socketStatus != 0x00 && socketStatus != 0x1C); // 0x17 == SOCK_ESTABLISHED; 0x00 = CLOSED; 0x1C = SOCK_CLOSE_WAIT
                    if (socketStatus == 0x00)
                    {
                        // timeout...our connection failed; clean up our socket reservation
                        throw new SocketException(SocketError.TimedOut);
                    }
                    else if (socketStatus == 0x1C)
                    {
                        // the remote target disconnected; clean up our socket reservation
                        // socket n command register (Sn_CR)
                        WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x10); // 0x10 = CLOSE
                        throw new SocketException(SocketError.Disconnecting);
                    }

                    // if we get here, our socket connection has succeeded!
                    break;
                case ProtocolType.Udp:
                    // nothing else to do
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        internal int CreateSocket(System.Net.Sockets.Socket socket)
        {
            // create a socket using the first available local port
            return CreateSocket(socket, -1);
        }

        internal int CreateSocket(System.Net.Sockets.Socket socket, int sourcePort)
        {
            int socketIndex = -1; // uninitialized
            byte socketStatus;

            // see if we have a free socket available...
            lock (m_SocketsLock)
            {
                for (int i = 0; i < m_Sockets.Length; i++)
                {
                    if (m_Sockets[i] == null)
                    {
                        m_Sockets[i] = socket;
                        socketIndex = i;
                        break;
                    }
                }
            }

            // if we were not able to reserve a socket, throw an exception now.
            if (socketIndex == -1)
                throw new SocketException(SocketError.TooManyOpenSockets);

            // now, try to open our socket...  If we cannot open the socket, we must unreserve the socket from our available pool of sockets.
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            // first, make sure we have this socket configured as "closed" -- while it should never happen, we don't want to operate on an "existing" socket
            // socket n command register (Sn_CR)
            WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x10); // 0x10 = CLOSE
            while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command

            // configure our socket mode
            switch (socket.m_protocolType)
            {
                case ProtocolType.Tcp:
                    // socket n mode register (Sn_MR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0000), 0x21); // TCP
                    break;
                case ProtocolType.Udp:
                    // socket n mode register (Sn_MR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0000), 0x02); // UDP
                    break;
                default:
                    throw new NotSupportedException();
            }

            // get a valid "next source port" number
            bool sourcePortTaken;
            if (sourcePort != -1)
            {
                lock (m_SocketsLock)
                {
                    sourcePortTaken = false;

                    for (int i = 0; i < m_Sockets.Length; i++)
                    {
                        if (m_Sockets[i] != null && m_Sockets[i].LocalEndPoint != null && ((IPEndPoint)m_Sockets[i].LocalEndPoint).Port == sourcePort)
                            sourcePortTaken = true;
                    }

                    if (sourcePortTaken)
                        throw new SocketException(SocketError.AccessDenied);
                }
            }
            else
            {
                int sourcePortNumber;
                lock (m_SocketsLock)
                {
                    do
                    {
                        sourcePortNumber = m_randomGenerator.Next(16383) + 49152; // we use a range of 49152-65535 for source ports.
                        sourcePortTaken = false;

                        for (int i = 0; i < m_Sockets.Length; i++)
                        {
                            if (m_Sockets[i] != null && m_Sockets[i].LocalEndPoint != null && ((IPEndPoint)m_Sockets[i].LocalEndPoint).Port == sourcePortNumber)
                                sourcePortTaken = true;
                        }

                    } while (sourcePortTaken);
                }
            }
            // save our IP address and source port number in our local end point configuration...
            socket.LocalEndPoint = new IPEndPoint(m_ipAddress, sourcePort);
            // socket n source port register (Sn_PORT)
            WriteRegister((UInt16)(socketBaseAddress + 0x0004), (byte)((sourcePort & 0xFF00) >> 8));
            WriteRegister((UInt16)(socketBaseAddress + 0x0005), (byte)(sourcePort & 0xFF));

            if (socket.m_protocolType == ProtocolType.Tcp)
            {
                // configure maximum segment size
                // socket n maximum segment size register (Sn_MSS)
                int maxSegmentSize = 1460; // default
                WriteRegister((UInt16)(socketBaseAddress + 0x0012), (byte)((maxSegmentSize & 0xFF00) >> 8));
                WriteRegister((UInt16)(socketBaseAddress + 0x0013), (byte)(maxSegmentSize & 0xFF));
            }

            //// clear the TOS register (since we are not using RAW mode)
            //// socket n type of service register (Sn_TOS)
            //WriteRegister((UInt16)(socketBaseAddress + 0x0015), 0x00);

            // configure TTL
            // socket n time to live register (Sn_TTL)
            byte timeToLive = 128; // default
            WriteRegister((UInt16)(socketBaseAddress + 0x0016), timeToLive);

            // TODO: configure TX/RX pointers

            switch (socket.m_protocolType)
            {
                case ProtocolType.Tcp:
                    // issue command to open (initialize) the socket
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x01); // 0x01 = OPEN
                    while(ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0); // wait to process the command
                    // wait for open (initialize) to complete
                    do
                    {
                        // socket n status register (Sn_SR)
                        socketStatus = ReadRegister((UInt16)(socketBaseAddress + 0x0003));
                    } while (socketStatus != 0x13); // 0x13 == SOCK_INIT

                    // if we get here, our socket initialization has succeeded!
                    break;
                case ProtocolType.Udp:
                    // issue command to open (initialize) the socket
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x01); // 0x01 = OPEN
                    while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command
                    // wait for open (initialize) to complete
                    do
                    {
                        // socket n status register (Sn_SR)
                        socketStatus = ReadRegister((UInt16)(socketBaseAddress + 0x0003));
                    } while (socketStatus != 0x22); // 0x22 == SOCK_UDP

                    // if we get here, our socket initialization has succeeded!
                    break;
            }

            // set up interrupts for this socket (if we have an interrupt line)
            if (m_interruptPort != null)
            {
                // interrupt mask register (IMR)
                byte interruptMaskRegister = ReadRegister(0x0016);
                // disable interrupts for this socket
                WriteRegister(0x0016, (byte)(interruptMaskRegister & ~(1 << socketIndex)));
                // and then (re)enable interrupts for this socket
                WriteRegister(0x0016, (byte)(interruptMaskRegister | (1 << socketIndex)));
            }

            // return our socket index; this will confirm that our socket has connected.
            return socketIndex;
        }

        internal void DisconnectSocket(int socketIndex)
        {
            byte socketStatus;

            // try to disconnect our socket...  If we cannot disconnect the socket, we will close it instead.  Either way, we will unreserve the socket from our available pool of sockets.
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            switch (m_Sockets[socketIndex].m_protocolType)
            {
                case ProtocolType.Tcp:
                    // issue command to disconnect the socket
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x08); // 0x08 = DISCON
                    while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command

                    // wait for disconnect to complete
                    if (m_interruptPort != null)
                        // TODO: should we implement some sort of timeout here, or generally on any waithandle/status waits?  The user may have unplugged the Wiznet module or it may not be responding...
                        WaitHandle.WaitAny(new WaitHandle[] { m_SocketConfig[socketIndex].DisconnectedInterrupt, m_SocketConfig[socketIndex].TimeoutInterrupt }); // wait for disconnection/timeout
                    do
                    {
                        // socket n status register (Sn_SR)
                        socketStatus = ReadRegister((UInt16)(socketBaseAddress + 0x0003));
                    } while (socketStatus != 0x00); // 0x00 = CLOSED

                    // clean up our socket reservation
                    if (!m_Sockets[socketIndex].IsListening)
                    {
                        lock (m_SocketsLock)
                        {
                            m_Sockets[socketIndex] = null;
                        }
                    }

                    // if we get here, our socket has been closed successfully.
                    break;
                case ProtocolType.Udp:
                    // disconnect is not valid; close instead.
                    CloseSocket(socketIndex);
                    break;
            }
        }

        internal void CloseSocket(int socketIndex)
        {
            byte socketStatus;

            // close our socket.  Unreserve the socket from our available pool of sockets.
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            switch (m_Sockets[socketIndex].m_protocolType)
            {
                case ProtocolType.Tcp:
                case ProtocolType.Udp:
                    // issue command to close the socket
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x10); // 0x10 = CLOSE
                    while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command

                    // wait for close to complete
                    do
                    {
                        // socket n status register (Sn_SR)
                        socketStatus = ReadRegister((UInt16)(socketBaseAddress + 0x0003));
                    } while (socketStatus != 0x00); // 0x00 = CLOSED

                    // clean up our socket reservation
                    lock (m_SocketsLock)
                    {
                        m_Sockets[socketIndex] = null;
                    }

                    // if we get here, our socket has been closed successfully.
                    break;
            }
        }

        internal int Send(int socketIndex, byte[] data, int offset, int size)
        {
            // lock on our transmit buffer lock; only one thread can send to a socket simultaneously
            lock (m_SocketConfig[socketIndex].TransmitBufferLock)
            {
                int bytesToTransmit;
                UInt16 transmitBytesFree;
                UInt16 registerTransmitWritePointer;
                UInt16 transmitWritePointer;
                UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));
                UInt16 socketTransmitBufferBaseAddress = (UInt16)(0x4000 + (socketIndex * SOCKET_TX_BUFFER_SIZE));

                int currentIndex = offset;

                // push all of our data into the socket's transmit buffer; return when all data has been buffered
                while (currentIndex < offset + size)
                {
                    // socket n TX free size register (Sn_TX_FSR)
                    transmitBytesFree = (UInt16)(ReadRegister((UInt16)(socketBaseAddress + 0x0020)) * 0x100);
                    transmitBytesFree += (UInt16)ReadRegister((UInt16)(socketBaseAddress + 0x0021));
                    bytesToTransmit = System.Math.Min(transmitBytesFree, offset - currentIndex + size);

                    // calculate the pointer to where we should starting saving the data in the Wiznet chip's memory space
                    // first, retrieve the value from the TX write pointer register
                    // socket n TX write pointer register (Sn_TX_WR)
                    registerTransmitWritePointer = (UInt16)(ReadRegister((UInt16)(socketBaseAddress + 0x0024)) * 0x100);
                    registerTransmitWritePointer += ReadRegister((UInt16)(socketBaseAddress + 0x0025));
                    // calculate our actual in-memory address (mask off value and add to transmit buffer base memory address)
                    transmitWritePointer = (UInt16)(socketTransmitBufferBaseAddress + (registerTransmitWritePointer & SOCKET_TX_BUFFER_MASK));

                    // now, write data until we reach the end of our buffer
                    while (currentIndex < offset + bytesToTransmit)
                    {
                        WriteRegister(transmitWritePointer, data[currentIndex]);

                        // increment our pointers
                        transmitWritePointer++;
                        currentIndex++;

                        // check to see if we need to wrap-around
                        if (transmitWritePointer == socketTransmitBufferBaseAddress + SOCKET_TX_BUFFER_SIZE)
                        {
                            // if we passed the end of our buffer, wrap around...
                            transmitWritePointer = socketTransmitBufferBaseAddress;
                        }
                    }

                    // update the pointer to show where we have finished writing our data
                    registerTransmitWritePointer = (UInt16)((registerTransmitWritePointer + bytesToTransmit) % 0x10000);
                    // socket n write pointer register (Sn_TX_WR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0024), (byte)((registerTransmitWritePointer & 0xFF00) >> 8));
                    WriteRegister((UInt16)(socketBaseAddress + 0x0025), (byte)(registerTransmitWritePointer & 0xFF));

                    // send the newly-buffered data...
                    // socket n command register (Sn_CR)
                    WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x20);
                    while (ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0) ; // wait to process the command

                    // if we weren't able to write all of our data, then wait for this data to be written so we can write more...
                    if (currentIndex < offset + size)
                    {
                        if (m_interruptPort != null)
                        {
                            // TODO: should we implement some sort of timeout here, or generally on any waithandle/status waits?  The user may have unplugged the Wiznet module or it may not be responding...
                            int result = WaitHandle.WaitAny(new WaitHandle[] { m_SocketConfig[socketIndex].DataWrittenInterrupt, m_SocketConfig[socketIndex].TimeoutInterrupt, m_SocketConfig[socketIndex].DisconnectedInterrupt }); // wait for completion/disconnection/timeout
                            switch (result)
                            {
                                case 0: // datawritten
                                    // do nothing; continue to write more data
                                    break;
                                case 1: // timeout
                                    throw new SocketException(SocketError.TimedOut);
                                case 2: // disconnected
                                    throw new SocketException(SocketError.Disconnecting);
                            }
                        }
                        else
                        {
                            while (true)
                            {
                                // interrupts are not supported, so we'll poll for completion instead.
                                // socket n status register (Sn_SR)
                                byte socketInterruptRegister = ReadRegister((UInt16)(socketBaseAddress + 0x0002));
                                WriteRegister((UInt16)(socketBaseAddress + 0x0002), 0x10); // clear this bit in our interrupt register
                                if ((socketInterruptRegister & 0x10) == 0x10)
                                {
                                    // datawritten
                                    // do nothing; continue to write more data
                                    break;
                                }
                                if ((socketInterruptRegister & 0x08) == 0x08)
                                {
                                    // timeout
                                    WriteRegister((UInt16)(socketBaseAddress + 0x0002), 0x08); // clear this bit in our interrupt register
                                    throw new SocketException(SocketError.TimedOut);
                                }
                                if ((socketInterruptRegister & 0x02) == 0x02)
                                {
                                    // disconnected
                                    throw new SocketException(SocketError.Disconnecting);
                                }
                            }
                        }
                    }
                }

                // if we get here, all data was transmitted.
                return size;
            }
        }

        internal int GetBytesToRead(int socketIndex)
        {
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            int bytesToRead;

            // socket n RX received size register (Sn_RX_RSR)
            bytesToRead = (UInt16)(ReadRegister((UInt16)(socketBaseAddress + 0x0026)) * 0x100);
            bytesToRead += (UInt16)ReadRegister((UInt16)(socketBaseAddress + 0x0027));

            return bytesToRead;
        }

        internal bool WaitForReceivedData(int socketIndex, int milliSeconds)
        {
            UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));

            // first, return true if we already have data...
            if (GetBytesToRead(socketIndex) > 0)
                return true;

            if (m_interruptPort != null)
            {
                // TODO: should we implement some sort of timeout here, or generally on any waithandle/status waits?  The user may have unplugged the Wiznet module or it may not be responding...
                int result = WaitHandle.WaitAny(new WaitHandle[] { m_SocketConfig[socketIndex].DataReceivedInterrupt, m_SocketConfig[socketIndex].DisconnectedInterrupt }, milliSeconds, false); // wait for completion/disconnection/timeout
                switch (result)
                {
                    case 0: // datareceived
                        return true;
                    case 1: // disconnected
                        return true;
                    case WaitHandle.WaitTimeout:
                        return false;
                    default:
                        // we should never get here
                        return false;
                }
            }
            else
            {
                long latestTimeoutTicks = Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks + (milliSeconds * System.TimeSpan.TicksPerMillisecond);
                while (true)
                {
                    // interrupts are not supported, so we'll poll for received data instead.
                    // socket n status register (Sn_SR)
                    byte socketInterruptRegister = ReadRegister((UInt16)(socketBaseAddress + 0x0002));
                    WriteRegister((UInt16)(socketBaseAddress + 0x0002), 0x04); // clear our interrupt register
                    if ((socketInterruptRegister & 0x04) == 0x04)
                    {
                        // datareceived
                        return true;
                    }
                    if ((socketInterruptRegister & 0x02) == 0x02)
                    {
                        // disconnected
                        return true;
                    }

                    if (Microsoft.SPOT.Hardware.Utility.GetMachineTime().Ticks >= latestTimeoutTicks)
                        // timeout
                        return false;

                    // wait 10ms before checking again...
                    Thread.Sleep(10);
                }
            }
        }

        internal int Peek(int socketIndex, byte[] data, int offset, int size)
        {
            // lock on our receive buffer lock; only one thread can read from the receive buffer simultaneously
            lock (m_SocketConfig[socketIndex].ReceiveBufferLock)
            {
                UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));
                UInt16 socketReceiveBufferBaseAddress = (UInt16)(0x6000 + (socketIndex * SOCKET_RX_BUFFER_SIZE));
                UInt16 registerReceiveReadPointer;
                UInt16 receiveReadPointer;

                int currentIndex = offset;

                int bytesToReceive = System.Math.Min(GetBytesToRead(socketIndex), size);

                // calculate the pointer to where we should starting reading the data in the Wiznet chip's memory space
                // first, retrieve the value from the RX read pointer register
                // socket n RX read pointer register (Sn_RX_RD)
                registerReceiveReadPointer = (UInt16)(ReadRegister((UInt16)(socketBaseAddress + 0x0028)) * 0x100);
                registerReceiveReadPointer += ReadRegister((UInt16)(socketBaseAddress + 0x0029));
                // calculate our actual in-memory address (mask off value and add to receive buffer base memory address)
                receiveReadPointer = (UInt16)(socketReceiveBufferBaseAddress + (registerReceiveReadPointer & SOCKET_RX_BUFFER_MASK));

                // now, read data until we reach the end of our buffer
                while (currentIndex < offset + bytesToReceive)
                {
                    data[currentIndex] = ReadRegister(receiveReadPointer);

                    // increment our pointers
                    receiveReadPointer++;
                    currentIndex++;

                    // check to see if we need to wrap-around
                    if (receiveReadPointer == socketReceiveBufferBaseAddress + SOCKET_RX_BUFFER_SIZE)
                    {
                        // if we passed the end of our buffer, wrap around...
                        receiveReadPointer = socketReceiveBufferBaseAddress;
                    }
                }

                // if we get here, data was received.
                return bytesToReceive;
            }
        }

        internal int Receive(int socketIndex, byte[] data, int offset, int size)
        {
            // lock on our receive buffer lock; only one thread can read from the receive buffer simultaneously
            lock (m_SocketConfig[socketIndex].ReceiveBufferLock)
            {
                UInt16 socketBaseAddress = (UInt16)(0x400 + (socketIndex * 0x100));
                UInt16 socketReceiveBufferBaseAddress = (UInt16)(0x6000 + (socketIndex * SOCKET_RX_BUFFER_SIZE));
                UInt16 registerReceiveReadPointer;
                UInt16 receiveReadPointer;

                int currentIndex = offset;

                int bytesToReceive = System.Math.Min(GetBytesToRead(socketIndex), size);
               
                // calculate the pointer to where we should starting reading the data in the Wiznet chip's memory space
                // first, retrieve the value from the RX read pointer register
                // socket n RX read pointer register (Sn_RX_RD)
                registerReceiveReadPointer = (UInt16)(ReadRegister((UInt16)(socketBaseAddress + 0x0028)) * 0x100);
                registerReceiveReadPointer += ReadRegister((UInt16)(socketBaseAddress + 0x0029));
                // calculate our actual in-memory address (mask off value and add to receive buffer base memory address)
                receiveReadPointer = (UInt16)(socketReceiveBufferBaseAddress + (registerReceiveReadPointer & SOCKET_RX_BUFFER_MASK));

                // now, read data until we reach the end of our buffer
                while (currentIndex < offset + bytesToReceive)
                {
                    data[currentIndex] = ReadRegister(receiveReadPointer);

                    // increment our pointers
                    receiveReadPointer++;
                    currentIndex++;

                    // check to see if we need to wrap-around
                    if (receiveReadPointer == socketReceiveBufferBaseAddress + SOCKET_RX_BUFFER_SIZE)
                    {
                        // if we passed the end of our buffer, wrap around...
                        receiveReadPointer = socketReceiveBufferBaseAddress;
                    }
                }

                // update the pointer to show where we have finished reading our data
                registerReceiveReadPointer = (UInt16)((registerReceiveReadPointer + bytesToReceive) % 0x10000);
                // socket n RX read pointer register (Sn_RX_RD)
                WriteRegister((UInt16)(socketBaseAddress + 0x0028), (byte)((registerReceiveReadPointer & 0xFF00) >> 8));
                WriteRegister((UInt16)(socketBaseAddress + 0x0029), (byte)(registerReceiveReadPointer & 0xFF));

                // tell the Wiznet chip to receive more data into the buffer...
                // socket n command register (Sn_CR)
                WriteRegister((UInt16)(socketBaseAddress + 0x0001), 0x40);
                while(ReadRegister((UInt16)(socketBaseAddress + 0x0001)) != 0); // wait to process the command

                // if we get here, data was received.
                return bytesToReceive;
            }
        }
    }
}

