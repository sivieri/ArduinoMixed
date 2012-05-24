using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Erlang.NET;

namespace TestApp
{
    public class Program
    {
        public const int PORT = 3000;
        public const int BUF = 1024;
        private Socket socket = null;

        public static void Main()
        {
            new Program();
        }

        public Program()
        {
            SecretLabs.NETMF.Net.Wiznet5100 wiznet = new SecretLabs.NETMF.Net.Wiznet5100(SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10, Pins.GPIO_PIN_D2);
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces()[0];
            networkInterface.PhysicalAddress = new byte[] { 0x90, 0xA2, 0xDA, 0x0D, 0x32, 0xCF };
            networkInterface.EnableStaticIP("10.0.0.101", "255.255.255.0", "10.0.0.1");
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint local = new IPEndPoint(IPAddress.Parse(networkInterface.IPAddress), PORT);
            this.socket.Bind(local);
            Thread t = new Thread(ListenActivity);
            t.Start();
        }

        private void ListenActivity()
        {
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            for (;;)
            {
                byte[] message = new byte[BUF];
                while (this.socket.Available == 0) { }
                int size = this.socket.ReceiveFrom(message, ref remote);
                Debug.Print("Received " + size + " bytes from " + ((IPEndPoint)remote).Address + " (" + ((IPEndPoint)remote).Port + ")");
                string messageString = "";
                foreach (byte b in message)
                {
                    messageString += (int)b + " ";
                }
                Debug.Print(messageString);
                OtpInputStream inStream = new OtpInputStream(message);
                OtpErlangObject msg = inStream.read_any();
                Debug.Print(msg.ToString());
                OtpErlangTuple t = new OtpErlangTuple(new OtpErlangObject[] { new OtpErlangAtom("ok"), msg });
                OtpOutputStream outStream = new OtpOutputStream(t, false);
                byte[] answer = outStream.GetBuffer();
                this.socket.SendTo(answer, remote);
                Debug.Print(t.ToString());
            }
        }
    }
}
