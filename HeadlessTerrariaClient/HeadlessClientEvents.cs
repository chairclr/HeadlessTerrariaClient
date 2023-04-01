using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient
{
    public event Action<int, NetworkText>? ChatMessageReceived;

    public event Action<TileManipulationAction>? TileManipulationReceived;
}


public class TileManipulationAction
{
    public TileManipulationType Type;

    public int X;

    public int Y;

    public short Flags1;
    
    public byte Flags2;
}