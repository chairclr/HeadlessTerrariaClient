using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Messages;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient : IDisposable
{
    internal readonly TerrariaNetworkClient TerrariaNetworkClient;

    private readonly TerrariaMessageHandler MessageHandler;

    private readonly TerrariaMessageWriter MessageWriter;

    public bool Connected => TerrariaNetworkClient.Connected;

    public ConnectionState ConnectionState { get; private set; } = ConnectionState.None;

    public World World = new World();

    public int LocalPlayerIndex = 0;

    public Player LocalPlayer => World.Players[LocalPlayerIndex];

    public string ClientUUID = Guid.NewGuid().ToString();

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

        TerrariaNetworkClient = new TerrariaNetworkClient(foundIp, port);

        MessageHandler = new TerrariaMessageHandler(this);

        TerrariaNetworkClient.OnReceiveCallback = MessageHandler.ReceiveMessage;

        MessageWriter = new TerrariaMessageWriter(TerrariaNetworkClient.Writer);
    }

    public void Connect()
    {
        TerrariaNetworkClient.Connect();

        SendHello();

        ConnectionState = ConnectionState.SyncingPlayer;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await TerrariaNetworkClient.ConnectAsync(cancellationToken);

        await SendHelloAsync(cancellationToken: cancellationToken);

        ConnectionState = ConnectionState.SyncingPlayer;
    }

    public void Disconnect()
    {
        ConnectionState = ConnectionState.None;

        TerrariaNetworkClient.Disconnect();
    }

    public async Task DisconnectAsync()
    {
        ConnectionState = ConnectionState.None;

        await TerrariaNetworkClient.DisconnectAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                TerrariaNetworkClient.Dispose();
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
