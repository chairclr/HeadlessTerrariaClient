using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Messages;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient : IDisposable
{
    internal readonly TCPNetworkClient TCPNetworkClient;

    private readonly TerrariaMessageHandler MessageHandler;

    private readonly TerrariaMessageWriter MessageWriter;

    public bool Connected => TCPNetworkClient.Connected;

    public ConnectionState ConnectionState { get; private set; } = ConnectionState.None;

    public World World = new World();

    public int LocalPlayerIndex = 0;

    public Player LocalPlayer => World.Players[LocalPlayerIndex];

    public string ClientUUID = Guid.NewGuid().ToString();

    public bool WasKicked { get; private set; }

    public string? KickReason { get; private set; }

    private bool Disposed;

    public HeadlessClient(string ip, int port)
    {
        IPAddress? foundIp = null;

        if (IPAddress.TryParse(ip, out IPAddress? parsed))
        {
            foundIp = parsed;
        }
        else
        {
            IPAddress[] foundIps = Dns.GetHostAddresses(ip);

            for (int i = 0; i < foundIps.Length; i++)
            {
                if (foundIps[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    foundIp = foundIps[i];
                }
            }
        }

        if (foundIp is null)
        {
            throw new ArgumentException(null, nameof(ip));
        }

        TCPNetworkClient = new TCPNetworkClient(foundIp, port);

        MessageHandler = new TerrariaMessageHandler(this);

        TCPNetworkClient.OnReceiveCallback = MessageHandler.ReceiveMessage;

        MessageWriter = new TerrariaMessageWriter(TCPNetworkClient.Writer);
    }

    public void Connect()
    {
        TCPNetworkClient.Connect();

        SendHello();

        ConnectionState = ConnectionState.SyncingPlayer;
    }

    public async ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        await TCPNetworkClient.ConnectAsync(cancellationToken);
    }

    public void Disconnect()
    {
        ConnectionState = ConnectionState.None;

        TCPNetworkClient.Disconnect();
    }

    public async ValueTask DisconnectAsync()
    {
        ConnectionState = ConnectionState.None;

        await TCPNetworkClient.DisconnectAsync();
    }

    public bool JoinWorld(CancellationToken cancellationToken = default)
    {
        SendHello();

        ConnectionState = ConnectionState.SyncingPlayer;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (ConnectionState == ConnectionState.FinishedConnecting)
            {
                return true;
            }

            if (ConnectionState == ConnectionState.None)
            {
                return false;
            }

            Thread.Sleep(1);
        }

        return false;
    }

    public bool JoinWorld(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        SendHello();

        ConnectionState = ConnectionState.SyncingPlayer;

        Stopwatch watch = new Stopwatch();
        watch.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (watch.Elapsed > timeout)
            {
                return false;
            }

            if (ConnectionState == ConnectionState.FinishedConnecting)
            {
                return true;
            }

            if (ConnectionState == ConnectionState.None)
            {
                return false;
            }

            Thread.Sleep(1);
        }

        return false;
    }

    public async ValueTask<bool> JoinWorldAsync(CancellationToken cancellationToken = default)
    {
        await SendHelloAsync(cancellationToken: cancellationToken);

        ConnectionState = ConnectionState.SyncingPlayer;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (ConnectionState == ConnectionState.FinishedConnecting)
            {
                return true;
            }

            if (ConnectionState == ConnectionState.None)
            {
                return false;
            }

            await Task.Delay(1, cancellationToken);
        }

        return false;
    }

    public async ValueTask<bool> JoinWorldAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        await SendHelloAsync(cancellationToken: cancellationToken);

        ConnectionState = ConnectionState.SyncingPlayer;

        Stopwatch watch = new Stopwatch();
        watch.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (watch.Elapsed > timeout)
            {
                return false;
            }

            if (ConnectionState == ConnectionState.FinishedConnecting)
            {
                return true;
            }

            if (ConnectionState == ConnectionState.None)
            {
                return false;
            }

            await Task.Delay(1, cancellationToken);
        }

        return false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                TCPNetworkClient.Dispose();
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
