using System;
using HeadlessTerrariaClient;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Client;
using HeadlessTerrariaClient.Util;
using System.Threading;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria.ID;

namespace HeadlessTerrariaClient.Examples
{
    public static class Program
    {
        static void Main(string[] args)
        {
            SimpleChatClient chatClient = new SimpleChatClient();

            chatClient.Start().Wait();
        }
    }
    public class SimpleChatClient
    {
        const string ServerIP = "127.0.0.1";
        const int ServerPort = 7777;
        

        public async Task Start()
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
            HeadlessClient.PlayerFile.LoadDefaultInventory();

            // This can bypass some anti-cheats that attempt to block headless clients
            HeadlessClient.Settings.AutoSyncPlayerZone = true;

            // Run code when a chat message is recived
            HeadlessClient.ChatMessageRecieved += async (HeadlessClient client, ChatMessage message) =>
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
            await HeadlessClient.Connect(ServerIP, ServerPort);

            await Task.Delay(Timeout.Infinite);
        }
    }
}
