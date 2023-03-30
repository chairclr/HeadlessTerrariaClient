using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

namespace HeadlessTerrariaClient.Network;

public class TerrariaNetworkClient : INetworkClient, IDisposable
{
    public bool Connected { get; private set; }

    internal TCPNetworkClient TCPNetworkClient;

    private Task? ReceiveLoopTask;

    public ReceiveCallback? OnReceiveCallback;

    private readonly MemoryStream ReadBuffer = new MemoryStream(new byte[131070], 0, 131070, false, true);

    private readonly MemoryStream WriteBuffer = new MemoryStream(new byte[131070], 0, 131070, true, true);

    public readonly BinaryReader Reader;

    public readonly BinaryWriter Writer;

    private CancellationToken ReceiveLoopCancellationToken = new CancellationToken();

    private bool Disposed;

    public TerrariaNetworkClient(IPAddress ip, int port)
    {
        TCPNetworkClient = new TCPNetworkClient(ip, port);

        Reader = new BinaryReader(ReadBuffer);

        Writer = new BinaryWriter(WriteBuffer);
    }

    public void Connect()
    {
        if (Connected)
        {
            throw new InvalidOperationException("Already connected.");
        }

        TCPNetworkClient.Connect();

        Connected = true;

        ReceiveLoopTask = Task.Run(ReceiveLoop);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (Connected)
        {
            throw new InvalidOperationException("Already connected.");
        }

        await TCPNetworkClient.ConnectAsync(cancellationToken);

        Connected = true;

        ReceiveLoopTask = Task.Run(ReceiveLoop, CancellationToken.None);
    }

    public void Send(ReadOnlyMemory<byte> data)
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        TCPNetworkClient.Send(data);
    }

    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        await TCPNetworkClient.SendAsync(data, cancellationToken);
    }

    public void Disconnect()
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        ReceiveLoopCancellationToken = new CancellationToken(true);

        ReceiveLoopTask!.Wait();

        TCPNetworkClient.Dispose();

        Connected = false;
    }

    public async Task DisconnectAsync()
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        ReceiveLoopCancellationToken = new CancellationToken(true);

        await ReceiveLoopTask!;

        TCPNetworkClient.Dispose();

        Connected = false;
    }

    private async Task ReceiveLoop()
    {
        byte[] rawReadBuffer = ReadBuffer.GetBuffer();

        try
        {
            while (!ReceiveLoopCancellationToken.IsCancellationRequested)
            {
                ReadBuffer.Position = 0;

                await TCPNetworkClient.NetworkStream!.ReadExactlyAsync(rawReadBuffer.AsMemory(0, 2), ReceiveLoopCancellationToken);

                ushort messageLength = Reader.ReadUInt16();

                await TCPNetworkClient.NetworkStream!.ReadExactlyAsync(rawReadBuffer.AsMemory(2, messageLength - 2), ReceiveLoopCancellationToken);

                if (OnReceiveCallback is not null)
                {
                    await OnReceiveCallback(2, messageLength - 2);
                }
            }
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch (EndOfStreamException)
        {
            Connected = false;

            TCPNetworkClient.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                TCPNetworkClient.Dispose();

                ReadBuffer.Dispose();
                Reader.Dispose();

                WriteBuffer.Dispose();
                Writer.Dispose();
            }

            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public delegate Task ReceiveCallback(int start, int length);
}
