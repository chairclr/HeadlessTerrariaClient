using System;
using System.IO;
using System.Net;
using ArkNetwork;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Utility;
using HeadlessTerrariaClient.Client;
using System.Net.Sockets;
using System.Numerics;

namespace HeadlessTerrariaClient.Terraria
{
    public class TileManipulationHandler
    {
        /// <summary>
        /// Handles the default action for someone manipulating a tile
        /// </summary>
        public static void Handle(HeadlessClient client, int action, int x, int y, int flags1, int flags2)
        {
            if (!client.IsInWorld || client.Settings.IgnoreTileChunks || !client.Settings.LoadTileSections)
                return;

            if (client.World.CurrentWorld.Tiles[x, y] == null)
            {
                client.World.CurrentWorld.Tiles[x, y] = new Tile();
            }

            Tile tile = client.World.CurrentWorld.Tiles[x, y];

            bool fail = flags1 == 1;

            switch (action)
            {
                case TileManipulationID.KillTile:
                case TileManipulationID.KillTileNoItem:
                    if (!fail)
                    {
                        tile.SetTileActive(false);
                    }
                    break;
                case TileManipulationID.PlaceTile:
                case TileManipulationID.ReplaceTile:
                    tile.SetTileActive(true);
                    tile.tileType = (ushort)flags1;
                    break;
                case TileManipulationID.KillWall:
                    if (!fail)
                    {
                        tile.wallType = 0;
                    }
                    break;
                case TileManipulationID.PlaceWall:
                case TileManipulationID.ReplaceWall:
                    tile.wallType = (ushort)flags1;
                    break;
                case TileManipulationID.SlopeTile:
                    if (!client.World.CurrentWorld.CanPoundTile(x, y))
                        break;
                    tile.SetHalfBrick(false);
                    tile.SetSlopeType((byte)flags1);
                    break;
                case TileManipulationID.SlopePoundTile:
                    if (!client.World.CurrentWorld.CanPoundTile(x, y))
                        break;
                    tile.SetHalfBrick(false);
                    tile.SetSlopeType((byte)flags1);
                    tile.SetHalfBrick(!tile.GetHalfBrick());
                    break;
                case TileManipulationID.KillWire:
                case TileManipulationID.KillWire2:
                case TileManipulationID.KillWire3:
                case TileManipulationID.KillWire4:
                case TileManipulationID.PlaceWire:
                case TileManipulationID.PlaceWire2:
                case TileManipulationID.PlaceWire3:
                case TileManipulationID.PlaceWire4:
                    // Implement wiring later
                    break;
            }
        }
    }
}
