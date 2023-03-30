using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Messages;

namespace HeadlessTerrariaClient;

public partial class HeadlessClient
{
    [HandleMessage(MessageType.PlayerInfo)]
    internal async Task HandlePlayerInfo(BinaryReader reader)
    {
        int playerIndex = reader.ReadByte();

        bool checkBytesInClientLoopThread = reader.ReadBoolean();

        if (LocalPlayerIndex != playerIndex)
        {
            lock (World)
            {
                (World.Players[LocalPlayerIndex], World.Players[playerIndex]) = (World.Players[playerIndex], World.Players[LocalPlayerIndex]);

                World.UpdatePlayerIndexes();
            }
        }

        LocalPlayer.Active = false;

        await SendSyncLocalPlayerAsync();
        await SendClientUUIDAsync();
        await SendPlayerLifeAsync();
        await SendPlayerManaAsync();
        await SendSyncPlayerBuffsAsync();

        for (int i = 0; i < 350; i++)
        {
            await SendSyncEquipmentAsync(i);
        }

        ConnectionState = Network.ConnectionState.RequestingWorldData;

        await SendRequestWorldDataAsync();
    }
}
