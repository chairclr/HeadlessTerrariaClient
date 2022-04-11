using System;
using HeadlessTerrariaClient;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.ID;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Client;
using HeadlessTerrariaClient.Util;

namespace HeadlessTerrariaClient.Examples
{
    public class SimpleChatClient
    {
        const string ServerIP = "127.0.0.1";
        const int ServerPort = 7777;
        static void Main(string[] args)
        {
            Start();

            System.Threading.Thread.Sleep(-1);
        }

        public static void Start()
        {
            // Create an empty world
            ClientWorld clientWorld = new ClientWorld();

            // Create a new client
            HeadlessClient HeadlessClient = new HeadlessClient();

            // Random client UUID
            HeadlessClient.clientUUID = Guid.NewGuid().ToString();

            // Assaign world reference
            HeadlessClient.World = clientWorld;

            // Name the player
            HeadlessClient.PlayerFile.name = $"ExampleChatClient";  

            // Softcore player
            HeadlessClient.PlayerFile.difficulty = PlayerDifficultyID.SoftCore;

            // Load default player style so we arent some weird white goblin
            HeadlessClient.PlayerFile.LoadDefaultAppearence();

            // This can bypass some anti-cheats that attempt to block headless clients
            HeadlessClient.Settings.AutoSyncPlayerZone = true;

            // Run code when a chat message is recived
            HeadlessClient.ChatMessageRecieved += (HeadlessClient client, ChatMessage message) =>
            {
                // Messages of id 255 are not from another player
                if (message.author != 255)
                {
                    Player sender = client.World.player[message.author];
                    Console.WriteLine($"<{sender.name}> {message.message}");
                }
                else
                {
                    Console.WriteLine(message.message);
                }
            };

            // Connect to a server
            HeadlessClient.Connect(ServerIP, ServerPort);
        }

    }
}
