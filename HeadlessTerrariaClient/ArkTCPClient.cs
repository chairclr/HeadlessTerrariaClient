using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkNetwork
{
    public class ArkTCPClient
    {
        public delegate void ConnectionAccepted(Socket handler);
        public delegate void ConnectionClosed(Socket handler);
        public delegate int OnRecieveBytes(Socket handler, int bytesRead);

        public Socket client;
        public IPAddress IPAddress;
        public int port;
        public OnRecieveBytes OnRecieve;
        public byte[] ReadBuffer;
        public NetworkStream NetworkStream;
        public Task ClientLoop;
        public bool Exit = false;

        public ArkTCPClient(IPAddress ip, byte[] readBuffer, int port, OnRecieveBytes OnRecieve)
        {
            IPAddress = ip;
            this.port = port;
            this.OnRecieve = OnRecieve;
            ReadBuffer = readBuffer;
            client = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task Connect()
        {
            client.Connect(IPAddress, port);
            ClientLoop = Task.Run(RunClientLoop);
            return;
        }

        private async Task RunClientLoop()
        {
            using (NetworkStream = new NetworkStream(client))
            {
                int nextPacketLength = -1;
                int lastPacketLength = 0;
                while (client.Connected)
                {
                    if (Exit)
                        return;
                    try
                    {
                        int dataLength = nextPacketLength;
                        if (nextPacketLength == -1)
                        {
                            // read the length of the packet from the network into the first 2 bytes of the ReadBuffer
                            NetworkStream.Read(ReadBuffer, 0, 2);
                            int len = BitConverter.ToInt16(ReadBuffer);

                            int bytesRead = 2;
                            while (bytesRead < len)
                            {
                                int bytesReceived = NetworkStream.Read(ReadBuffer, 2, len - bytesRead);
                                bytesRead += bytesReceived;
                            }
                            lastPacketLength = bytesRead + 2;
                            nextPacketLength = this.OnRecieve(client, bytesRead);
                        }
                        else
                        {
                            // copy the length of the packet from the last 2 bytes of the ReadBuffer into the first 2 bytes of the ReadBuffer
                            ReadBuffer[0] = ReadBuffer[lastPacketLength - 2];
                            ReadBuffer[1] = ReadBuffer[lastPacketLength - 1];
                            int len = BitConverter.ToInt16(ReadBuffer);

                            int bytesRead = 0;
                            while (bytesRead < len)
                            {
                                int bytesReceived = NetworkStream.Read(ReadBuffer, 2, len - bytesRead);
                                bytesRead += bytesReceived;
                            }
                            lastPacketLength = bytesRead;
                            nextPacketLength = this.OnRecieve(client, bytesRead);
                        }
                    }
                    catch (AggregateException ae)
                    {
                        Console.WriteLine(ae.ToString());
                    }
                    catch (ObjectDisposedException ode)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        break;
                    }
                }
            }
        }

        public void Send(byte[] data, int length = -1)
        {
            if (length == -1)
                length = data.Length;
            NetworkStream.Write(data, 0, length);
        }
    }
}
