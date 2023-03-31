namespace HeadlessTerrariaClient.Network;

internal interface INetworkClient
{
    public void Connect();

    public ValueTask ConnectAsync(CancellationToken cancellationToken);

    public void Send(ReadOnlyMemory<byte> data);

    public ValueTask SendAsync(ReadOnlyMemory<byte> data);
}
