using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace HeadlessTerrariaClient.Network;

public class TCPNetworkClient : INetworkClient, IDisposable
{
    public bool Connected { get; private set; }

    private Socket Socket;

    public bool Blocking { get => Socket.Blocking; set => Socket.Blocking = value; }

    private readonly IPAddress IPAddress;

    private readonly int Port;

    internal NetworkStream? NetworkStream;

    private Task? ReceiveLoopTask;

    public ReceiveCallback? OnReceiveCallback;

    private readonly MemoryStream ReadBuffer = new MemoryStream(new byte[131070], 0, 131070, false, true);

    private readonly MemoryStream WriteBuffer = new MemoryStream(new byte[131070], 0, 131070, true, true);

    public readonly BinaryReader Reader;

    public readonly BinaryWriter Writer;

    private CancellationToken ReceiveLoopCancellationToken = new CancellationToken();

    private bool Disposed;

    public TCPNetworkClient(IPAddress ip, int port)
    {
        ArgumentNullException.ThrowIfNull(ip, nameof(ip));

        if (port < ushort.MinValue || port > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        Socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            Blocking = true
        };

        IPAddress = ip;

        Port = port;

        Reader = new BinaryReader(ReadBuffer);

        Writer = new BinaryWriter(WriteBuffer);
    }

    public void Connect()
    {
        if (Connected)
        {
            throw new InvalidOperationException("Already connected.");
        }

        Socket.Connect(IPAddress, Port);

        NetworkStream = new NetworkStream(Socket);

        Connected = true;

        ReceiveLoopTask = Task.Run(ReceiveLoop);
    }

    public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (Connected)
        {
            throw new InvalidOperationException("Already connected.");
        }

        await Socket.ConnectAsync(IPAddress, Port, cancellationToken);

        NetworkStream = new NetworkStream(Socket);

        Connected = true;

        ReceiveLoopTask = Task.Run(ReceiveLoop, CancellationToken.None);
    }

    public void Send(ReadOnlyMemory<byte> data)
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        NetworkStream!.Write(data.Span);
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> data)
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        await NetworkStream!.WriteAsync(data);
    }

    public void Disconnect()
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        ReceiveLoopCancellationToken = new CancellationToken(true);

        ReceiveLoopTask!.Wait();

        Socket.Dispose();
        NetworkStream?.Dispose();

        Connected = false;
    }

    public async ValueTask DisconnectAsync()
    {
        if (!Connected)
        {
            throw new InvalidOperationException("Not connected.");
        }

        ReceiveLoopCancellationToken = new CancellationToken(true);

        await ReceiveLoopTask!;

        Socket.Dispose();
        NetworkStream?.Dispose();

        Connected = false;
    }

    private void ReceiveLoop()
    {
        byte[] rawReadBuffer = ReadBuffer.GetBuffer();
        ref byte messageLengthPointer = ref rawReadBuffer[0];

        try
        {
            while (!ReceiveLoopCancellationToken.IsCancellationRequested)
            {
                NetworkStream!.ReadExactly(rawReadBuffer.AsSpan(0, 2));

                ushort messageLength = Unsafe.ReadUnaligned<ushort>(ref messageLengthPointer);

                NetworkStream!.ReadExactly(rawReadBuffer.AsSpan(2, messageLength - 2));

                ReadBuffer.Position = 2;

                if (OnReceiveCallback is not null)
                {
                    OnReceiveCallback(2, messageLength - 2);
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

            Socket.Dispose();
            NetworkStream?.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                Socket.Dispose();
                NetworkStream?.Dispose();

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
    
    public delegate void ReceiveCallback(int start, int length);
}
