using System;
using HeadlessTerrariaClient;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Client;
using HeadlessTerrariaClient.Utility;
using System.Threading;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria.ID;

namespace HeadlessTerrariaClient.Examples
{
    /// <summary>
    /// Example client that protects the entire world from tile edits
    /// </summary>
    public class ProtectionClient
    {
        const string ServerIP = "127.0.0.1";
        const int ServerPort = 7777;

        const int RateLimit = 500;
        const int RateLimitTimeout = 150;
        int placeRateLimit = 0;

        public async Task Start()
        {
            // Create a new client
            HeadlessClient HeadlessClient = new HeadlessClient();

            // Random client UUID
            HeadlessClient.clientUUID = Guid.NewGuid().ToString();

            // Assaign world reference
            HeadlessClient.World = new ClientWorld();

            // Name the player
            HeadlessClient.LocalPlayer.name = "ExampleProtClient";

            // Softcore player, Default appearence, and Default inventory
            HeadlessClient.LocalPlayer.LoadDefaultPlayer();

            // Run code when someone else manipulates a tile
            HeadlessClient.TileManipulationMessageRecieved += HandleTileManipulation;

            // Connect to a server
            await HeadlessClient.Connect(ServerIP, ServerPort);

            // Wait for client to join the world
            await HeadlessClient.WaitForClientToFinishConnecting();

            // Load every section in the world
            await HeadlessClient.LoadEntireWorld();

            await Task.Delay(Timeout.Infinite);
        }

        // Handle tile manipulation, and do the opposite to protect the world
        public bool HandleTileManipulation(HeadlessClient client, TileManipulation manipulation)
        {
            // When someone breaks a tile, place the same tile back
            if (manipulation.action == TileManipulationID.KillTile 
                || manipulation.action == TileManipulationID.KillTileNoItem 
                || manipulation.action == TileManipulationID.KillTile2)
            {
                // Limit number of actions we can do in an amount of time
                if (placeRateLimit > RateLimit)
                {
                    Task.Delay(250).Wait();
                    placeRateLimit = 0;
                }

                client.SendPlaceTile(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].tileType);
                // Make sure to paint it again
                client.SendPaintTile(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].GetTilePaint());


                placeRateLimit++;

                // Return false so the tile stays the same on client
                return false;
            }

            // When someone breaks a wall, place the same wall back
            if (manipulation.action == TileManipulationID.KillWall)
            {
                // Limit number of actions we can do in an amount of time
                if (placeRateLimit > RateLimit)
                {
                    Task.Delay(RateLimitTimeout).Wait();
                    placeRateLimit = 0;
                }

                client.SendPlaceWall(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].wallType);
                // Make sure to paint it again
                client.SendPaintWall(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].GetWallPaint());


                placeRateLimit++;

                // Return false so the tile stays the same on client
                return false;
            }

            // Break a tile when someone else places it
            if (manipulation.action == TileManipulationID.PlaceTile || manipulation.action == TileManipulationID.ReplaceTile)
            {
                // Limit number of actions we can do in an amount of time
                if (placeRateLimit > 500)
                {
                    Task.Delay(RateLimitTimeout).Wait();
                    placeRateLimit = 0;
                }

                if (client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].GetTileActive())
                {
                    client.SendPlaceTile(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].tileType);
                    // Make sure to paint it again
                    client.SendPaintTile(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].GetTilePaint());
                }
                else
                {
                    client.SendBreakTile(manipulation.tileX, manipulation.tileY);
                }

                placeRateLimit++;

                // Return false so the tile stays the same on client
                return false;
            }

            // Break a wall when someone else places it
            if (manipulation.action == TileManipulationID.PlaceWall)
            {
                // Limit number of actions we can do in an amount of time
                if (placeRateLimit > 500)
                {
                    Task.Delay(250).Wait();
                    placeRateLimit = 0;
                }

                client.SendBreakWall(manipulation.tileX, manipulation.tileY);

                placeRateLimit++;

                // Return false so the tile stays the same on client
                return false;
            }

            // Replace a wall a wall when someone else places it
            if (manipulation.action == TileManipulationID.ReplaceWall)
            {
                // Limit number of actions we can do in an amount of time
                if (placeRateLimit > 500)
                {
                    Task.Delay(250).Wait();
                    placeRateLimit = 0;
                }

                client.SendBreakWall(manipulation.tileX, manipulation.tileY);

                client.SendPlaceWall(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].wallType);
                // Make sure to paint it again
                client.SendPaintWall(manipulation.tileX, manipulation.tileY, client.World.CurrentWorld.tile[manipulation.tileX, manipulation.tileY].GetWallPaint());

                placeRateLimit++;

                // Return false so the tile stays the same on client
                return false;
            }

            return true;
        }
    }
}
