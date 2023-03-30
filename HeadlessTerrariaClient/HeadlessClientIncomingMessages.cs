using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Messages;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient
{
    [IncomingMessage(MessageType.Kick)]
    internal async ValueTask HandleKick(BinaryReader reader)
    {
        ConnectionState = ConnectionState.None;

        WasKicked = true;

        KickReason = reader.ReadNetworkText().ToString(); 

        await TCPNetworkClient.DisconnectAsync();
    }

    [IncomingMessage(MessageType.PlayerInfo)]
    internal async ValueTask HandlePlayerInfo(BinaryReader reader)
    {
        int playerIndex = reader.ReadByte();

        bool checkBytesInClientLoopThread = reader.ReadBoolean();

        if (LocalPlayerIndex != playerIndex)
        {
            lock (World)
            {
                (World.Players[LocalPlayerIndex], World.Players[playerIndex]) = (World.Players[playerIndex], World.Players[LocalPlayerIndex]);

                LocalPlayerIndex = playerIndex;

                World.UpdatePlayerIndexes();
            }
        }

        LocalPlayer.Active = false;

        await SendSyncLocalPlayerAsync();
        await SendClientUUIDAsync();
        await SendPlayerLifeAsync();
        await SendPlayerManaAsync();
        await SendSyncPlayerBuffsAsync();
        await SendSyncLoadoutAsync();

        for (int i = 0; i < 350; i++)
        {
            await SendSyncEquipmentAsync(i);
        }

        if (ConnectionState == ConnectionState.SyncingPlayer)
        {
            ConnectionState = ConnectionState.RequestingWorldData;
        }

        await SendRequestWorldDataAsync();
    }

    [IncomingMessage(MessageType.WorldData)]
    internal async ValueTask HandleWorldData(BinaryReader reader)
    {
        World.HandleWorldData(reader);

        World.Tile.ResetHeap();

        if (ConnectionState == ConnectionState.RequestingWorldData)
        {
            ConnectionState = ConnectionState.RequestingSpawnTileData;
            await SendSpawnTileDataAsync();
        }
    }

    [IncomingMessage(MessageType.InitialSpawn)]
    internal async ValueTask HandleInitialSpawn(BinaryReader reader)
    {
        if (ConnectionState == ConnectionState.RequestingSpawnTileData)
        {
            ConnectionState = ConnectionState.SpawningPlayer;
        }

        LocalPlayer.Spawn(World.SpawnTileX, World.SpawnTileY);

        if (ConnectionState == ConnectionState.SpawningPlayer)
        {
            await SendPlayerSpawnAsync(1);

            ConnectionState = ConnectionState.Connected;
        }
    }

    [IncomingMessage(MessageType.UpdateWorldEvil)]
    internal void HandleUpdateWorldEvil(BinaryReader reader)
    {
        byte good = reader.ReadByte();
        byte evil = reader.ReadByte();
        byte blood = reader.ReadByte();
    }

    [IncomingMessage(MessageType.FinishedConnectingToServer)]
    internal void HandleFinishedConnectingToServer(BinaryReader reader)
    {
        ConnectionState = ConnectionState.FinishedConnecting;
    }
}
