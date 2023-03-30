using System.Net;
using System.Net.Sockets;

namespace HeadlessTerrariaClient.Network;

internal class TCPNetworkClient : INetworkClient, IDisposable
{
    private Socket Socket;

    private readonly IPAddress IPAddress;

    private readonly int Port;

    public NetworkStream? NetworkStream;

    private bool Disposed;

    public TCPNetworkClient(IPAddress ip, int port)
    {
        ArgumentNullException.ThrowIfNull(ip, nameof(ip));

        if (port < ushort.MinValue || port > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        Socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        IPAddress = ip;

        Port = port;
    }

    public void Connect()
    {
        Socket.Connect(IPAddress, Port);

        NetworkStream = new NetworkStream(Socket);
    }

    public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        await Socket.ConnectAsync(IPAddress, Port, cancellationToken);

        NetworkStream = new NetworkStream(Socket);
    }

    public void Send(ReadOnlyMemory<byte> data)
    {
        NetworkStream!.Write(data.Span);
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        await NetworkStream!.WriteAsync(data, cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                Socket.Dispose();
                NetworkStream?.Dispose();
            }

            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
