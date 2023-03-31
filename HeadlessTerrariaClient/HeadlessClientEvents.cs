using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient
{
    public event Action<int, NetworkText>? ChatMessageReceived;
}
