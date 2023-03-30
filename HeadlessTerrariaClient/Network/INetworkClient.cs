namespace HeadlessTerrariaClient.Network;

internal interface INetworkClient
{
    public void Connect();

    public Task ConnectAsync(CancellationToken cancellationToken);

    public void Send(ReadOnlyMemory<byte> data);

    public Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
}
