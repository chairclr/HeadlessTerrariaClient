namespace HeadlessTerrariaClient.Network;

/// <summary>
/// Stages that a client can be in the connection protocol
/// </summary>
public enum ConnectionState : int
{
    /// <summary>
    /// The client is not connected at all
    /// </summary>
    None,

    /// <summary>
    /// The client and server are syncing player data
    /// </summary>
    SyncingPlayer,

    /// <summary>
    /// The client is requesting the world data, such as width and height
    /// </summary>
    RequestingWorldData,

    /// <summary>
    /// The client is requesting the spawn tile data of the server
    /// </summary>
    RequestingSpawnTileData,

    /// <summary>
    /// The client is spawning the player
    /// </summary>
    SpawningPlayer,

    /// <summary>
    /// The client is fully connected to the server
    /// </summary>
    Connected,

    /// <summary>
    /// The client has finished the connection process and is ready to do some stuff
    /// </summary>
    FinishedConnecting,
}