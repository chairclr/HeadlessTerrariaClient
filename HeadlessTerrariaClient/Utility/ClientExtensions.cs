﻿using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using System.Net.Sockets;
using System.Numerics;
using HeadlessTerrariaClient.Client;

namespace HeadlessTerrariaClient.Utility
{
    /// <summary>
    /// Extensions to HeadlessClient that are very useful, yet aren't actually part of the client
    /// </summary>
    public static class ClientExtensions
    {
        /// <summary>
        /// Sends a chat message to the server
        /// </summary>
        /// <param name="msg">message to send</param>
        public static void SendChatMessage(this HeadlessClient client, string msg)
        {
            lock (client.WriteBuffer)
            {
                BinaryWriter writer = client.MessageWriter;

                writer.Seek(2, SeekOrigin.Begin);

                writer.Write((byte)MessageID.NetModules);

                // module type
                writer.Write((ushort)NetModuleID.Text);

                // NetworkText mode
                NetworkText networkText = new NetworkText(msg);

                networkText.Serialize(writer);

                int length = (int)client.MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);

                client.TCPClient.Send(client.WriteBuffer, length);
            }
        }

        /// <summary>
        /// Gets a players index by their name
        /// </summary>
        /// <param name="name">name to be searched for</param>
        /// <returns>the index into client.World.Players[] if the player was found, but -1 if no player by that name exists</returns>
        public static int FindPlayerByName(this HeadlessClient client, string name)
        {
            for (int i = 0; i < 255; i++)
            {
                if (client.World.Players[i].active && client.World.Players[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Teleports the player directly to that position (does NOT update client.LocalPlayer.position)
        /// </summary>
        /// <param name="position">position to teleport to</param>
        public static void Teleport(this HeadlessClient client, Vector2 position)
        {
             client.CustomSendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI, position.X, position.Y);
        }

        /// <summary>
        /// Teleports the player directly to the tile (does NOT update client.LocalPlayer.position)
        /// </summary>
        /// <param name="tileX">position to teleport to</param>
        /// <param name="tileY">position to teleport to</param>
        public static void Teleport(this HeadlessClient client, int tileX, int tileY)
        {
             client.Teleport(new Vector2(tileX * 16, tileY * 16));
        }

        /// <summary>
        /// Teleports the client directly to position and then back 
        /// </summary>
        /// <param name="position">position to teleport to</param>
        public static void TeleportThereAndBack(this HeadlessClient client, Vector2 position)
        {
            client.CustomSendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI, position.X, position.Y);
            client.SendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI);
        }

        /// <summary>
        /// Teleports the client directly to the tile and then back 
        /// </summary>
        /// <param name="tileX">position to teleport to</param>
        /// <param name="tileY">position to teleport to</param>
        public static void TeleportThereAndBack(this HeadlessClient client, int tileX, int tileY)
        {
            client.TeleportThereAndBack(new Vector2(tileX * 16, tileY * 16));
        }

        /// <summary>
        /// Breaks a tile
        /// </summary>
        public static void SendBreakTile(this HeadlessClient client, int tileX, int tileY)
        {
            client.SendData(MessageID.TileManipulation, TileManipulationID.KillTileNoItem, tileX, tileY);
        }

        /// <summary>
        /// Places a tile
        /// </summary>
        /// <param name="bypassTShock">whether or not to attempt to bypass TShock's default anti-cheat</param>
        public static void SendPlaceTile(this HeadlessClient client, int tileX, int tileY, int type, bool bypassTShock = true)
        {
            if (bypassTShock)
            {
                client.CustomSendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI, tileX * 16f, tileY * 16f);
                client.CustomSendData(MessageID.SyncEquipment, client.LocalPlayer.whoAmI, 0, 1, BlockTypeItem.TileToItem[type]);

                client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceTile, tileX, tileY, type);
            }
            else
                client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceTile, tileX, tileY, type);
        }

        /// <summary>
        /// Breaks a wall
        /// </summary>
        public static void SendBreakWall(this HeadlessClient client, int tileX, int tileY)
        {
            client.SendData(MessageID.TileManipulation, TileManipulationID.KillWall, tileX, tileY);
        }

        /// <summary>
        /// Places a wall
        /// </summary>
        /// <param name="bypassTShock">whether or not to attempt to bypass TShock's default anti-cheat</param>
        public static void SendPlaceWall(this HeadlessClient client, int tileX, int tileY, int type, bool bypassTShock = true)
        {
            if (bypassTShock)
            {
                client.CustomSendData(MessageID.SyncEquipment, client.LocalPlayer.whoAmI, 0, 1,BlockTypeItem.WallToItem[type]);

                client.CustomSendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI, tileX * 16f, tileY * 16f);
                client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceWall, tileX, tileY, type);
            }
            else
                client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceWall, tileX, tileY, type);
        }

        /// <summary>
        /// Paints a tile
        /// </summary>
        /// <param name="bypassTShock">whether or not to attempt to bypass TShock's default anti-cheat</param>
        public static void SendPaintTile(this HeadlessClient client, int tileX, int tileY, int paintType, bool bypassTShock = true)
        {
            if (bypassTShock)
            {
                client.CustomSendData(MessageID.SyncEquipment, client.LocalPlayer.whoAmI, 0, 1,ItemID.Paintbrush);
                client.CustomSendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI, tileX * 16f, tileY * 16f);
                client.SendData(MessageID.PaintTile, tileX, tileY, paintType);
            }
            else
                client.SendData(MessageID.PaintTile, tileX, tileY, paintType);
        }

        /// <summary>
        /// Paints a wall
        /// </summary>
        /// <param name="bypassTShock">whether or not to attempt to bypass TShock's default anti-cheat</param>
        public static void SendPaintWall(this HeadlessClient client, int tileX, int tileY, int paintType, bool bypassTShock = true)
        {
            if (bypassTShock)
            {
                client.CustomSendData(MessageID.PlayerControls, client.LocalPlayer.whoAmI, tileX * 16f, tileY * 16f);
                client.CustomSendData(MessageID.SyncEquipment, client.LocalPlayer.whoAmI, 0, 1, ItemID.PaintRoller);
                client.SendData(MessageID.PaintWall, tileX, tileY, paintType);
            }
            else
                client.SendData(MessageID.PaintWall, tileX, tileY, paintType);
        }

        /// <summary>
        /// Deletes or "picks up" an item off the ground
        /// </summary>
        /// <param name="bypassTShock">whether or not to attempt to bypass TShock's default anti-cheat</param>
        public static void RemoveItem(this HeadlessClient client, int i, bool bypassTShock = true)
        {
            if (bypassTShock)
                client.Teleport(client.World.Items[i].position);
            client.World.Items[i].active = false;
            client.World.Items[i].stack = 0;
            client.World.Items[i].type = 0;
            client.World.Items[i].prefix = 0;
            client.SendData(MessageID.SyncItem, i);
        }

        /// <summary>
        /// Spawns or "throws" an item onto the ground
        /// </summary>
        /// <param name="stack">number of items in the stack</param>
        /// <param name="bypassTShock">whether or not to attempt to bypass TShock's default anti-cheat</param>
        public static void SpawnItem(this HeadlessClient client, int type, int stack, Vector2 positoin, Vector2 velocity, bool bypassTShock = true)
        {
            if (bypassTShock)
                client.Teleport(positoin);

            int nextItemIndex = 400;

            int num = 0;
            int num2 = 400;
            int num3 = 1;
            for (int i = num; i != num2; i += num3)
            {
                if (!client.World.Items[i].active)
                {
                    nextItemIndex = i;
                    break;
                }
            }

            client.World.Items[nextItemIndex] = new Item(type, stack, 0, true);
            client.World.Items[nextItemIndex].velocity = velocity;
            client.World.Items[nextItemIndex].position = positoin;

            client.SendData(MessageID.SyncItem, nextItemIndex);
        }

        /// <summary>
        /// Loads the entire world into memory
        /// </summary>
        public static Task LoadEntireWorld(this HeadlessClient client, int timeout = 25)
        {
            return Task.Run(async () =>
            {
                bool entireWorldLoaded = false;
                while (!entireWorldLoaded)
                {
                    bool hasGoneToNewChunk = false;
                    for (int x = 0; x < client.World.CurrentWorld.LoadedTileSections.GetLength(0); x++)
                    {
                        for (int y = 0; y < client.World.CurrentWorld.LoadedTileSections.GetLength(1); y++)
                        {
                            if (!client.World.CurrentWorld.LoadedTileSections[x, y])
                            {
                                if (!hasGoneToNewChunk)
                                {
                                    client.Teleport(x * 200 + 100, y * 150 + 75);
                                    hasGoneToNewChunk = true;
                                    break;
                                }
                            }
                        }
                        if (hasGoneToNewChunk)
                            break;
                    }

                    if (!hasGoneToNewChunk)
                        break;
                    await Task.Delay(timeout);
                }
            });
        }

        /// <summary>
        /// Waits for the client to be fully connected
        /// </summary>
        public static Task WaitForClientToFinishConnecting(this HeadlessClient client)
        {
            return Task.Run(async () =>
            {
                while (!client.IsInWorld)
                {
                    await Task.Delay(32);
                }
            });
        }

        /// <summary>
        /// For trolling
        /// </summary>
        public static void CustomSendData(this HeadlessClient client, int messageType, int number = 0, float number2 = 0, float number3 = 0, float number4 = 0, float number5 = 0, float number6 = 0, float number7 = 0, float number8 = 0)
        {
            if (client.TCPClient == null)
                return;
            lock (client.WriteBuffer)
            {
                BinaryWriter writer = client.MessageWriter;

                writer.Seek(2, SeekOrigin.Begin);

                writer.Write((byte)messageType);
                switch (messageType)
                {
                    case MessageID.SyncEquipment:
                    {
                        // player index
                        writer.Write((byte)number);

                        // inventory index
                        writer.Write((short)number2);

                        // stack?
                        writer.Write((short)number3);
                        // prefix?
                        writer.Write((byte)0);
                        // type?
                        writer.Write((short)number4);
                        break;
                    }
                    case MessageID.PlayerControls:
                    {
                        // player id
                        writer.Write((byte)number);
                        // Control
                        writer.Write((byte)0);
                        // Pulley
                        writer.Write((byte)0);
                        // Misc
                        writer.Write((byte)0);
                        // SleepingInfo
                        writer.Write((byte)0);
                        // Selected Item
                        writer.Write((byte)0);

                        writer.Write(number2);
                        writer.Write(number3);

                        writer.Write(number4);
                        writer.Write(number5);

                        break;
                    }
                }

                int length = (int)client.MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);


                if (client.NetMessageSent != null)
                {
                    RawOutgoingPacket packet = new RawOutgoingPacket
                    {
                        WriteBuffer = client.WriteBuffer,
                        Writer = writer,
                        MessageType = messageType,
                        ContinueWithPacket = true
                    };

                    client.NetMessageSent?.Invoke(client, packet);

                    if (!packet.ContinueWithPacket)
                    {
                        return;
                    }
                }

                client.TCPClient.Send(client.WriteBuffer, length);
            }
        }
    }
}