using System;
using System.Collections.Generic;
using System.Text;

namespace HeadlessTerrariaClient.Client
{
    /// <summary>
    /// Stages that a client can be in the connection protocol
    /// </summary>
    public enum ConnectionState : int
    {
        None,
        SyncingPlayer,
        RequestingWorldData,
        RequestingTileData,
        Connected,
    }
}
