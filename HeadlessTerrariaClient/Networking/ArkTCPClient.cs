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
        public delegate void OnRecieveBytes(Socket handler, int bytesRead);

        /// <summary>
        /// Raw TCP socket
        /// </summary>
        public Socket client;

        /// <summary>
        /// IP address that is real
        /// </summary>
        public IPAddress IPAddress;

        /// <summary>
        /// Port that the TCP socket is connected on
        /// </summary>
        public int port;

        /// <summary>
        /// Callback for when bytes are received from the server
        /// </summary>
        public OnRecieveBytes OnRecieve;

        /// <summary>
        /// Buffer to read data into
        /// </summary>
        public byte[] ReadBuffer;

        /// <summary>
        /// Networkstream wrapping the TCP socket
        /// </summary>
        public NetworkStream NetworkStream;

        /// <summary>
        /// Task for the client loop
        /// </summary>
        public Task ClientLoop;

        /// <summary>
        /// why
        /// </summary>
        public bool Exit = false;
        public bool IsReading = false;
        public bool IsWriting = false;

        /// <summary>
        /// Constructs the TCP client
        /// </summary>
        /// <param name="ip">IP address to connect to</param>
        /// <param name="readBuffer">buffer to read data into</param>
        /// <param name="port">port to connect to</param>
        /// <param name="OnRecieve">callback for when bytes are received</param>
        public ArkTCPClient(IPAddress ip, byte[] readBuffer, int port, OnRecieveBytes OnRecieve)
        {
            IPAddress = ip;
            this.port = port;
            this.OnRecieve = OnRecieve;
            ReadBuffer = readBuffer;
            client = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Connects to the server and starts the client loop
        /// </summary>
        public async Task Connect()
        {
            client.Connect(IPAddress, port);
            ClientLoop = Task.Run(RunClientLoop);
            return;
        }

        /// <summary>
        /// Starts receiving data from the server
        /// </summary>
        private async Task RunClientLoop()
        {
            using (NetworkStream = new NetworkStream(client))
            {
                while (client.Connected)
                {
                    if (IsReading)
                    {
                        await Task.Delay(16);
                        continue;
                    }
                    if (client.Available <= 2)
                    {
                        await Task.Delay(16);
                        continue;
                    }
                    if (Exit)
                        return;
                    try
                    {
                        IsReading = true;


                        // read the length of the packet from the network into the first 2 bytes of the ReadBuffer
                        int bytesRead = NetworkStream.Read(ReadBuffer, 0, 2);

                        if (bytesRead < 2)
                        {
                            break;
                        }

                        int len = BitConverter.ToInt16(ReadBuffer);

                        while (bytesRead < len)
                        {
                            int bytesReceived = NetworkStream.Read(ReadBuffer, bytesRead, len - bytesRead);
                            bytesRead += bytesReceived;
                        }

                        this.OnRecieve(client, bytesRead);


                        IsReading = false;
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

        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="data">byte array to send</param>
        /// <param name="length">length of data to send, if length is -1, send the entire buffer</param>
        public void Send(byte[] data, int length = -1)
        {
            while (IsWriting)
                Thread.Sleep(16);
            IsWriting = true;
            if (length == -1)
                length = data.Length;
            NetworkStream.Write(data, 0, length);
            IsWriting = false;
        }


        /// <summary>
        /// Sends data to the server but it has the async keyword
        /// </summary>
        /// <param name="data">byte array to send</param>
        /// <param name="length">length of data to send, if length is -1, send the entire buffer</param>
        public async Task SendAsync(byte[] data, int length = -1)
        {
            while (IsWriting)
                Thread.Sleep(16);
            IsWriting = true;
            if (length == -1)
                length = data.Length;
            await NetworkStream.WriteAsync(data, 0, length);
            IsWriting = false;
        }
    }
}
