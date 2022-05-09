using System;
using System.Collections.Generic;
using System.Text;

namespace HeadlessTerrariaClient.Client
{
    public enum ConnectionState : int
    {
        None,
        SyncingPlayer,
        RequestingWorldData,
        RequestingTileData,
        Connected,
    }
}
