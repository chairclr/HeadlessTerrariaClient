using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Util;
using System.Net.Sockets;
using System.Numerics;
using HeadlessTerrariaClient.Client;

namespace HeadlessTerrariaClient.Util
{
    public static class ClientExtensions
    {
        public static void SendChatMessage(this HeadlessClient client, string msg)
        {
            lock (client.WriteBuffer)
            {
                BinaryWriter writer = client.MessageWriter;

                writer.Seek(2, SeekOrigin.Begin);

                writer.Write((byte)MessageID.NetModules);

                // module type
                writer.Write((ushort)1);

                // NetworkText mode
                writer.Write((byte)0);

                // text
                writer.Write(msg);

                int length = (int)client.MemoryStreamWrite.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((short)length);

                client.TCPClient.Send(client.WriteBuffer, length);
            }
        }
        public static int FindPlayerByName(this HeadlessClient client, string name)
        {
            for (int i = 0; i < 255; i++)
            {
                if (client.World.player[i].active && client.World.player[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void SendBreakTile(this HeadlessClient client, int tileX, int tileY)
        {
            client.SendData(MessageID.TileManipulation, TileManipulationID.KillTileNoItem, tileX, tileY);
        }
        public static void SendPlaceTile(this HeadlessClient client, int tileX, int tileY, int type)
        {
            client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceTile, tileX, tileY, type);
        }
        public static void SendPlaceTile_TShockBypass(this HeadlessClient client, int tileX, int tileY, int type)
        {
            client.CustomSendData(MessageID.PlayerControls, client.myPlayer, tileX * 16f, tileY * 16f);
            client.CustomSendData(MessageID.SyncEquipment, client.myPlayer, 0, 1, 0, BlockTypeItem.TileToItem[type]);

            client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceTile, tileX, tileY, type);

            client.SendData(MessageID.PlayerControls, client.myPlayer);
            client.SendData(MessageID.SyncEquipment, client.myPlayer, 0);
        }

        public static void SendBreakWall(this HeadlessClient client, int tileX, int tileY)
        {
            client.SendData(MessageID.TileManipulation, TileManipulationID.KillWall, tileX, tileY);
        }
        public static void SendPlaceWall(this HeadlessClient client, int tileX, int tileY, int type)
        {
            client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceWall, tileX, tileY, type);
        }
        public static void SendPlaceWall_TShockBypass(this HeadlessClient client, int tileX, int tileY, int type)
        {
            client.CustomSendData(MessageID.PlayerControls, client.myPlayer, tileX * 16f, tileY * 16f);
            client.CustomSendData(MessageID.SyncEquipment, client.myPlayer, 0, 1, 0, BlockTypeItem.WallToItem[type]);

            client.SendData(MessageID.TileManipulation, TileManipulationID.PlaceWall, tileX, tileY, type);

            client.SendData(MessageID.PlayerControls, client.myPlayer);
            client.SendData(MessageID.SyncEquipment, client.myPlayer, 0);
        }

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
                        // player id
                        writer.Write((byte)number);

                        // slot
                        writer.Write((short)number2);

                        // stack
                        writer.Write((short)number3);

                        // prefix
                        writer.Write((byte)number4);

                        // type
                        if (number5 == -1)
                            writer.Write((short)0);
                        else
                            writer.Write((short)number5);
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
                    RawOutgoingPacket packet = new RawOutgoingPacket();
                    packet.WriteBuffer = client.WriteBuffer;
                    packet.Writer = writer;
                    packet.MessageType = messageType;
                    packet.ContinueWithPacket = true;

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