﻿using System;
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
        public Action<int> OnRecieve;

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

        /// <summary>
        /// Constructs the TCP client
        /// </summary>
        /// <param name="ip">IP address to connect to</param>
        /// <param name="readBuffer">buffer to read data into</param>
        /// <param name="port">port to connect to</param>
        /// <param name="OnRecieve">callback for when bytes are received</param>
        public ArkTCPClient(IPAddress ip, byte[] readBuffer, int port, Action<int> OnRecieve)
        {
            this.IPAddress = ip;
            this.port = port;
            this.OnRecieve = OnRecieve;
            this.ReadBuffer = readBuffer;
            this.client = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
                try
                {
                    while (client.Connected)
                    {
                        if (Exit)
                            break;

                        // Wait for any data, so we dont be cringe
                        if (!NetworkStream.DataAvailable)
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        
                        
                        int bytesRead = NetworkStream.Read(ReadBuffer, 0, 2);

                        // sanity check
                        if (bytesRead < 2)
                        {
                            break;
                        }

                        int len = BitConverter.ToUInt16(ReadBuffer);

                        while (len > bytesRead)
                        {
                            bytesRead += NetworkStream.Read(ReadBuffer, bytesRead, len - bytesRead);
                        }

                        this.OnRecieve(bytesRead);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="data">buffer to send</param>
        /// <param name="length">length of data to send</param>
        public void Send(byte[] data, int length)
        {
            NetworkStream.Write(data, 0, length);
        }


        /// <summary>
        /// Sends data to the server but it has the async keyword
        /// </summary>
        /// <param name="data">buffer to send</param>
        /// <param name="length">length of data to send</param>
        public Task SendAsync(byte[] data, int length)
        {
            return NetworkStream.WriteAsync(data, 0, length);
        }
    }
}
