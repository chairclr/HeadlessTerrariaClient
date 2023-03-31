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

    [IncomingMessage(MessageType.SyncPlayer)]
    internal void HandleSyncPlayer(BinaryReader reader)
    {
        int playerIndex = reader.ReadByte();

        if (playerIndex == LocalPlayerIndex && !World.ServerSideCharacters)
        {
            return;
        }

        Player player = World.Players[playerIndex];

        player.Index = playerIndex;

        player.Style.SkinVariant = reader.ReadByte();

        player.Style.SkinVariant = (int)Math.Clamp(player.Style.SkinVariant, 0f, 11);

        player.Style.HairType = reader.ReadByte();
        if (player.Style.HairType >= 165)
        {
            player.Style.HairType = 0;
        }

        player.Name = reader.ReadString().Trim().Trim();

        player.Style.HairDye = reader.ReadByte();

        reader.ReadAccessoryVisibility(new bool[10]);

        //player.HideMisc = reader.ReadByte();
        reader.ReadByte();
        player.Style.HairColor = reader.ReadRGB();
        player.Style.SkinColor = reader.ReadRGB();
        player.Style.EyeColor = reader.ReadRGB();
        player.Style.ShirtColor = reader.ReadRGB();
        player.Style.UnderShirtColor = reader.ReadRGB();
        player.Style.PantsColor = reader.ReadRGB();
        player.Style.ShoeColor = reader.ReadRGB();

        BitsByte playerDifficultyData = reader.ReadByte();
        player.Difficulty = PlayerDifficulty.Normal;
        if (playerDifficultyData[0])
        {
            player.Difficulty = PlayerDifficulty.Mediumcore;
        }
        if (playerDifficultyData[1])
        {
            player.Difficulty = PlayerDifficulty.Hardcode;
        }
        if (playerDifficultyData[3])
        {
            player.Difficulty = PlayerDifficulty.Creative;
        }

        //player.ExtraAccessory = bitsByte24[2];

        BitsByte torchAndCartData = reader.ReadByte();
        //player.UsingBiomeTorches = bitsByte25[0];
        //player.HappyFunTorchTime = bitsByte25[1];
        //player.UnlockedBiomeTorches = bitsByte25[2];
        //player.UnlockedSuperCart = bitsByte25[3];
        //player.EnabledSuperCart = bitsByte25[4];

        BitsByte extraBuffData = reader.ReadByte();
        //player.UsedAegisCrystal = bitsByte26[0];
        //player.UsedAegisFruit = bitsByte26[1];
        //player.UsedArcaneCrystal = bitsByte26[2];
        //player.UsedGalaxyPearl = bitsByte26[3];
        //player.UsedGummyWorm = bitsByte26[4];
        //player.UsedAmbrosia = bitsByte26[5];
        //player.AteArtisanBread = bitsByte26[6];
    }

    [IncomingMessage(MessageType.SyncEquipment)]
    internal void HandleSyncEquipment(BinaryReader reader)
    {
        int playerIndex = reader.ReadByte();

        Player player = World.Players[playerIndex];

        if (playerIndex == LocalPlayerIndex && !World.ServerSideCharacters)
        {
            return;
        }

        int slot = reader.ReadInt16();
        int stack = reader.ReadInt16();
        int prefix = reader.ReadByte();
        int netId = reader.ReadInt16();

        player.Inventory[slot].SetTypeFromNetId(netId);
        player.Inventory[slot].Prefix = prefix;
        player.Inventory[slot].Stack = stack;
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


    [IncomingMessage(MessageType.PlayerActive)]
    internal void HandlePlayerActive(BinaryReader reader)
    {
        int playerIndex = reader.ReadByte();

        Player player = World.Players[playerIndex];

        bool active = reader.ReadBoolean();

        if (active)
        {
            if (!player.Active)
            {
                World.Players[playerIndex] = new Player();

                World.UpdatePlayerIndexes();
            }

            World.Players[playerIndex].Active = true;
        }
        else
        {
            World.Players[playerIndex].Active = false;
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
